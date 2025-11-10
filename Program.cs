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

    private static readonly LeaderboardStore _lbStore = new("data/leaderboard.json");
    private static readonly Leaderboard _lb = new(_lbStore);
    static void Main()
    {
        //These are created once and reused for the entire program lifetime
        
        string userFilePath = Path.Combine("data", "users.json");
        var userStore = new UserStore(userFilePath);
        auth = new Auth(userStore);  // Initialize here

        AppState state = AppState.LoginMenu;

        while (state != AppState.Exit)

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
    static Auth auth;

    static AppState HandleLoginMenu()
    {
        Console.Clear();
        Console.WriteLine("=== Main Menu ===");
        Console.WriteLine("1) Quick Play (runs game now)");
        Console.WriteLine("2) Register   (TODO: Logic/Auth + Storage/UserStore)");
        Console.WriteLine("3) Login      (TODO: Logic/Auth + Storage/UserStore)");
        Console.WriteLine("4) Leaderboard (view top 10) (TODO: Logic/Leaderboard + Storage/LeaderboardStore)");
        Console.WriteLine("5) Exit");
        Console.Write("Choose: ");
        var key = Console.ReadKey(true).KeyChar;

        switch (key)
        {
            case '1':
                // Quick Play path: no auth, just run the game
                currentUser = null;
                return AppState.Playing;

            case '2':
                // Register new user
                string path = Path.Combine("..", "..", "..", "data", "users.json");
                var userStore = new UserStore(path);
                User user = new User();

                // Get username and password
                Console.Write("\nChoose a username: ");
                user.Username = Console.ReadLine()?.Trim() ?? "";
                Console.Write("Choose a password: ");
                user.Password = Console.ReadLine()?.Trim() ?? "";

                // Check if username already exists
                if (userStore.Exists(user.Username))
                {
                    Console.WriteLine("Username already exists. Please choose another one.");
                    Pause();
                    return AppState.LoginMenu;
                }

                // Add user to store
                userStore.Add(user);

                // Confirm registration
                Console.WriteLine("Registration successful! You can now log in.");
                Pause();
                return AppState.LoginMenu;

            case '3':
                {
                    var freshPath = Path.Combine("..", "..", "..", "data", "users.json");
                    auth = new Auth(new UserStore(freshPath));


                    Console.Write("Username: ");
                    string username = Console.ReadLine()?.Trim() ?? "";

                    Console.Write("Password: ");
                    string password = Console.ReadLine()?.Trim() ?? "";

                    if (auth.Login(username, password))
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

            case '4':

                // TODO: show Top 10 via Logic/Leaderboard.Top(10)
                Console.WriteLine("\n[TODO] Leaderboard: implement Logic/Leaderboard + Storage/LeaderboardStore.");
                Pause();
                return AppState.LoginMenu;

            case '5':
                return AppState.Exit;

            default:
                return AppState.LoginMenu;
        }
    }

    static AppState HandleGameplayMenu()
    {
        Console.Clear();
        Console.WriteLine($"=== Gameplay Menu (user: {currentUser ?? "guest"}) ===");
        Console.WriteLine("1) Start");
        Console.WriteLine("2) High Score (your best) (TODO: Leaderboard.BestFor)");
        Console.WriteLine("3) Leaderboard (top 10)  (TODO: Leaderboard.Top)");
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
                Console.WriteLine("\n[TODO] Show Top 10 via Leaderboard.Top(10).");
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
