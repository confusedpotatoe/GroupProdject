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
        // Generic method to display enum-based menu and return selection
        public T ShowMenu<T>(string title) where T : Enum
        {
            var items = Enum.GetValues(typeof(T)).Cast<T>().ToList();

            return AnsiConsole.Prompt(
                new SelectionPrompt<T>()
                    .Title(title)
                    .PageSize(10)
                    .AddChoices(items)
            );
        }
    }
}
