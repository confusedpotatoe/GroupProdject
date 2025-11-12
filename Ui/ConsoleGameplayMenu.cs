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

            while (true)
            {
                Console.Write("Choose: ");
                var key = ReadMenuChoice();

                switch (key)
                {
                    case '1': return GameplayMenuChoice.Start;
                    case '2': return GameplayMenuChoice.Best;
                    case '3': return GameplayMenuChoice.Leaderboard;
                    case '4': return GameplayMenuChoice.Logout;
                }
            }
        }

        static char ReadMenuChoice()
        {
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar is >= '1' and <= '4')
                {
                    Console.WriteLine(key.KeyChar);
                    DrainPendingKeys();
                    return key.KeyChar;
                }
                if (key.Key == ConsoleKey.Enter) continue;
            }
        }

        static void DrainPendingKeys()
        {
            while (Console.KeyAvailable)
                Console.ReadKey(true);
        }
    }
}
