using Microsoft.VisualBasic;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.UI.Ui
{
    public class MenuHelper
    {
        public T ShowMenu<T>(string title, Color? titleColor = null, Color? highlightColor = null) where T : Enum
        {
            // Set colors
            var tColor = titleColor ?? Color.Orange1;
            var hColor = highlightColor ?? Color.White;

            // Clear console
            AnsiConsole.Clear();

            // Show Figlet title
            AnsiConsole.Write(
                new FigletText(title)
                    .Centered()
                    .Color(tColor)
            );

            // Menu items
            var items = Enum.GetValues(typeof(T)).Cast<T>().ToList();

            return AnsiConsole.Prompt(
                new SelectionPrompt<T>()
                    .Title("[bold yellow]Select an option:[/]")
                    .PageSize(10)
                    .AddChoices(items)
                    .HighlightStyle(new Style(hColor, Color.Black, Decoration.Bold))
            );
        }
    }
}
