using BrickBreaker.Logic;
using BrickBreaker.Models;
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
    public class LoginMenu : ILoginMenu
    {
        private readonly MenuHelper _menuHelper = new MenuHelper();

        public LoginMenuChoice Show()
        {
            // Clear console for a clean display
            AnsiConsole.Clear();
           
            // Display menu using Spectre.Console
            var choice = _menuHelper.ShowMenu<LoginMenuChoice>("Brick Breaker");

            return choice;
        }
    }
}

