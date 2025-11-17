namespace BrickBreaker.UI.Game.Models
{
    public enum PowerUpType
    {
        MultiBall,
        PaddleExpand // ADDED A NEW POWER-UP TYPE
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