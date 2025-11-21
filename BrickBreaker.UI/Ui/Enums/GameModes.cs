using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.UI.Ui.Enums
{
    // GameMode represents different ways the game can be played.
    public enum GameMode
    {
        Normal,   // Standard gameplay mode, typically requiring login or user setup.
        QuickPlay // A faster mode that skips login and starts the game immediately as a guest.

    }
}
