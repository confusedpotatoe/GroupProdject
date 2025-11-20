using BrickBreaker.UI.Ui.Interfaces;
using Spectre.Console;

namespace BrickBreaker.Ui
{
    // Implementation of console dialogs using Spectre.Console
    // Handles user prompts and messages in the console UI
    public class ConsoleDialogs : IConsoleDialogs
    {
        public (string Username, string Password) PromptCredentials()
        {
            // Prompt for username
            var username = AnsiConsole.Prompt(
                new TextPrompt<string>("Username: ")
                    .PromptStyle("White"))
                    .Trim();

            // Prompt for password (hidden input)
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Password: ")
                    .PromptStyle("White")
                    .Secret())
                    .Trim();

            return (username, password);
        }

        public string PromptNewUsername()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("\nChoose a username:")
                    .PromptStyle("White"))
                .Trim();
        }

        public string PromptNewPassword()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Choose a password:")
                    .PromptStyle("White")
                    .Secret())
                .Trim();
        }

        public void ShowMessage(string message) => AnsiConsole.MarkupLine(message);

        public void Pause()
        {
            AnsiConsole.MarkupLine("[grey]Press any key…[/]");
            Console.ReadKey(true);
        }

        // Displays leaderboard entries in a formatted table
        public void ShowLeaderboard(IEnumerable<(string Username, int Score, DateTimeOffset At)> entries)
        {
            // Create a table
            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title("Top 10 Leaderboard");

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

            AnsiConsole.Write(table);
        }
    }
}


