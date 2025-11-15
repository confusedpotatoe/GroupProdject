namespace BrickBreaker.Game
{
    public enum PowerUpType
    {
        MultiBall
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