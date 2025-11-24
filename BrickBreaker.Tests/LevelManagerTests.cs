using BrickBreaker.Game.Systems;

namespace BrickBreaker.Tests;

public sealed class LevelManagerTests
{
    [Fact]
    public void AllBricksCleared_ReturnsTrueOnlyWhenEveryBrickIsRemoved()
    {
        var manager = new LevelManager();

        Assert.False(manager.AllBricksCleared());

        var width = manager.Bricks.GetLength(0);
        var height = manager.Bricks.GetLength(1);
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                manager.Bricks[x, y] = false;
            }
        }

        Assert.True(manager.AllBricksCleared());
    }

    [Fact]
    public void LoadLevel_ClonesLayoutSoMutationsDoNotLeak()
    {
        var manager = new LevelManager();
        var index = manager.CurrentLevelIndex;
        var original = manager.Bricks[0, 0];

        manager.Bricks[0, 0] = !original;
        Assert.NotEqual(original, manager.Bricks[0, 0]);

        manager.LoadLevel(index);

        Assert.Equal(original, manager.Bricks[0, 0]);
    }

    [Fact]
    public void TryLoadNextLevel_AdvancesUntilNoMoreLevels()
    {
        var manager = new LevelManager();
        var startingIndex = manager.CurrentLevelIndex;

        var advanced = manager.TryLoadNextLevel();

        Assert.True(advanced);
        Assert.Equal(startingIndex + 1, manager.CurrentLevelIndex);

        while (manager.TryLoadNextLevel())
        {
            // Keep advancing until the final level is reached.
        }

        Assert.False(manager.TryLoadNextLevel());
    }
}
