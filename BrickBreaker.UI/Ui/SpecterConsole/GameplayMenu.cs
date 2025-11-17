using BrickBreaker.UI.Ui.Enums;
using BrickBreaker.UI.Ui.Interfaces;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.UI.Ui.SpecterConsole
{
    public class GameplayMenu : IGameplayMenu
    {
        private readonly MenuHelper _menuHelper = new MenuHelper();

        public GameplayMenuChoice Show(string username)
        {
            // Clear the console for a clean menu
            AnsiConsole.Clear();

            // Use MenuHelper to display the menu
            var choice = _menuHelper.ShowMenu<GameplayMenuChoice>("Brick Breaker");

            return choice;
        }
    }
}
