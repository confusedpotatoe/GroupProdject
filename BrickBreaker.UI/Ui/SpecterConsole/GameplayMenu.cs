using BrickBreaker.UI.Ui.Enums;
using BrickBreaker.UI.Ui.Interfaces;
using Spectre.Console;

namespace BrickBreaker.UI.Ui.SpecterConsole
{
    // implementation of the gameplay menu using Spectre.Console
    // displays options related to gameplay
    // Shows after user logs in
    public class GameplayMenu : IGameplayMenu
    {
        // Helper class to manage menu display and user input
        private readonly MenuHelper _menuHelper = new MenuHelper();

        // shows the gameplay menu and returns the user's choice
        public GameplayMenuChoice Show(string username)
        {
            // Clear the console for a clean menu
            AnsiConsole.Clear();

            // Use MenuHelper to display the menu
            var choice = _menuHelper.ShowMenu<GameplayMenuChoice>("Brick Breaker", welcomeMessage: $"[bold]Welcome, {username}![/]\n");

            // Return the user's choice
            return choice;
        }
    }
}
