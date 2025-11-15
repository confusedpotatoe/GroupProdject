using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;

namespace BrickBreaker.Logic;

public sealed class Leaderboard
{
    private readonly ILeaderboardStore _store;
    public Leaderboard(ILeaderboardStore store) => _store = store;

    public void Submit(ScoreEntry entry)
    {
        _store.Add(entry);
    }

    public void Submit(string username, int score)
    {
        if (string.IsNullOrWhiteSpace(username) || score < 0) return;
        _store.Add(new ScoreEntry(username.Trim(), score, DateTimeOffset.UtcNow));
    }

    public List<ScoreEntry> Top(int n)
    {
        var list = _store.ReadAll();
        return list
            .Where(s => !string.IsNullOrWhiteSpace(s.Username) && s.Score >= 0)
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.At)
            .ThenBy(s => s.Username, StringComparer.OrdinalIgnoreCase)
            .Take(n)
            .ToList();
    }

    public ScoreEntry? BestFor(string username)
    {
        var list = _store.ReadAll();
        return list
            .Where(s => !string.IsNullOrWhiteSpace(s.Username) &&
                        s.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.At)
            .ThenBy(s => s.Username, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }
}
