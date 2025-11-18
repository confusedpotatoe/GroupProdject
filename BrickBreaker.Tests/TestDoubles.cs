using System.Linq;
using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;

namespace BrickBreaker.Tests;

internal sealed class FakeUserStore : IUserStore
{
    private readonly Dictionary<string, User> _users = new(StringComparer.OrdinalIgnoreCase);

    public List<User> AddedUsers { get; } = new();
    public string? LastExistsUsername { get; private set; }
    public string? LastGetUsername { get; private set; }

    public void Seed(User user)
    {
        if (!string.IsNullOrWhiteSpace(user?.Username))
        {
            _users[user.Username.Trim()] = user;
        }
    }

    public bool Exists(string username)
    {
        LastExistsUsername = username;
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        return _users.ContainsKey(username.Trim());
    }

    public void Add(User user)
    {
        AddedUsers.Add(user);
        if (!string.IsNullOrWhiteSpace(user.Username))
        {
            _users[user.Username.Trim()] = user;
        }
    }

    public User? Get(string username)
    {
        LastGetUsername = username;
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return _users.TryGetValue(username.Trim(), out var user) ? user : null;
    }
}

// In-memory leaderboard store
internal sealed class FakeLeaderboardStore : ILeaderboardStore
{
    public List<ScoreEntry> AddedEntries { get; } = new();
    public List<ScoreEntry> EntriesToReturn { get; set; } = new();

    public void Add(ScoreEntry entry)
    {
        AddedEntries.Add(entry);
    }

    public List<ScoreEntry> ReadAll()
    {
        return EntriesToReturn
            .Select(e => new ScoreEntry(e.Username, e.Score, e.At))
            .ToList();
    }
}
