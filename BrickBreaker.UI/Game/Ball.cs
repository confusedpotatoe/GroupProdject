namespace BrickBreaker.Game
{
    public enum BallType { Ball }


    public class Ball
    {
        public int X, Y;
        public double Vx, VxCarry;
        public int Dy;
        public bool IsMultiball;

        public Ball(int x, int y, double vx, int dy, bool isMultiball = false)
        {
            X = x;
            Y = y;
            Vx = vx;
            VxCarry = 0;
            Dy = dy;
            IsMultiball = isMultiball;
        }
    }
}