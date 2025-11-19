namespace BrickBreaker.Game;

/// <summary>
/// Shared abstraction for running a playable BrickBreaker session.
/// Implementations can provide their own presentation (console, WinForms, etc.)
/// but must return the achieved score once the session ends.
/// </summary>
public interface IGame
{
    /// <summary>
    /// Starts the gameplay loop and blocks until the session ends.
    /// The returned integer represents the player's final score.
    /// </summary>
    int Run();
}
