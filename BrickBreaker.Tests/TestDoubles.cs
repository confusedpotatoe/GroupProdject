// A fake in-memory implementation of IUserStore interface to simulate user data storage and retrieval in tests.
using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;

internal sealed class FakeUserStore : IUserStore
{
    // Dictionary holding users keyed by username (case-insensitive).
    private readonly Dictionary<string, User> _users = new(StringComparer.OrdinalIgnoreCase);

    // List of users that were added during the test run.
    public List<User> AddedUsers { get; } = new();

    // Tracks the last username checked for existence, useful for verifying test calls.
    public string? LastExistsUsername { get; private set; }

    // Tracks the last username requested by the Get method.
    public string? LastGetUsername { get; private set; }

    // Seeds initial users into the store for test setup.
    public void Seed(User user)
    {
        if (!string.IsNullOrWhiteSpace(user?.Username))
        {
            _users[user.Username.Trim()] = user;
        }
    }

    // Checks if a username exists in the store.
    public bool Exists(string username)
    {
        LastExistsUsername = username;
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }
        return _users.ContainsKey(username.Trim());
    }

    // Adds a user to the store and tracks the addition.
    public void Add(User user)
    {
        AddedUsers.Add(user);
        if (!string.IsNullOrWhiteSpace(user.Username))
        {
            _users[user.Username.Trim()] = user;
        }
    }

    // Retrieves a user by username if present, otherwise returns null.
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

// Fake in-memory leaderboard store for testing leaderboard persistence and retrieval.
internal sealed class FakeLeaderboardStore : ILeaderboardStore
{
    // List of score entries added during tests.
    public List<ScoreEntry> AddedEntries { get; } = new();

    // List of score entries to return for read/readall operations (simulating stored data).
    public List<ScoreEntry> EntriesToReturn { get; set; } = new();

    // Adds a score entry to the internal list.
    public void Add(ScoreEntry entry)
    {
        AddedEntries.Add(entry);
    }

    // returns a copy of the entries to simulate retrieving leaderboard data.
    public List<ScoreEntry> ReadAll()
    {
        // return deep copies to prevent tests from accidentally mutating internal state.
        return EntriesToReturn
            .Select(e => new ScoreEntry(e.Username, e.Score, e.At))
            .ToList();
    }
}
