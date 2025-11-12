// Program.cs — minimal, runnable, with Auth/Leaderboard singletons and UI split points

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

    // Singletons for the whole app
    static readonly LeaderboardStore _lbStore = new("data/leaderboard.json");
    static readonly Leaderboard _lb = new(_lbStore);

    static Auth _auth = null!; // set in Main

    // UI (console implementations for now)
    static ILoginMenu _loginMenu = new ConsoleLoginMenu();
    static IGameplayMenu _gameplayMenu = new ConsoleGameplayMenu();

    static void Main()
    {
        // Initialize storage/logic once
        var userStore = new UserStore("data/users.json");
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
                    Console.WriteLine($"\nFinal score: {score}");

                    _lb.Submit(currentUser ?? "guest", score);

                    Pause();
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
                Pause();
                return AppState.LoginMenu;

            case LoginMenuChoice.Login:
                if (DoLogin()) return AppState.GameplayMenu;
                Pause();
                return AppState.LoginMenu;

            case LoginMenuChoice.Leaderboard:
                ShowLeaderboard();
                Pause();
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
                // TODO: _lb.BestFor(currentUser!)
                Console.WriteLine("\n[TODO] Show your best score via Leaderboard.BestFor(username).");
                Pause();
                return AppState.GameplayMenu;

            case GameplayMenuChoice.Leaderboard:
                ShowLeaderboard();
                Pause();
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
        Console.Write("\nChoose a username: ");
        var username = Console.ReadLine();

        Console.Write("Choose a password: ");
        var password = Console.ReadLine();

        bool ok = _auth.Register(username, password);
        Console.WriteLine(ok ? "Registration successful! You can now log in." :
                               "Registration failed (empty or already exists).");
    }

    static bool DoLogin()
    {
        Console.WriteLine();
        Console.Write("Username: ");
        var username = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        if (_auth.Login(username, password))
        {
            currentUser = (username ?? "").Trim();
            return true;
        }

        Console.WriteLine("Login failed (wrong username or password).");
        return false;
    }

    static void ShowLeaderboard()
    {
        Console.WriteLine("\nTop 10 leaderboard:");
        var top = _lb.Top(10);
        if (top.Count == 0)
        {
            Console.WriteLine("No scores yet.");
            return;
        }
        int i = 1;
        foreach (var s in top)
            Console.WriteLine($"{i++}. {s.Username}  {s.Score}  {s.At:yyyy-MM-dd HH:mm}");
    }

    static void Pause()
    {
        ClearInputBuffer();
        Console.WriteLine("\nPress Enter to continue...");
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) break;
        }
    }

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
