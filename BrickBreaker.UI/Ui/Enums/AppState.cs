using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BrickBreaker.UI.Ui.Enums
{
    //AppState represents the different states the application can be in, out of the following. 
    public enum AppState
    {
        LoginMenu, // The application is showing the login menu (start screen).
        GameplayMenu, // The user is logged in or selected quick play and is viewing gameplay-related options.
        Playing, // The game itself is running.
        Exit // The application should close.
    }
}

