using BrickBreaker.Game;
using BrickBreaker.Logic;
using BrickBreaker.Models;
using BrickBreaker.Storage;
using BrickBreaker.Ui;
using BrickBreaker.UI.Ui;
using BrickBreaker.UI.Ui.Enums;
using BrickBreaker.UI.Ui.Interfaces;
using BrickBreaker.UI.Ui.SpecterConsole;
using Spectre.Console;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
class Program
{
    static string? currentUser = null;
    private static Leaderboard _lb = null!;
    private static Auth _auth = null!;
    private static bool _databaseAvailable = false;

    // UI menus and dialogs
    static ILoginMenu _loginMenu = new LoginMenu();
    static IGameplayMenu _gameplayMenu = new GameplayMenu();
    static IConsoleDialogs _dialogs = new ConsoleDialogs();
    static GameMode currentMode = GameMode.Normal;

    static Header header = new Header();

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var storageConfig = new StorageConfiguration();
        var connectionString = storageConfig.GetConnectionString();

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            var userStore = new UserStore(connectionString);
            var leaderboardStore = new LeaderboardStore(connectionString);

            _lb = new Leaderboard(leaderboardStore);
            _auth = new Auth(userStore);
            _databaseAvailable = true;
        }
        else
        {
            _lb = new Leaderboard(new DisabledLeaderboardStore());
            _auth = new Auth(new DisabledUserStore());
            _databaseAvailable = false;
            ShowDatabaseWarning("Supabase connection string missing. Database features are disabled until it is configured.");
        }

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

    // =========================
    // Login Menu Handler
    // =========================
    static AppState HandleLoginMenu()
    {
        var choice = _loginMenu.Show();

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

    // =========================
    // Gameplay Menu Handler
    // =========================
    static AppState HandleGameplayMenu()
    {
        GameplayMenuChoice choice = _gameplayMenu.Show(currentUser ?? "guest");

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

    // =========================
    // Playing Handler
    // =========================
    static AppState HandlePlaying()
    {
        AnsiConsole.Clear();
        IGame game = new BrickBreakerGame();
        int score = game.Run();

        int lowerLine = Console.WindowHeight - 4;
        Console.SetCursorPosition(0, lowerLine);

        _dialogs.ShowMessage($"\nFinal score: {score}");
        if (currentMode != GameMode.QuickPlay)
        {
            if (_databaseAvailable)
            {
                _lb.Submit(currentUser ?? "guest", score);
            }
            else
            {
                ShowDatabaseWarning("Unable to submit scores without the Supabase connection string. Your score was not saved.");
            }
        }

        _dialogs.Pause();

        return currentUser is null ? AppState.LoginMenu : AppState.GameplayMenu;
    }

    // =========================
    // Helper methods
    // =========================

    static void DoRegister()
    {
        if (!_databaseAvailable)
        {
            ShowDatabaseWarning("Registration is disabled because the Supabase connection string is missing.");
            return;
        }

        var username = _dialogs.PromptNewUsername();

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

        var password = _dialogs.PromptNewPassword();

        bool ok = _auth.Register(username, password);
        _dialogs.ShowMessage(ok
            ? "Registration successful! You can now log in."
            : "Registration failed (empty or already exists).");
    }

    static bool DoLogin()
    {
        if (!_databaseAvailable)
        {
            ShowDatabaseWarning("Login requires the Supabase database. Please configure the connection string first.");
            return false;
        }

        var (username, password) = _dialogs.PromptCredentials();

        if (_auth.Login(username, password))
        {
            currentUser = username;

            // === Loading bar AFTER successful login ===
            AnsiConsole.Progress()
                .Columns(
                    new ProgressColumn[]
                    {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn()
                    })
                .Start(ctx =>
                {
                    var verifyTask = ctx.AddTask("[yellow]Verifying user[/]");
                    var loadTask = ctx.AddTask("[green]Loading game data[/]");

                    while (!ctx.IsFinished)
                    {
                        verifyTask.Increment(5);
                        loadTask.Increment(4);
                        Thread.Sleep(40);
                    }
                });

            return true;
        }

        _dialogs.ShowMessage("Login failed (wrong username or password).");
        return false;
    }

    static void ShowLeaderboard()
    {
        AnsiConsole.Progress()
        .Columns(new ProgressColumn[]
        {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn()
        })
    .Start(ctx =>
        {
            var task = ctx.AddTask("[green]Loading Scores[/]", maxValue: 100);
            while (!ctx.IsFinished)
            {
                task.Increment(4);
                Thread.Sleep(40);
            }
        });


        AnsiConsole.Clear();
        header.TitleHeader();

        if (!_databaseAvailable)
        {
            ShowDatabaseWarning("Leaderboard is unavailable because the Supabase connection string is missing.");
            return;
        }

        var top = _lb.Top(10);
        if (!top.Any())
        {
            _dialogs.ShowMessage("\nTop 10 leaderboard:\nNo scores yet.");
            return;
        }

        var items = top.Select(s => (s.Username, s.Score, s.At));
        _dialogs.ShowLeaderboard(items);
    }

    static void ShowBestScore()
    {
        if (!_databaseAvailable)
        {
            ShowDatabaseWarning("Best score lookup requires the Supabase database. Please configure the connection string first.");
            return;
        }

        var best = _lb.BestFor(currentUser!);

        if (best == null)
        {
            _dialogs.ShowMessage("\nNo scores recorded yet.");
        }
        else
        {
            _dialogs.ShowMessage(
                $"\nYour best score: {best.Score} on {best.At.ToLocalTime():yyyy-MM-dd HH:mm}"
            );
        }

        _dialogs.Pause();
    }

    // =========================
    // Input buffer helper
    // =========================
    const int STD_INPUT_HANDLE = -10;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool FlushConsoleInputBuffer(IntPtr hConsoleInput);

    static void ClearInputBuffer()
    {
        while (Console.KeyAvailable)
            Console.ReadKey(true);

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

    static void ShowDatabaseWarning(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        _dialogs.ShowMessage(message);
        Console.ForegroundColor = previousColor;
    }
}
