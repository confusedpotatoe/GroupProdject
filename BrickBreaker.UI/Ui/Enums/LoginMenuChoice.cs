namespace BrickBreaker.UI.Ui.Enums
{
    // LoginMenuChoice defines the options available to the user
    public enum LoginMenuChoice
    // These values are used to determine what action the UIManager should take next.  
    {
        QuickPlay, // Start the game immediately without creating or logging into an account.
        Register, // Create a new user account.
        Login,  // Log in with an existing user account.
        Leaderboard, // View the leaderboard before entering the game.
        Exit, // Close the application.

    }
}
