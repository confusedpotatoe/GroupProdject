namespace BrickBreaker.UI.Game.Models
{
    public enum BallType { Ball }

    public class Ball
    {
        
        public int X { get; private set; }
        public int Y { get; private set; }
        public double Vx { get; private set; }
        public double VxCarry { get; private set; }
        public int Dy { get; private set; }
        public bool IsMultiball { get; }

        public Ball(int x, int y, double vx, int dy, bool isMultiball = false)
        {
            X = x;
            Y = y;
            Vx = vx;
            VxCarry = 0;
            Dy = dy;
            IsMultiball = isMultiball;
        }

        // NEW PUBLIC METHODS
        // These methods are how other classes (like CollisionHandler) 
        // will tell the ball to change its state.

        public void SetPosition(int newX, int newY)
        {
            X = newX;
            Y = newY;
        }

        public void InvertHorizontalVelocity()
        {
            Vx = -Vx;
            VxCarry = -VxCarry;
        }

        public void InvertVerticalVelocity()
        {
            Dy = -Dy;
        }

        public void SetHorizontalVelocity(double newVx, double newVxCarry)
        {
            Vx = newVx;
            VxCarry = newVxCarry;
        }

        public void SetVerticalVelocity(int dy)
        {
            Dy = dy;
        }

        // This method lets the CollisionHandler update the velocity carry
        public void UpdateAndGetDx(out int dxStep)
        {
            VxCarry += Vx;
            dxStep = 0;
            while (VxCarry >= 1) { VxCarry -= 1; dxStep++; }
            while (VxCarry <= -1) { VxCarry += 1; dxStep--; }
        }
    }
}