using System;
using System.Drawing;
namespace BrickBreaker.Entities
{
    public class ScorePopup
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Value { get; private set; }
        private string? CustomText { get; set; } // Optional custom text for multiplier
        public int Lifetime { get; private set; } = 30; // Slightly shorter life for snappiness
        private int _age = 0;
        private float _riseSpeed = 2.0f; // Slightly faster rise

        // Constructor for score popup (numeric)
        public ScorePopup(int x, int y, int value)
        {
            X = x;
            Y = y;
            Value = value;
            CustomText = null;
        }

        // Overloaded constructor for custom text (e.g., "x2")
        public ScorePopup(int x, int y, string text)
        {
            X = x;
            Y = y;
            Value = 0;
            CustomText = text;
        }

        public void Update()
        {
            Y -= (int)_riseSpeed;
            _age++;
        }
        // Add this inside ScorePopup class
        public void Shift(int dx, int dy)
        {
            X += dx;
            Y += dy;
        }

        public bool IsAlive => _age < Lifetime; // Slightly shorter lifetime

        public void Draw(Graphics g) // Adjusted drawing for visibility
        {
            int alpha = 255; // Full opacity
            if (_age > Lifetime - 10) // Fade out in the last 10 frames
                alpha = (int)(255 * ((float)(Lifetime - _age) / 10f)); // Faster fade out

            // Color for multiplier or score
            Color mainColor = CustomText != null ? Color.FromArgb(alpha, Color.OrangeRed) : Color.FromArgb(alpha, Color.Yellow);
            Color shadowColor = Color.FromArgb(alpha, Color.Black);

            using (Font font = new Font("Arial", 14, FontStyle.Bold))
            {
                string text = CustomText ?? ("+" + Value.ToString());

                using (Brush shadowBrush = new SolidBrush(shadowColor))
                    g.DrawString(text, font, shadowBrush, X + 2, Y + 2);

                using (Brush mainBrush = new SolidBrush(mainColor))
                    g.DrawString(text, font, mainBrush, X, Y);
            }
        }
    }
}
    
