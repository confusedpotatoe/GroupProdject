namespace BrickBreaker.Game
{
    public enum BallColor { Default, Rainbow }
    public enum BallType { Ball }


    public class Ball
    {
        public int X, Y;
        public double Vx, VxCarry;
        public int Dy;
        public int Size = 1;
        public BallColor Color = BallColor.Default;
        public int BigBallTicks = 0; // counts how many update ticks left for big ball effect


        public Ball(int x, int y, double vx, int dy)
        {
            X = x;
            Y = y;
            Vx = vx;
            Dy = dy;
            VxCarry = 0;
        }
    }
}
