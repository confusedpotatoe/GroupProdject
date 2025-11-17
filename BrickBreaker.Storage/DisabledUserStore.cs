using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;

namespace BrickBreaker.Storage;

/// <summary>
/// Fallback implementation that keeps the app running when the database connection is unavailable.
/// </summary>
public sealed class DisabledUserStore : IUserStore
{
    public bool Exists(string username) => false;

    public void Add(User user)
    {
        // Intentionally left blank - database-backed registration is disabled.
    }

    public User? Get(string username) => null;
}
