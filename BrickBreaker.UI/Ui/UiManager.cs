using BrickBreaker.UI.Ui.Enums;
using BrickBreaker.UI.Ui.SpecterConsole;

namespace BrickBreaker.UI.Ui
{
    // Manages the overall UI flow of the Brick Breaker application
    // Handles transitions between different menus based on user input

    public class UiManager
    {
        // Enum representing the different states of the application
        private readonly LoginMenu _loginMenu = new LoginMenu();
        private readonly GameplayMenu _gameplayMenu = new GameplayMenu();

        // Stores the current logged-in user
        private string? currentUser = null;

        // Main loop that controls the application state (AppState)
        public void Run()
        {
            AppState state = AppState.LoginMenu; // Start in the login menu

            // Continue running until the user chooses to exit
            while (state != AppState.Exit)
            {
                // Switch to the appropriate handler based on the current application state
                state = state switch
                {
                    AppState.LoginMenu => HandleLoginMenu(), 
                    AppState.GameplayMenu => HandleGameplayMenu(),
                    _ => AppState.Exit
                };
            }
        }

        // Handles the Login Menu state and returns the next application state
        // Triggers by appstate loginmenu
        private AppState HandleLoginMenu()
        {
            var choice = _loginMenu.Show(); // Show login menu and store the user's selected option in 'choice'

            // Convert user choice into the next application state
            return choice switch
            {
                LoginMenuChoice.QuickPlay => AppState.Playing, // Quick Play goes directly to gameplay
                LoginMenuChoice.Login => AppState.LoginMenu,   // Will stay in login menu after login attempt
                LoginMenuChoice.Register => AppState.LoginMenu, // Same for registration
                LoginMenuChoice.Leaderboard => AppState.LoginMenu, // Leaderboard returns back to login menu
                LoginMenuChoice.Exit => AppState.Exit, // Exit application
                _ => AppState.LoginMenu
            };
        }

        // Handles user choices in the gameplay menu
        private AppState HandleGameplayMenu()
        {
            // Use current user or "guest" if no user is logged in
            var choice = _gameplayMenu.Show(currentUser ?? "guest");

            // Convert gameplay menu choice into application state
            return choice switch
            {
                GameplayMenuChoice.Start => AppState.Playing, // Start the game
                GameplayMenuChoice.Best => AppState.GameplayMenu, // Show best scores and stay in menu
                GameplayMenuChoice.Logout => AppState.LoginMenu, // Return to login menu
                GameplayMenuChoice.Exit => AppState.Exit, // Exit application
                _ => AppState.GameplayMenu
            };
        }
    }
}

