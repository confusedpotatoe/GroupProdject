using BrickBreaker.Models;

namespace BrickBreaker.Logic.Abstractions;

public interface ILeaderboardStore
{
    void Add(ScoreEntry entry);
    List<ScoreEntry> ReadAll();
}
