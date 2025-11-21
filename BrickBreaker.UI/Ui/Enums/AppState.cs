namespace BrickBreaker.UI.Ui.Enums;

// Represents the major states the UI can be in to drive screen changes.
public enum AppState
{
    LoginMenu,      // The application is showing the login menu (start screen).
    GameplayMenu,   // The user is logged in or selected quick play.
    Playing,        // The game itself is running.
    Exit            // The application should close.
}

