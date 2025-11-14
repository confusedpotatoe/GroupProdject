using System.Runtime.InteropServices;
using BrickBreaker.Game;
using BrickBreaker.Logic;
using BrickBreaker.Models;
using BrickBreaker.Storage;
using BrickBreaker.Ui;

enum AppState { LoginMenu, GameplayMenu, Playing, Exit }

class Program
{
    static string? currentUser = null;
    private static LeaderboardStore _lbStore;
    private static Leaderboard _lb;

    static Auth _auth = null!; // set in Main

    // UI (console implementations for now)
    static ILoginMenu _loginMenu = new ConsoleLoginMenu();
    static IGameplayMenu _gameplayMenu = new ConsoleGameplayMenu();
    static IConsoleDialogs _dialogs = new ConsoleDialogs();

    static void Main()
    {
        var paths = new FilePathProvider();

        var userStore = new UserStore(paths.GetUserPath());
        _auth = new Auth(userStore);
        // Initialize storage/logic once

        _lbStore = new LeaderboardStore(paths.GetLeaderboardPath());
        _lb = new Leaderboard(_lbStore);


        _auth = new Auth(userStore);

        AppState state = AppState.LoginMenu;

        while (state != AppState.Exit)
        {
            switch (state)
            {
                case AppState.LoginMenu:
                    state = HandleLoginMenu();
                    break;

                case AppState.GameplayMenu:
                    state = HandleGameplayMenu();
                    break;

                case AppState.Playing:
                    IGame game = new BrickBreakerGame();
                    int score = game.Run();
                    int lowerLine = Console.WindowHeight - 4; // a few lines from bottom
                    Console.SetCursorPosition(0, lowerLine);
                    _dialogs.ShowMessage($"\nFinal score: {score}");
                    _lb.Submit(currentUser ?? "guest", score);
                    _dialogs.Pause();

                    state = currentUser is null ? AppState.LoginMenu : AppState.GameplayMenu;
                    break;
            }
        }
    }

    static AppState HandleLoginMenu()
    {
        ClearInputBuffer();
        var choice = _loginMenu.Show();

        switch (choice)
        {
            case LoginMenuChoice.Register:
                DoRegister();
                _dialogs.Pause();
                return AppState.LoginMenu;

            case LoginMenuChoice.Login:
                if (DoLogin()) return AppState.GameplayMenu;
                _dialogs.Pause();
                return AppState.LoginMenu;

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

    static AppState HandleGameplayMenu()
    {
        ClearInputBuffer();
        var choice = _gameplayMenu.Show(currentUser ?? "guest");



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
        if (top.Count == 0)
        {
            _dialogs.ShowMessage("\nTop 10 leaderboard:\nNo scores yet.");
            return;
        }
        var items = top.Select(s => (s.Username, s.Score, s.At));
        _dialogs.ShowLeaderboard(items);
    }

    //  Input buffer helper
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

    // Shows the current user's highest score
    static void ShowBestScore()
    {
        var best = _lb.BestFor(currentUser!);

        if (best == null)
        {
            _dialogs.ShowMessage("\nNo scores recorded yet.");
        }
        else
        {
            // Convert stored UTC to the local time zone for display
            _dialogs.ShowMessage(
                $"\nYour best score: {best.Score} on {best.At.ToLocalTime():yyyy-MM-dd HH:mm}"
            );
        }

        _dialogs.Pause();
    }

}
