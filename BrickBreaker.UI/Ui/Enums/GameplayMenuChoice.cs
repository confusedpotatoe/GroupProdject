namespace BrickBreaker.UI.Ui.Enums
{
    // Represents the various options available in the gameplay menu
    // Connected to the IGameplayMenu interface
    public enum GameplayMenuChoice
    {
        Start, //Start a new game session.
        Best, //Show the player's best scores or stats.
        Leaderboard, //Display the leaderboard.
        Logout, //Log out the current user and return to the login menu.
        Exit, //Close the application.
    }
}
