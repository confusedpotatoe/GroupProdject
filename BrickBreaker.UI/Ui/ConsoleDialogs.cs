using BrickBreaker.UI.Ui.Interfaces;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.Ui
{
    public class ConsoleDialogs : IConsoleDialogs
    {
        public (string Username, string Password) PromptCredentials()
        {
            var username = AnsiConsole.Prompt(
                new TextPrompt<string>("Username: ")
                    .PromptStyle("White"));

            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Password: ")
                    .PromptStyle("White")
                    .Secret());

            return (username, password);
        }

        public string PromptNewUsername()
        {
            AnsiConsole.Write("\nChoose a username: ");
            return Console.ReadLine()?.Trim() ?? "";
        }

        public string PromptNewPassword()
        {
           AnsiConsole.Write("Choose a password: ");
            return Console.ReadLine()?.Trim() ?? "";
        }

        public void ShowMessage(string message) => AnsiConsole.MarkupLine(message);

        public void Pause()
        {
            AnsiConsole.MarkupLine("[grey]Press any key…[/]");
            Console.ReadKey(true);
        }

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

            int i = 1;

            foreach (var e in entries)
            {
                var localAt = e.At.ToLocalTime();

                table.AddRow(
                    $"{i++}",
                    e.Username,
                    $"{e.Score}",
                    localAt.ToString("yyyy-MM-dd HH:mm")
                );

                if (i > 10)
                    break;
            }

            // Print table
            AnsiConsole.Write(table);
        }
    }
}


