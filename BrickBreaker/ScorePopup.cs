using System;
using System.Drawing;

public class ScorePopup
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Value { get; private set; }
    public int Lifetime { get; private set; } = 40; // ticks to display (adjust as you like)
    private int _age = 0;
    private float _riseSpeed = 1.5f;

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
        // Fade out near end of life
        int alpha = 255;
        if (_age > Lifetime - 10)
            alpha = (int)(255 * ((float)(Lifetime - _age) / 10f));

        Color color = Color.FromArgb(alpha, Color.Yellow);
        using (Brush brush = new SolidBrush(color))
        {
            g.DrawString("+" + Value,
                         new Font("Arial", 13, FontStyle.Bold),
                         brush, X, Y);
        }
    }
}
