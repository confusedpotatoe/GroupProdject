using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.Models;


public sealed class ScoreEntry
{
    public string Username { get; set; }
    public int Score { get; set; }
    public DateTimeOffset At { get; set; }

    public ScoreEntry(string username, int score, DateTimeOffset at)
    {
        Username = username;
        Score = score;
        At = at;
    }
}