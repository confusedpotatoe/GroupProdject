using BrickBreaker.Logic;
using BrickBreaker.Models;
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
        public T ShowMenu<T>(string title,string? welcomeMessage = null, Color? titleColor = null, Color? highlightColor = null) where T : Enum
        {
            // Set colors
            var tColor = titleColor ?? Color.Orange1;
            var hColor = highlightColor ?? Color.White;



            // Show Figlet title
            AnsiConsole.Write(
                new FigletText(title)
                    .Centered()
                    .Color(tColor)
            );

            if (!string.IsNullOrWhiteSpace(welcomeMessage))
            {
                AnsiConsole.MarkupLine(welcomeMessage);
                AnsiConsole.WriteLine();
            }

            // Menu items
            var items = Enum.GetValues(typeof(T)).Cast<T>().ToList();

            return AnsiConsole.Prompt(
                new SelectionPrompt<T>()
                    .Title("[gray]Select an option:[/]")
                    .PageSize(10)
                    .AddChoices(items)
                    .HighlightStyle(new Style(hColor, Color.Black, Decoration.Bold))
                    .UseConverter(choice =>
                    {
                        var text = choice.ToString();

                        return text is "Exit" or "Logout"
                            ? $"[red]{text}[/]"
                            : text;
                    })
            );

            
        }
    }
}
