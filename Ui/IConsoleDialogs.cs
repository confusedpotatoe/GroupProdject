using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace BrickBreaker.Ui
    {
   
            public interface IConsoleDialogs
            {
                (string Username, string Password) PromptCredentials();
                string PromptNewUsername();
                string PromptNewPassword();

                void ShowMessage(string message);
                void Pause();

                void ShowLeaderboard(System.Collections.Generic.IEnumerable<(string Username, int Score, System.DateTimeOffset At)> entries);
            }
        }

    
