using Spectre.Console;

namespace BrickBreaker.UI.Ui
{
    // Helper class to display menus using Spectre.Console
    // Generic method to show menus for different enum types

    public class MenuHelper
    {
        public T ShowMenu<T>(string title, string? welcomeMessage = null, Color? titleColor = null, Color? highlightColor = null) where T : Enum
        {
            var tColor = titleColor ?? Color.Orange1;
            var hColor = highlightColor ?? Color.White;

            // Display title in Figlet font
            AnsiConsole.Write(
                new FigletText(title)
                    .Centered()
                    .Color(tColor)
            );

            // If a welcome message exists, print it below the title.
            if (!string.IsNullOrWhiteSpace(welcomeMessage))
            {
                AnsiConsole.MarkupLine(welcomeMessage);
                AnsiConsole.WriteLine(); // Add extra spacing before the menu
            }

            // Retrieve all enum values for the generic type T.
            // These values will be used as selectable items in the menu.
            var items = Enum.GetValues(typeof(T)).Cast<T>().ToList();

            // Create and display a selection menu using Spectre.Console.
            // The menu allows the user to navigate with arrow keys and confirm with Enter.
            return AnsiConsole.Prompt(
                new SelectionPrompt<T>()
                    .Title("[gray]Select an option:[/]") // Text shown above the choices
                    .PageSize(10)                       // Max number of items before scrolling
                    .AddChoices(items)                  // Add all enum values as menu options

                    // Apply highlight styling when hovering over an option.
                    .HighlightStyle(new Style(hColor, Color.Black, Decoration.Bold))

                    // Convert enum names to formatted display text.
                    // Special cases: "Exit" and "Logout" are colored red for emphasis.
                    .UseConverter(choice =>
                    {
                        var text = choice.ToString();

                        // Highlight "Exit" and "Logout" in red
                        return text is "Exit" or "Logout"
                            ? $"[red]{text}[/]"  // Red text for important actions
                            : text;              // Default rendering for all other items
                    })
            );
        }
    }
}
