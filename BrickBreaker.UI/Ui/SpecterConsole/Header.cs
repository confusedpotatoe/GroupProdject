using Spectre.Console;

namespace BrickBreaker.UI.Ui.SpecterConsole
{
    // Displays the header for the Brick Breaker game
    // Use FigletText from Spectre.Console

    public class Header
    {
        public void TitleHeader()
        {
            AnsiConsole.Write(
                new FigletText("Brick Breaker")
                    .Centered()
                    .Color(Color.Orange1)
            );
        }
    }
}
