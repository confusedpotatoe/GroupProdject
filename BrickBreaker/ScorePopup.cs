using System;
using System.Drawing;

public class ScorePopup
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Value { get; private set; }
    public int Lifetime { get; private set; } = 30; // Slightly shorter life for snappiness
    private int _age = 0;
    private float _riseSpeed = 2.0f; // Slightly faster rise

    public ScorePopup(int x, int y, int value)
    {
        X = x;
        Y = y;
        Value = value;
    }

    public void Update()
    {
        Y -= (int)_riseSpeed;
        _age++;
    }

    public bool IsAlive => _age < Lifetime;

    public void Draw(Graphics g)
    {
        // Calculate transparency
        int alpha = 255;
        if (_age > Lifetime - 10)
            alpha = (int)(255 * ((float)(Lifetime - _age) / 10f));

        // Create colors
        Color mainColor = Color.FromArgb(alpha, Color.Yellow);
        Color shadowColor = Color.FromArgb(alpha, Color.Black);

        using (Font font = new Font("Arial", 14, FontStyle.Bold))
        {
            string text = "+" + Value;

            // 1. Draw Shadow (offset by 2 pixels)
            using (Brush shadowBrush = new SolidBrush(shadowColor))
            {
                g.DrawString(text, font, shadowBrush, X + 2, Y + 2);
            }

            // 2. Draw Main Text
            using (Brush mainBrush = new SolidBrush(mainColor))
            {
                g.DrawString(text, font, mainBrush, X, Y);
            }
        }
    }
}