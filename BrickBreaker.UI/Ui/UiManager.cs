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

        public void Run()
        {
            AppState state = AppState.LoginMenu;

            while (state != AppState.Exit)
            {
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
            var choice = _loginMenu.Show();

            return choice switch
            {
                LoginMenuChoice.QuickPlay => AppState.Playing, 
                LoginMenuChoice.Login => AppState.LoginMenu,  
                LoginMenuChoice.Register => AppState.LoginMenu, 
                LoginMenuChoice.Leaderboard => AppState.LoginMenu, 
                LoginMenuChoice.Exit => AppState.Exit, 
                _ => AppState.LoginMenu 
            };
        }

        // Handles the Gameplay Menu state and returns the next application state
        // Triggers by appstate gameplaymenu
        private AppState HandleGameplayMenu()
        {
            var choice = _gameplayMenu.Show(currentUser ?? "guest");

            return choice switch
            {
                GameplayMenuChoice.Start => AppState.Playing,
                GameplayMenuChoice.Best => AppState.GameplayMenu,
                GameplayMenuChoice.Logout => AppState.LoginMenu,
                GameplayMenuChoice.Exit => AppState.Exit,
                _ => AppState.GameplayMenu
            };
        }
    }
}
