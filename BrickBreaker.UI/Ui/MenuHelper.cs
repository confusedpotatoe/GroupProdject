using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.UI.Ui
{
    public static class MenuHelper
    {
        public static T ShowMenu<T>(string title) where T : Enum
        {
            // Convert enum values to string list for Spectre console
            var options = Enum.GetNames(typeof(T));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{title}[/]")
                    .PageSize(10)
                    .AddChoices(options)
            );

            return (T)Enum.Parse(typeof(T), choice);
        }

        public static void Pause(string message = "Press any key to continue...")
        {
            AnsiConsole.MarkupLine(message);
            Console.ReadKey(true);
        }

        public static bool Confirm(string message)
        {
            return AnsiConsole.Confirm(message);
        }
    }
}
