using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.UI.Ui.Enums
{
    // GameplayMenuChoice defines the possible actions a user can select,
    // these values determine what the UIManager.cs does next.
    public enum GameplayMenuChoice
    {
        Start, //Start a new game session.
        Best, //Show the player's best scores or stats.
        Leaderboard, //Display the leaderboard.
        Logout, //Log out the current user and return to the login menu.
        Exit, //Close the application.
    }
}
