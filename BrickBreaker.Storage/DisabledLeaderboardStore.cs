using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;
using System.Collections.Generic;

namespace BrickBreaker.Storage;

/// <summary>
/// Fallback implementation that no-ops when the Supabase connection string is missing.
/// </summary>
public sealed class DisabledLeaderboardStore : ILeaderboardStore
{
    public void Add(ScoreEntry entry)
    {
        // Intentionally does nothing - leaderboard persistence requires the database.
    }

    public List<ScoreEntry> ReadAll() => new();
}
