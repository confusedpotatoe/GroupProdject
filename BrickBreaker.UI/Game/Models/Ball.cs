namespace BrickBreaker.UI.Game.Models
{
    public enum BallType { Ball }

    public class Ball
    {

        public int X { get; private set; } // Ball's horizontal position
        public int Y { get; private set; } // Ball's vertical position
        public double Vx { get; private set; } // Ball's horizontal velocity
        public double VxCarry { get; private set; } // Carry-over for sub-pixel horizontal movement
        public int Dy { get; private set; } // Ball's vertical velocity
        public bool IsMultiball { get; } // Indicates if the ball is part of a multiball event

        public Ball(int x, int y, double vx, int dy, bool isMultiball = false) // Constructor to initialize ball properties
        {
            X = x; // Set initial horizontal position
            Y = y; // Set initial vertical position
            Vx = vx; // Set initial horizontal velocity
            VxCarry = 0; // Initialize carry-over to zero
            Dy = dy; // Set initial vertical velocity
            IsMultiball = isMultiball; // Set multiball status
        }

        // NEW PUBLIC METHODS
        // These methods are how other classes (like CollisionHandler) 
        // will tell the ball to change its state.

        public void SetPosition(int newX, int newY) // Sets the ball's position to new coordinates
        {
            X = newX; // Update horizontal position
            Y = newY; // Update vertical position
        }

        public void InvertHorizontalVelocity() // Inverts the ball's horizontal velocity
        {
            Vx = -Vx; // Reverse horizontal velocity
            VxCarry = -VxCarry; // Reverse carry-over for horizontal movement
        }

        public void InvertVerticalVelocity() // Inverts the ball's vertical velocity
        {
            Dy = -Dy; // Reverse vertical velocity
        }

        public void SetHorizontalVelocity(double newVx, double newVxCarry) // Sets the ball's horizontal velocity and carry-over
        {
            Vx = newVx; // Update horizontal velocity
            VxCarry = newVxCarry; // Update carry-over for horizontal movement
        }

        public void SetVerticalVelocity(int dy) // Sets the ball's vertical velocity
        {
            Dy = dy; // Update vertical velocity
        }

        // This method lets the CollisionHandler update the velocity carry
        public void UpdateAndGetDx(out int dxStep) // Updates the ball's horizontal position based on velocity and carry-over
        {
            VxCarry += Vx; // Add horizontal velocity to carry-over
            dxStep = 0; // Initialize horizontal step to zero
            while (VxCarry >= 1) { VxCarry -= 1; dxStep++; } // Move right if carry-over is positive
            while (VxCarry <= -1) { VxCarry += 1; dxStep--; } // Move left if carry-over is negative
        }
    }
}