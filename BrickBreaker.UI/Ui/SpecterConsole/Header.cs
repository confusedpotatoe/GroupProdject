using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.UI.Ui.SpecterConsole
{
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
