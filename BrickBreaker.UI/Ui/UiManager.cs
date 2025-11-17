using BrickBreaker.UI.Ui.Enums;
using BrickBreaker.UI.Ui.SpecterConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.UI.Ui
{
    public class UiManager
    {
        private readonly LoginMenu _loginMenu = new LoginMenu();
        private readonly GameplayMenu _gameplayMenu = new GameplayMenu();

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

        private AppState HandleLoginMenu()
        {
            var choice = _loginMenu.Show();

            return choice switch
            {
                LoginMenuChoice.QuickPlay => AppState.Playing, //fixed to go to gameplay menu after quick play
                LoginMenuChoice.Login => AppState.LoginMenu,
                LoginMenuChoice.Register => AppState.LoginMenu,
                LoginMenuChoice.Leaderboard => AppState.LoginMenu,
                LoginMenuChoice.Exit => AppState.Exit,
                _ => AppState.LoginMenu
            };
        }

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
