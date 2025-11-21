namespace BrickBreaker.UI.Ui.Enums
{
    // GameMode represents different ways the game can be played.
    
    // Used to differentiate between gameplay styles or settings
    // makes it possible to trigger direct gameplay without going through menus
    // connected to UiManager and potentially other gameplay-related components
    public enum GameMode
    {
        Normal,   // Standard gameplay mode, typically requiring login or user setup.
        QuickPlay // A faster mode that skips login and starts the game immediately as a guest.

    }
}
