namespace BrickBreaker.UI.Ui.Interfaces
{
    // interface for console dialog interactions
    // handles user prompts and messages in the console UI
    // connected to console dialog implementations

    // collection of methods for prompting user input and displaying messages
    public interface IConsoleDialogs
    {
        (string Username, string Password) PromptCredentials();
        string PromptNewUsername();
        string PromptNewPassword();

        void ShowMessage(string message);
        void Pause();

        // displays leaderboard entries in a formatted table
        void ShowLeaderboard(IEnumerable<(string Username, int Score, DateTimeOffset At)> entries);
    }
}


