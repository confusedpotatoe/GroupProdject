using System.Drawing;
using BrickBreaker.Entities;

namespace BrickBreaker.Utilities
{
    public static class GameUtils
    {
        public static bool CheckCollision(Ball ball, Rectangle rect)
        {
            return ball.X + ball.Radius * 2 >= rect.X &&
                   ball.X <= rect.X + rect.Width &&
                   ball.Y + ball.Radius * 2 >= rect.Y &&
                   ball.Y <= rect.Y + rect.Height;
        }
    }
}