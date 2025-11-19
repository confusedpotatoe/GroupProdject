using System.Drawing;

public enum PowerUpType
{
    Multiball,
    PaddleExtender
}

public class PowerUp
{
    public int X;
    public int Y;
    public int Size = 20;
    public PowerUpType Type;
    public int Speed = 4;  // Falling speed in pixels per update
    public int Width { get; set; } = 30;   // Default size, tweak if needed
    public int Height { get; set; } = 30;  // Default size, tweak if needed

    public PowerUp(int x, int y, PowerUpType type)
    {
        X = x;
        Y = y;
        Type = type;
    }

    public void UpdatePosition()
    {
        Y += Speed; // Move down each update
    }

    public void Draw(Graphics g)
    {
        Brush brush;
        switch (Type)
        {
            case PowerUpType.Multiball:
                brush = Brushes.Yellow;
                break;
            case PowerUpType.PaddleExtender:
                brush = Brushes.Cyan;
                break;
            default:
                brush = Brushes.White;
                break;
        }
        g.FillEllipse(brush, X, Y, Size, Size);
    }
}


