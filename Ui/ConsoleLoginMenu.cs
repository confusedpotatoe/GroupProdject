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

                    while (true)
                    {
                        Console.Write("Choose: ");
                        var key = ReadMenuChoice();

                        switch (key)
                        {
                            case '1': return LoginMenuChoice.Register;
                            case '2': return LoginMenuChoice.Login;
                            case '3': return LoginMenuChoice.Leaderboard;
                            case '4': return LoginMenuChoice.Exit;
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
                        Console.WriteLine("Please enter 1-4.");
                    }
                }

                static void DrainPendingKeys()
                {
                    while (Console.KeyAvailable)
                        Console.ReadKey(true);
                }
            }
        }
