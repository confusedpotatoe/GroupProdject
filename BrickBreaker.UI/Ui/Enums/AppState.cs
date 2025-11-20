namespace BrickBreaker.UI.Ui.Enums
{

    // enumberation representing different states of the application
    // trigger transitions between different screens and functionalities

    // connection to interfaces like ILoginMenu and IGameplayMenu
    public enum AppState
    {
        LoginMenu,
        GameplayMenu,
        Playing,
        Exit
    }
}
