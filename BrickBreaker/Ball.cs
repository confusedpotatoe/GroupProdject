public class Ball
{
    public int X { get; set; }
    public int Y { get; set; }
    public double VX { get; set; }
    public double VY { get; set; }
    public int Radius { get; set; }

    public int BrickStreak { get; set; } = 0;
    public int Multiplier { get; set; } = 1;

    public Ball(int x, int y, double vx, double vy, int radius)
    {
        X = x;
        Y = y;
        VX = vx;
        VY = vy;
        Radius = radius;
    }

    public void UpdatePosition()
    {
        X += (int)VX;
        Y += (int)VY;
    }

    public void InvertVerticalVelocity()
    {
        VY = -VY;
    }

    public void InvertHorizontalVelocity()
    {
        VX = -VX;
    }

    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }
}
