using BrickBreaker.Game.Systems;
using BrickBreaker.UI.Game.Models;

namespace BrickBreaker.Tests;

public sealed class PowerUpLogicTests
{
    [Fact]
    public void ActivatePowerUp_MultiBall_AddsTwoBallsWithOpposingVelocities()
    {
        var balls = new List<Ball>();
        var powerUp = new PowerUp(0, 0, PowerUpType.MultiBall);
        var paddleX = 10;
        var paddleWidth = 6;
        var paddleCenter = paddleX + paddleWidth / 2;
        var paddleExtendTimer = 0;
        var paddleY = 20;

        PowerUpLogic.ActivatePowerUp(powerUp, balls, ref paddleX, ref paddleWidth, ref paddleExtendTimer, paddleY);

        Assert.Equal(2, balls.Count);
        Assert.All(balls, ball =>
        {
            Assert.Equal(paddleCenter, ball.X);
            Assert.Equal(paddleY - 1, ball.Y);
            Assert.True(ball.IsMultiball);
            Assert.Equal(-1, ball.Dy);
        });
        Assert.Equal(1, balls[0].Vx);
        Assert.Equal(-1, balls[1].Vx);
        Assert.Equal(0, paddleExtendTimer);
        Assert.Equal(10, paddleX);
        Assert.Equal(6, paddleWidth);
    }

    [Fact]
    public void ActivatePowerUp_PaddleExpand_ClampsAndExtendsPaddle()
    {
        var balls = new List<Ball>();
        var powerUp = new PowerUp(0, 0, PowerUpType.PaddleExpand);
        var paddleX = 2;
        var paddleWidth = 6;
        var paddleExtendTimer = 0;

        PowerUpLogic.ActivatePowerUp(powerUp, balls, ref paddleX, ref paddleWidth, ref paddleExtendTimer, paddleY: 20);

        Assert.Empty(balls);
        Assert.Equal(15, paddleWidth);
        Assert.Equal(1, paddleX); // clamped to screen
        Assert.Equal(600, paddleExtendTimer);
    }

    [Fact]
    public void ActivatePowerUp_PaddleExpandOnlyResetsTimerWhenAlreadyWide()
    {
        var balls = new List<Ball>();
        var powerUp = new PowerUp(0, 0, PowerUpType.PaddleExpand);
        var paddleX = 5;
        var paddleWidth = 20;
        var paddleExtendTimer = 0;

        PowerUpLogic.ActivatePowerUp(powerUp, balls, ref paddleX, ref paddleWidth, ref paddleExtendTimer, paddleY: 20);

        Assert.Equal(20, paddleWidth);
        Assert.Equal(5, paddleX);
        Assert.Equal(600, paddleExtendTimer);
    }
}
