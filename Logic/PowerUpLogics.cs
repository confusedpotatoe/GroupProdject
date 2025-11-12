
using BrickBreaker.Game;
using System.Collections.Generic;

namespace BrickBreaker.Logics
{
    public static class PowerUpLogic
    {
        public static void ActivatePowerUp(PowerUp powerUp, List<Ball> balls, int paddleX, int paddleY)
        {
            switch (powerUp.Type)
            {
                case PowerUpType.MultiBall:
                    balls.Add(new Ball(
                        paddleX + 4, // PaddleW = 9, so /2 is 4
                        paddleY - 1,
                        1,
                        -1
                    ));
                    break;

                    //case PowerUpType.BigBall:
                    //    //if (targetBall != null)
                    //    {
                    //     //   targetBall.Size = 3;
                    //       // targetBall.BigBallTicks = 300; // ~10 seconds if Update runs every 33 ms
                    //    }
                    //    break;
                    //case PowerUpType.FastBall:
                    //    foreach (var ball in balls) ball.Vx *= 1.5;
                    //    break;
                    //case PowerUpType.RainbowBall:
                    //    foreach (var ball in balls) ball.Color = BallColor.Rainbow;
                    //    break;


            }
        }
    }
}
