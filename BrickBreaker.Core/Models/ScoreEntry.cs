namespace BrickBreaker.Models;


public sealed class ScoreEntry //defining which attributes the classes have
{
    public string Username { get; set; }
    public int Score { get; set; }
    public DateTimeOffset At { get; set; }

    public ScoreEntry(string username, int score, DateTimeOffset at) //constructor for the class
    {
        Username = username;
        Score = score;
        At = at;
    }
}