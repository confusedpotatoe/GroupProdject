// Temporary version
// Keeps current “run the game and print score” flow via menu option 1.
using BrickBreaker.Game;
using BrickBreaker.Logic;
using BrickBreaker.Models;
using BrickBreaker.Storage;
using BrickBreaker.Ui;


enum AppState { LoginMenu, GameplayMenu, Playing, Exit }


class Program
{
    static string? currentUser = null; // TODO: set after Login

    private static readonly LeaderboardStore _lbStore = new("../../../data/leaderboard.json");
    private static readonly Leaderboard _lb = new(_lbStore);

    static ILoginMenu _loginMenu = new ConsoleLoginMenu();

    static IGameplayMenu _gameplayMenu = new ConsoleGameplayMenu();

    static void Main()
    {
        //These are created once and reused for the entire program lifetime

        string userFilePath = Path.Combine("data", "users.json");
        var userStore = new UserStore(userFilePath);
        _auth = new Auth(userStore);  // Initialize here

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

                    // Play the game
                    IGame game = new BrickBreakerGame();
                    int score = game.Run();
                    Console.WriteLine($"\nFinal score: {score}");
                    //Save result to leaderboard using the shared instance
                    _lb.Submit(currentUser ?? "guest", score);

                    Pause();
                    state = currentUser is null ? AppState.LoginMenu : AppState.GameplayMenu;
                    break;
            }
        }
    }
    static Auth _auth;

    static AppState HandleLoginMenu()
    {
        var choice = _loginMenu.Show();  

        switch (choice)
        {
            case LoginMenuChoice.Register:
                DoRegister();
                Pause();
                return AppState.LoginMenu;

            case LoginMenuChoice.Login:
                if (DoLogin())
                    return AppState.GameplayMenu;
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
        var choice = _gameplayMenu.Show(currentUser ?? "guest");

        switch (choice)
        {
            case GameplayMenuChoice.Start:
                return AppState.Playing;

            case GameplayMenuChoice.Best:
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
        string path = Path.Combine("data", "users.json");
        var userStore = new UserStore(path);
        var user = new User();

        Console.Write("\nChoose a username: ");
        user.Username = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Choose a password: ");
        user.Password = Console.ReadLine()?.Trim() ?? "";

        if (userStore.Exists(user.Username))
        {
            Console.WriteLine("Username already exists. Please choose another one.");
            return;
        }

        userStore.Add(user);
        Console.WriteLine("Registration successful! You can now log in.");
    }

    static bool DoLogin()
    {
        Console.Write("Username: ");
        string username = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Password: ");
        string password = Console.ReadLine()?.Trim() ?? "";

        if (auth.Login(username, password))
        {
            currentUser = username;
            return true;
        }

        Console.WriteLine("Login failed (wrong username or password).");
        return false;
    }

    static void ShowLeaderboard()
    {
        Console.WriteLine("\nTop 10 leaderboard: ");
        var top = _lb.Top(10);
        foreach (var item in top)
        {
            Console.WriteLine($"{item.Username} - {item.Score} - {item.At}");
        }
    }

    static void Pause()
    {
        Console.WriteLine("\nPress any key...");
        Console.ReadKey(true);
    }
}
