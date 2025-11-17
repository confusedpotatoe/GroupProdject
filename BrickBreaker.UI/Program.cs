using BrickBreaker.Game;
using BrickBreaker.Logic;
using BrickBreaker.Storage;
using BrickBreaker.Ui;
using BrickBreaker.UI.Ui.Enums;
using BrickBreaker.UI.Ui.Interfaces;
using BrickBreaker.UI.Ui.SpecterConsole;
using System.Linq;
using System.Runtime.InteropServices;

class Program
{
    static string? currentUser = null ;
    private static Leaderboard _lb = null!;
    private static Auth _auth = null!;

    // UI menus and dialogs
    static ILoginMenu _loginMenu = new LoginMenu();
    static IGameplayMenu _gameplayMenu = new GameplayMenu();
    static IConsoleDialogs _dialogs = new ConsoleDialogs();

    static void Main()
    {
        var storageConfig = new StorageConfiguration();
        var connectionString = storageConfig.GetConnectionString();

        var userStore = new UserStore(connectionString);
        var leaderboardStore = new LeaderboardStore(connectionString);

        _lb = new Leaderboard(leaderboardStore);
        _auth = new Auth(userStore);

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
                return AppState.GameplayMenu;
        }
    }

    // =========================
    // Playing Handler
    // =========================
    static AppState HandlePlaying()
    {
        IGame game = new BrickBreakerGame();
        int score = game.Run();

        int lowerLine = Console.WindowHeight - 4;
        Console.SetCursorPosition(0, lowerLine);

        _dialogs.ShowMessage($"\nFinal score: {score}");
        _lb.Submit(currentUser ?? "guest", score);
        _dialogs.Pause();

        return currentUser is null ? AppState.LoginMenu : AppState.GameplayMenu;
    }

    // =========================
    // Helper methods
    // =========================
    static void DoRegister()
    {
        var username = _dialogs.PromptNewUsername();
        var password = _dialogs.PromptNewPassword();

        bool ok = _auth.Register(username, password);
        _dialogs.ShowMessage(ok
            ? "Registration successful! You can now log in."
            : "Registration failed (empty or already exists).");
    }

    static bool DoLogin()
    {
        var (username, password) = _dialogs.PromptCredentials();

        if (_auth.Login(username, password))
        {
            currentUser = username;
            return true;
        }

        _dialogs.ShowMessage("Login failed (wrong username or password).");
        return false;
    }

    static void ShowLeaderboard()
    {
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
}
