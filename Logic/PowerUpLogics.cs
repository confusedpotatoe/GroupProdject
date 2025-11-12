
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



            }
        }
    }
}
