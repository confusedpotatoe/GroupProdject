using BrickBreaker.UI.Game.Models;

namespace BrickBreaker.Game.Systems
{
    public static class PowerUpLogic
    {
        
        public static void ActivatePowerUp(
            PowerUp pu,
            List<Ball> balls,
            ref int paddleX,       
            ref int paddleWidth,    
            ref int paddleExtendTimer, 
            int paddleY)
        {
            if (pu.Type == PowerUpType.MultiBall)
            {
                int paddleCenter = paddleX + paddleWidth / 2;
                // Create Ball 1
                var ball1 = new Ball(paddleCenter, paddleY - 1, 0, 0, true);
                ball1.SetHorizontalVelocity(1, 0);  // Go right
                ball1.SetVerticalVelocity(-1); // Go up

                // Create Ball 2
                var ball2 = new Ball(paddleCenter, paddleY - 1, 0, 0, true);
                ball2.SetHorizontalVelocity(-1, 0); // Go left
                ball2.SetVerticalVelocity(-1);  // Go up

                // Add the new, moving balls to the list
                balls.Add(ball1);
                balls.Add(ball2);
            }
            else if (pu.Type == PowerUpType.PaddleExpand)
            {
                const int newWidth = 15; // Set the new expanded width
                const int durationInFrames = 600; // 10 seconds at 60fps

                if (paddleWidth < newWidth)
                {
                    // Calculate how much the paddle grew and shift it left
                    int widthDifference = newWidth - paddleWidth;
                    paddleX -= widthDifference / 2;
                    paddleWidth = newWidth;

                    // Safety clamp to keep it on screen
                    if (paddleX < 1) paddleX = 1;
                }
                // Set (or reset) the timer
                paddleExtendTimer = durationInFrames;
            }
        }
    }
}