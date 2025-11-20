using Spectre.Console;

namespace BrickBreaker.UI.Ui
{
    // Helper class to display menus using Spectre.Console
    // Generic method to show menus for different enum types
    // Configurable title, welcome message, and colors

    public class MenuHelper
    {
        // Generic method to show a menu based on an enum type
        // Parameters: title, welcomeMessage, titleColor, highlightColor
        // Returns the selected enum value
        public T ShowMenu<T>(string title, string? welcomeMessage = null, Color? titleColor = null, Color? highlightColor = null) where T : Enum
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

            // Welcome message 
            if (!string.IsNullOrWhiteSpace(welcomeMessage))
            {
                AnsiConsole.MarkupLine(welcomeMessage);
                AnsiConsole.WriteLine();
            }

            // Menu items 
            var items = Enum.GetValues(typeof(T)).Cast<T>().ToList();

            // Display selection prompt
            return AnsiConsole.Prompt(
                new SelectionPrompt<T>()
                    .Title("[gray]Select an option:[/]")
                    .PageSize(10)
                    .AddChoices(items)
                    .HighlightStyle(new Style(hColor, Color.Black, Decoration.Bold))

                    // Customize display for specific choices
                    .UseConverter(choice =>
                    {
                        var text = choice.ToString();

                        // Highlight "Exit" and "Logout" in red
                        return text is "Exit" or "Logout"
                            ? $"[red]{text}[/]"
                            : text;
                    })
            );


        }
    }
}
