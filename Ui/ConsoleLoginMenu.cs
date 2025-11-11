using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

        namespace BrickBreaker.Ui
        {
            public class ConsoleLoginMenu : ILoginMenu
            {
                public LoginMenuChoice Show()
                {
                    Console.Clear();
                    Console.WriteLine("=== Main Menu ===");
                    Console.WriteLine("1) Register");
                    Console.WriteLine("2) Login");
                    Console.WriteLine("3) Leaderboard (view top 10)");
                    Console.WriteLine("4) Exit");
                    Console.Write("Choose: ");

                    var key = Console.ReadKey(true).KeyChar;

                    return key switch
                    {
                        '1' => LoginMenuChoice.Register,
                        '2' => LoginMenuChoice.Login,
                        '3' => LoginMenuChoice.Leaderboard,
                        '4' => LoginMenuChoice.Exit,
                        _ => LoginMenuChoice.Exit
                    };
                }
            }
        }

