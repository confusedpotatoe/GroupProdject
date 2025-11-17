namespace BrickBreaker.UI.Game.Infrastructure
{
    public interface IKeyboard
    {
        bool IsLeftPressed();
        bool IsRightPressed();
        bool IsEscapePressed();
        bool IsSpacePressed();
        bool IsUpPressed();
    }

}
