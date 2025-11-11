using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.Ui
{
    public class ConsoleGameplayMenu : IGameplayMenu
    {
        public GameplayMenuChoice Show(string currentUser)
        {
            Console.Clear();
            Console.WriteLine($"=== Gameplay Menu (user: {currentUser}) ===");
            Console.WriteLine("1) Start");
            Console.WriteLine("2) High Score (your best) (TODO: Leaderboard.BestFor)");
            Console.WriteLine("3) Leaderboard (top 10)");
            Console.WriteLine("4) Logout");
            Console.Write("Choose: ");

            var key = Console.ReadKey(true).KeyChar;

            return key switch
            {
                '1' => GameplayMenuChoice.Start,
                '2' => GameplayMenuChoice.Best,
                '3' => GameplayMenuChoice.Leaderboard,
                '4' => GameplayMenuChoice.Logout,
                _ => GameplayMenuChoice.Start
            };
        }
    }
}
