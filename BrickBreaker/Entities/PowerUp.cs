using System.Drawing;

namespace BrickBreaker.Entities
{
    public enum PowerUpType
    {
        Multiball,
        PaddleExtender
    }

    public class PowerUp
    {
        public int X, Y;
        public PowerUpType Type;
        public int Width = 30;  // Consistent size for Hitbox and Drawing
        public int Height = 30;

        public PowerUp(int x, int y, PowerUpType type)
        {
            X = x;
            Y = y;
            Type = type;
        }

        public void UpdatePosition()
        {
            Y += 3; // Fall speed
        }

        // This allows the PowerUp to draw itself
        public void Draw(Graphics g, Font font)
        {
            Brush brush = Type switch
            {
                PowerUpType.Multiball => Brushes.Yellow,
                PowerUpType.PaddleExtender => Brushes.Cyan,
                _ => Brushes.White
            };

            // 1. Draw the circle background
            g.FillEllipse(brush, X, Y, Width, Height);
            g.DrawEllipse(Pens.Black, X, Y, Width, Height);

            // Setup centering format
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;      // Horizontal Center
                sf.LineAlignment = StringAlignment.Center;  // Vertical Center

                // Define the rectangle where the text sits (same as the ball)
                RectangleF rect = new RectangleF(X, Y, Width, Height);

                // Draw the text inside that rectangle using the format
                string letter = Type == PowerUpType.Multiball ? "M" : "E";

                // Draw the letter centered in the circle
                g.DrawString(letter, font, Brushes.Black, rect, sf);
            }
        }
    }
}