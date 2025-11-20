using BrickBreaker.Game;
using BrickBreaker.Logic;
using BrickBreaker.Storage;
using BrickBreaker.Ui;
using BrickBreaker.UI.Ui.Enums;
using BrickBreaker.UI.Ui.Interfaces;
using BrickBreaker.UI.Ui.SpecterConsole;
using Spectre.Console;
using System.Runtime.InteropServices;
using System.Text;
class Program
{
    // Application states
    static string? currentUser = null;
    // Database and authentication
    private static Leaderboard _lb = null!;
    // Authentication manager
    private static Auth _auth = null!;
    // Database availability flag
    private static bool _databaseAvailable = false;

    // UI menus and dialogs 
    static ILoginMenu _loginMenu = new LoginMenu();
    static IGameplayMenu _gameplayMenu = new GameplayMenu();
    static IConsoleDialogs _dialogs = new ConsoleDialogs();
    static GameMode currentMode = GameMode.Normal;

    // Header instance for displaying titles
    static Header header = new Header();

    // Application entry point
    static void Main()
    {
        // Ensure UTF-8 encoding for console output
        Console.OutputEncoding = Encoding.UTF8;

        // Load storage configuration
        var storageConfig = new StorageConfiguration();
        // Get the connection string for the database
        var connectionString = storageConfig.GetConnectionString();

        // Initialize database stores based on the connection string
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            // Initialize user and leaderboard stores
            var userStore = new UserStore(connectionString);
            var leaderboardStore = new LeaderboardStore(connectionString);

            // Initialize leaderboard and authentication managers
            _lb = new Leaderboard(leaderboardStore);
            // Initialize authentication manager
            _auth = new Auth(userStore);
            // Set database availability flag
            _databaseAvailable = true;
        }
        else
        {
            // Fallback to disabled stores if connection string is missing
            _lb = new Leaderboard(new DisabledLeaderboardStore());
            // Initialize authentication manager with disabled user store
            _auth = new Auth(new DisabledUserStore());
            // Set database availability flag
            _databaseAvailable = false;
            // Show warning about missing database configuration
            ShowDatabaseWarning("Supabase connection string missing. Database features are disabled until it is configured.");
        }

        // Main application loop
        AppState state = AppState.LoginMenu;

