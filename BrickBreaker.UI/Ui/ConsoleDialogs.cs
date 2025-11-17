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
            Console.Write("\nChoose a username: ");
            return Console.ReadLine()?.Trim() ?? "";
        }

        public string PromptNewPassword()
        {
            Console.Write("Choose a password: ");
            return Console.ReadLine()?.Trim() ?? "";
        }

        public void ShowMessage(string message) => Console.WriteLine(message);

        public void Pause()
        {
            Console.WriteLine("\nPress any key...");
            Console.ReadKey(true);
        }

        public void ShowLeaderboard(System.Collections.Generic.IEnumerable<(string Username, int Score, DateTimeOffset At)> entries)
        {
            Console.WriteLine("\nTop 10 leaderboard:");
            int i = 1;
            foreach (var e in entries)
            {
                // Convert stored timestamp to the local timezone for display
                var localAt = e.At.ToLocalTime();
                Console.WriteLine($"{i++}. {e.Username}  {e.Score}  {localAt:yyyy-MM-dd HH:mm}");
            }
        }
    }
}


