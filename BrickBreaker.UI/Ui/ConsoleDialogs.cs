using BrickBreaker.UI.Ui.Interfaces;
using Spectre.Console;

namespace BrickBreaker.Ui
{
    // Implementation of console dialogs using Spectre.Console
    // Handles user prompts and messages in the console UI
    public class ConsoleDialogs : IConsoleDialogs
    {
        // Prompts user for username and password
        public (string Username, string Password) PromptCredentials()
        {
            // Prompt for username
            var username = AnsiConsole.Prompt(
                new TextPrompt<string>("Username: ")
                    .PromptStyle("White"));

            // Prompt for password (hidden input)
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Password: ")
                    .PromptStyle("White")
                    .Secret());

            // Return the entered credentials
            return (username, password);
        }

        // Prompts user to choose a new username
        public string PromptNewUsername()
        {
            // Prompt for new username
            AnsiConsole.Write("\nChoose a username: ");
            // Read and return the input trimmed of whitespace
            return Console.ReadLine()?.Trim() ?? "";
        }

        // Prompts user to choose a new password
        public string PromptNewPassword()
        {
            // Prompt for new password
            AnsiConsole.Write("Choose a password: ");
            // Read and return the input trimmed of whitespace
            return Console.ReadLine()?.Trim() ?? "";
        }

        // Displays a message to the user
        // Can be used to call diffrent types of messages
        public void ShowMessage(string message) => AnsiConsole.MarkupLine(message);

        // Pauses execution until user presses a key
        public void Pause()
        {
            // Prompt user to press any key to continue
            AnsiConsole.MarkupLine("[grey]Press any key…[/]");
            // Wait for a key press
            Console.ReadKey(true);
        }

        // Displays leaderboard entries in a formatted table
        // Shows top 10 entries
        // Each entry includes username, score, and date
        public void ShowLeaderboard(IEnumerable<(string Username, int Score, DateTimeOffset At)> entries)
        {
            // Create a table
            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title("Top 10 Leaderboard");

            // Add columns
            table.AddColumn("[bold]#[/]");
            table.AddColumn("[bold]Username[/]");
            table.AddColumn("[bold]Score[/]");
            table.AddColumn("[bold]Date[/]");

            // Row counter
            int i = 1;

            // Add rows for each entry
            foreach (var e in entries)
            {
                var localAt = e.At.ToLocalTime();

                table.AddRow(
                    $"{i++}",
                    e.Username,
                    $"{e.Score}",
                    localAt.ToString("yyyy-MM-dd HH:mm")
                );

                // Limit to top 10 entries
                if (i > 10)
                    break;
            }

            // Print table
            AnsiConsole.Write(table);
        }
    }
}


