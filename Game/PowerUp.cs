namespace BrickBreaker.Game
{
    public enum PowerUpType
    {
        MultiBall, // Splits the ball into multiple balls
        BigBall, // Increases the size of the ball
        FastBall, // Increases the speed of the ball
        RainbowBall // Changes the ball color to rainbow
        // Add more types as needed!
    }

    public class PowerUp
    {
        public int X, Y;
        public PowerUpType Type;

        public PowerUp(int x, int y, PowerUpType type)
        {
            X = x;
            Y = y;
            Type = type;
        }
    }
}
