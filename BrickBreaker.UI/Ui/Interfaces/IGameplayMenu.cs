using BrickBreaker.UI.Ui.Enums;

namespace BrickBreaker.UI.Ui.Interfaces
{
    // interface for displaying the gameplay menu
    public interface IGameplayMenu
    {
        GameplayMenuChoice Show(string username);
    }
}
