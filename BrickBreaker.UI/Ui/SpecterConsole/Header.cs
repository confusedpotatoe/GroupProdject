using Spectre.Console;

namespace BrickBreaker.UI.Ui.SpecterConsole
{
    // Displays the header for the Brick Breaker game
    // Use FigletText from Spectre.Console

    public class Header
    {
        // Renders the title header in the console as a method
        public void TitleHeader()
        {
            // Use FigletText to create a stylized title
            AnsiConsole.Write(
                new FigletText("Brick Breaker")
                    .Centered()
                    .Color(Color.Orange1)
            );
        }
    }
}
