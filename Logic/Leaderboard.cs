using BrickBreaker.Models;
using BrickBreaker.Storage;

namespace BrickBreaker.Logic;

public sealed class Leaderboard
{
    private readonly LeaderboardStore _store;
    public Leaderboard(LeaderboardStore store) => _store = store;

    public void Submit(ScoreEntry entry)
    {
        _store.Add(entry);
    }

    // convenience overload
    public void Submit(string username, int score)
    {
        _store.Add(new ScoreEntry(username, score, DateTimeOffset.UtcNow));
    }

    public List<ScoreEntry> Top(int n)
    {
        return _store.ReadAll()
                     .OrderByDescending(s => s.Score)
                     .ThenBy(s => s.At)
                     .Take(n)
                     .ToList();
    }

    public ScoreEntry? BestFor(string username)
    {
        return _store.ReadAll()
                     .Where(s => s.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                     .OrderByDescending(s => s.Score)
                     .ThenBy(s => s.At)
                     .FirstOrDefault();
    }
}





