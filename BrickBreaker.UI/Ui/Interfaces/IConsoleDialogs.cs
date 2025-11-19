using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BrickBreaker.UI.Ui.Interfaces
{

    public interface IConsoleDialogs
    {
        (string Username, string Password) PromptCredentials();
        string PromptNewUsername();
        string PromptNewPassword();

        void ShowMessage(string message);
        void Pause();

        void ShowLeaderboard(IEnumerable<(string Username, int Score, DateTimeOffset At)> entries);
    }
}


