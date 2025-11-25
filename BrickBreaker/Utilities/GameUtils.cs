using System.Drawing;
using BrickBreaker.Entities;

namespace BrickBreaker.Utilities
{
    public static class GameUtils
    {
        public static bool CheckCollision(Ball ball, Rectangle rect) // Axis-Aligned Bounding Box (AABB) collision detection
        {
            return ball.X + ball.Radius * 2 >= rect.X && // Right edge of ball >= Left edge of rectangle
                   ball.X <= rect.X + rect.Width && // Left edge of ball <= Right edge of rectangle
                   ball.Y + ball.Radius * 2 >= rect.Y && // Bottom edge of ball >= Top edge of rectangle
                   ball.Y <= rect.Y + rect.Height; // Top edge of ball <= Bottom edge of rectangle
        }
    }
}