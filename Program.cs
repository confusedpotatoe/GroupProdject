// Temporary version
// Keeps current “run the game and print score” flow via menu option 1.
using BrickBreaker.Models;
using BrickBreaker.Game;
using BrickBreaker.Logic;
using BrickBreaker.Storage;


enum AppState { LoginMenu, GameplayMenu, Playing, Exit }


class Program
{
    static string? currentUser = null; // TODO: set after Login

    private static readonly LeaderboardStore _lbStore = new("../../../data/leaderboard.json");
    private static readonly Leaderboard _lb = new(_lbStore);
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
        Console.Clear();
        Console.WriteLine("=== Main Menu ===");

        Console.WriteLine("1) Register");
        Console.WriteLine("2) Login");
        Console.WriteLine("3) Leaderboard (view top 10)");
        Console.WriteLine("4) Exit");
        Console.Write("Choose: ");
        var key = Console.ReadKey(true).KeyChar;

        switch (key)
        {
            /*case '1':
                // Quick Play path: no auth, just run the game
                currentUser = null;
                return AppState.Playing;*/

            case '1':

                Console.Write("\nChoose a username: ");
                var username = Console.ReadLine();

                Console.Write("Choose a password: ");
                var password = Console.ReadLine();

                bool ok = _auth.Register(username, password);
                Console.WriteLine(ok ? "Registration successful!" : "Registration failed.");
                Pause();
                return AppState.LoginMenu;

            case '2':
                {
                    Console.Write("Username: ");
                    username = Console.ReadLine();

                    Console.Write("Password: ");
                    password = Console.ReadLine();

                    if (_auth.Login(username, password))
                    {
                        currentUser = username;
                        return AppState.GameplayMenu;
                    }
                    else
                    {
                        Console.WriteLine("Login failed (wrong username or password).");
                        Pause();
                        return AppState.LoginMenu;
                    }
                }

            case '3':

                Console.WriteLine("\nTop 10 leaderboard: ");
                var top = _lb.Top(10);
                foreach (var item in top)
                {
                    Console.WriteLine($"{item.Username} - {item.Score} - {item.At}");
                }

                Pause();
                return AppState.LoginMenu;

            case '4':
                return AppState.Exit;

            default:
                return AppState.LoginMenu;
        }
    }

    static AppState HandleGameplayMenu()
    {
        string path = Path.Combine("..", "..", "..", "data", "users.json");

        Console.Clear();
        Console.WriteLine($"=== Gameplay Menu (user: {currentUser ?? "guest"}) ===");
        Console.WriteLine("1) Start");
        Console.WriteLine("2) High Score (your best) (TODO: Leaderboard.BestFor)");
        Console.WriteLine("3) Leaderboard (top 10)");
        Console.WriteLine("4) Logout");
        Console.Write("Choose: ");
        var key = Console.ReadKey(true).KeyChar;

        switch (key)
        {
            case '1':
                return AppState.Playing;

            case '2':
                Console.WriteLine("\n[TODO] Show your best score via Leaderboard.BestFor(username).");

                Pause();
                return AppState.GameplayMenu;

            case '3':
                Console.WriteLine("\nTop 10 leaderboard: ");
                var top = _lb.Top(10);
                foreach (var item in top)
                {
                    Console.WriteLine($"{item.Username} - {item.Score} - {item.At}");
                }

                Pause();
                return AppState.GameplayMenu;

            case '4':
                currentUser = null;
                return AppState.LoginMenu;

            default:
                return AppState.GameplayMenu;
        }
    }

    static void Pause()
    {
        Console.WriteLine("\nPress any key...");
        Console.ReadKey(true);
    }
}
