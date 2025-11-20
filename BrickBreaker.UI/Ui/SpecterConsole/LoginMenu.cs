using BrickBreaker.UI.Ui.Enums;
using BrickBreaker.UI.Ui.Interfaces;
using Spectre.Console;

namespace BrickBreaker.UI.Ui.SpecterConsole
{
    // implementation of the login menu using Spectre.Console
    // displays options related to user login and registration
    public class LoginMenu : ILoginMenu
    {
        // helper class for displaying menus
        private readonly MenuHelper _menuHelper = new MenuHelper();

        // shows the login menu and returns the user's choice
        public LoginMenuChoice Show()
        {
            // Clear console for a clean display
            AnsiConsole.Clear();

            // Display menu using Spectre.Console
            var choice = _menuHelper.ShowMenu<LoginMenuChoice>("Brick Breaker");

            // Return the user's choice
            return choice;
        }
    }
}

