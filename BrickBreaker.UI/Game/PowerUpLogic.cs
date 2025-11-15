using System.Collections.Generic;

namespace BrickBreaker.Game;

public static class PowerUpLogic
{
    public static void ActivatePowerUp(PowerUp pu, List<Ball> balls, int paddleX, int paddleY)
    {
        if (pu.Type == PowerUpType.MultiBall)
        {
            // Add TWO balls, both start at the center/top of the paddle
            balls.Add(new Ball(paddleX + 4, paddleY - 1, 1, -1, true));   // Ball 1 (angles can be tweaked)
            balls.Add(new Ball(paddleX + 4, paddleY - 1, -1, -1, true));  // Ball 2 (opposite vx)
        }
    }
}
