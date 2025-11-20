using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;

namespace BrickBreaker.Logic;

public sealed class Leaderboard
{
    private readonly ILeaderboardStore _store;
    public Leaderboard(ILeaderboardStore store) => _store = store;

    public void Submit(ScoreEntry entry) //adds a new score netry to _store (which is leaderboardstore)
    {
        _store.Add(entry);
    }

    public void Submit(string username, int score) //method that checks the username and score to make sure the data is valid, prevents invalid data entries
    {
        if (string.IsNullOrWhiteSpace(username) || score < 0) return;//checks if username is valid and if the score is valid
        _store.Add(new ScoreEntry(username.Trim(), score, DateTimeOffset.UtcNow)); //if both are it sends the info to leaderboardstore with a timestamp
    }

    public List<ScoreEntry> Top(int n) // a method that reads the top users in leaderboard and sorts them by score 
    {
        var list = _store.ReadAll();
        return list
            .Where(s => !string.IsNullOrWhiteSpace(s.Username) && s.Score >= 0)
            .OrderByDescending(s => s.Score) //orders the info by score
            .ThenBy(s => s.At)
            .ThenBy(s => s.Username, StringComparer.OrdinalIgnoreCase)
            .Take(n)
            .ToList();
    }

    public ScoreEntry? BestFor(string username) //finds the best score for the currentuser
    {
        var list = _store.ReadAll(); //reads the leaderboardstore
        return list
            .Where(s => !string.IsNullOrWhiteSpace(s.Username) &&
                        s.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase)) //looks for the username of the current user in leaderboardstore
            .OrderByDescending(s => s.Score) //orders by score
            .ThenBy(s => s.At)
            .ThenBy(s => s.Username, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }
}