        while (state != AppState.Exit)
        {
            state = state switch
            {
                AppState.LoginMenu => HandleLoginMenu(),
                AppState.GameplayMenu => HandleGameplayMenu(),
                AppState.Playing => HandlePlaying(),
                _ => AppState.Exit
            };
        }
    }

    // Login Menu Handler
    static AppState HandleLoginMenu()
    {
        // Show the login menu and get the user's choice
        var choice = _loginMenu.Show();

        // Handle the user's choice
        switch (choice)
        {
            case LoginMenuChoice.QuickPlay:
                ClearInputBuffer();
                currentMode = GameMode.QuickPlay;
                return AppState.Playing;
            case LoginMenuChoice.Register:
                DoRegister();
                _dialogs.Pause();
                return AppState.LoginMenu;

            case LoginMenuChoice.Login:
                return DoLogin() ? AppState.GameplayMenu : AppState.LoginMenu;

            case LoginMenuChoice.Leaderboard:
                ShowLeaderboard();
                _dialogs.Pause();
                return AppState.LoginMenu;

            case LoginMenuChoice.Exit:
                return AppState.Exit;

            default:
                return AppState.LoginMenu;
        }
    }


    // Gameplay Menu Handler
    static AppState HandleGameplayMenu()
    {
        // Show the gameplay menu and get the user's choice
        GameplayMenuChoice choice = _gameplayMenu.Show(currentUser ?? "guest");

        // Handle the user's choice
        switch (choice)
        {
            case GameplayMenuChoice.Start:
                currentMode = GameMode.Normal;
                return AppState.Playing;

            case GameplayMenuChoice.Best:
                ShowBestScore();
                return AppState.GameplayMenu;

            case GameplayMenuChoice.Leaderboard:
                ShowLeaderboard();
                _dialogs.Pause();
                return AppState.GameplayMenu;

            case GameplayMenuChoice.Logout:
                currentUser = null;
                return AppState.LoginMenu;

            default:
                return AppState.Exit;
        }
    }

    // Playing Handler
    static AppState HandlePlaying()
    {
        AnsiConsole.Clear();
        // Display game title header
        IGame game = new BrickBreakerGame();
        // Run the game and get the final score
        int score = game.Run();

        // Move cursor to lower part of the console
        int lowerLine = Console.WindowHeight - 4;
        // Ensure we don't move the cursor above the current line
        Console.SetCursorPosition(0, lowerLine);

        // Show final score message
        _dialogs.ShowMessage($"\nFinal score: {score}");
        if (currentMode != GameMode.QuickPlay)
        {
            // Submit score to leaderboard if database is available
            if (_databaseAvailable)
            {
                // Submit score
                _lb.Submit(currentUser ?? "guest", score);
            }
            else
            {
                // Show warning if unable to submit score
                ShowDatabaseWarning("Unable to submit scores without the Supabase connection string. Your score was not saved.");
            }
        }

        _dialogs.Pause();

        // Return to appropriate menu based on user login status
        return currentUser is null ? AppState.LoginMenu : AppState.GameplayMenu;
    }

    // Helper methods
    static void DoRegister()
    {
        // Check database availability
        if (!_databaseAvailable)
        {
            // Show warning if registration is disabled
            ShowDatabaseWarning("Registration is disabled because the Supabase connection string is missing.");
            return;
        }

        // Prompt for new username
        var username = _dialogs.PromptNewUsername();

        // Trim whitespace from username
        username = (username ?? "").Trim();

        // Checks so username is not empty 
        if (username.Length == 0)
        {
            _dialogs.ShowMessage("Username can't be empty.");
            return;
        }

        // Checks so username don´t already exists
        if (_auth.UsernameExists(username))
        {
            _dialogs.ShowMessage("Username already exists.");
            return;
        }

        // Prompt for new password
        var password = _dialogs.PromptNewPassword();

        // Attempt to register the new user/ add new user to the database
        bool ok = _auth.Register(username, password);
        _dialogs.ShowMessage(ok
            ? "Registration successful! You can now log in."
            : "Registration failed (empty or already exists).");
    }

    // Login helper method
    static bool DoLogin()
    {
        // Check database availability
        if (!_databaseAvailable)
        {
            // Show warning if login is disabled
            ShowDatabaseWarning("Login requires the Supabase database. Please configure the connection string first.");
            return false;
        }

        // Prompt for username and password
        var (username, password) = _dialogs.PromptCredentials();

        // Attempt to log in the user
        if (_auth.Login(username, password))
        {
            currentUser = username;

            //Loading bar animation AFTER successful login
            AnsiConsole.Progress()
                // Define progress columns
                .Columns(
                    new ProgressColumn[]
                    {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn()
                    })
                // Start the progress display
                .Start(ctx =>
                {
                    var verifyTask = ctx.AddTask("[yellow]Verifying user[/]");
                    var loadTask = ctx.AddTask("[green]Loading game data[/]");

                    // Simulate progress until tasks are finished
                    while (!ctx.IsFinished)
                    {
                        verifyTask.Increment(5);
                        loadTask.Increment(4);
                        Thread.Sleep(40);
                    }
                });

            return true;
        }

        // Show login failure message
        _dialogs.ShowMessage("Login failed (wrong username or password).");
        return false;
    }

    // Leaderboard display method
    static void ShowLeaderboard()
    {
        // Loading bar animation for leaderboard loading
        AnsiConsole.Progress()
        .Columns(new ProgressColumn[]
        {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn()
        })

        // Start the progress display
        .Start(ctx =>
            {
                // loading task for leaderboard and diseperaring after reatching 100 and sleeping 40ms
                var task = ctx.AddTask("[green]Loading Scores[/]", maxValue: 100);
                while (!ctx.IsFinished)
                {
                    task.Increment(4);
                    Thread.Sleep(40);
                }
            });


        AnsiConsole.Clear();

        // Display leaderboard title header
        header.TitleHeader();

        // Check database availability
        if (!_databaseAvailable)
        {
            ShowDatabaseWarning("Leaderboard is unavailable because the Supabase connection string is missing.");
            return;
        }

        // Retrieve top 10 leaderboard entries
        var top = _lb.Top(10);
        // Check if there are any scores to display 
        if (!top.Any())
        {
            // Show message if no scores are recorded
            _dialogs.ShowMessage("\nTop 10 leaderboard:\nNo scores yet.");
            return;
        }

        // Prepare leaderboard items for display
        var items = top.Select(s => (s.Username, s.Score, s.At));
        // Show the leaderboard entries
        _dialogs.ShowLeaderboard(items);
    }

    // Best score display method
    static void ShowBestScore()
    {
        // Check database availability
        if (!_databaseAvailable)
        {
            // Show warning if best score lookup is disabled
            ShowDatabaseWarning("Best score lookup requires the Supabase database. Please configure the connection string first.");
            return;
        }

        // Retrieve the best score for the current user
        var best = _lb.BestFor(currentUser!);

        // Display the best score or a message if no scores are recorded
        if (best == null)
        {
            // Show message if no scores are recorded
            _dialogs.ShowMessage("\nNo scores recorded yet.");
        }
        else
        {
            // Show the best score details
            _dialogs.ShowMessage(
                $"\nYour best score: {best.Score} on {best.At.ToLocalTime():yyyy-MM-dd HH:mm}"
            );
        }
        // Pause to allow user to read the message
        _dialogs.Pause();
    }

    // Input buffer helper
    const int STD_INPUT_HANDLE = -10;

    // gets the standard input handle for the console
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);


    // clears the console input buffer, to avoid leftover key presses affecting subsequent input
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool FlushConsoleInputBuffer(IntPtr hConsoleInput);

    // Clears any pending input in the console input buffer
    static void ClearInputBuffer()
    {
        while (Console.KeyAvailable)
            Console.ReadKey(true);

        // Attempt to flush the console input buffer on Windows
        try
        {
            var handle = GetStdHandle(STD_INPUT_HANDLE);
            if (handle != IntPtr.Zero)
                FlushConsoleInputBuffer(handle);
        }
        catch
        {
            // ignored: console might not be Windows or handle unavailable
        }
    }

    // Database warning display method
    static void ShowDatabaseWarning(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        _dialogs.ShowMessage(message);
        Console.ForegroundColor = previousColor;
    }
}
