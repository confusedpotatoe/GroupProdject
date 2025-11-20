using BrickBreaker.UI.Ui.Enums;
using BrickBreaker.UI.Ui.Interfaces;
using Spectre.Console;

namespace BrickBreaker.UI.Ui.SpecterConsole
{
    // implementation of the gameplay menu using Spectre.Console
    // Shows after user logs in
    public class GameplayMenu : IGameplayMenu
    {
        private readonly MenuHelper _menuHelper = new MenuHelper();

        // shows the gameplay menu and returns the user's choice
        public GameplayMenuChoice Show(string username)
        {
            AnsiConsole.Clear();
            var choice = _menuHelper.ShowMenu<GameplayMenuChoice>("Brick Breaker", welcomeMessage: $"[bold]Welcome, {username}![/]\n");
            return choice;
        }
    }
}
