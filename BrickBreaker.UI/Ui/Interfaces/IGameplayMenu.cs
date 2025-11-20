using BrickBreaker.UI.Ui.Enums;

namespace BrickBreaker.UI.Ui.Interfaces
{
    // interface for displaying the gameplay menu
    // handles user choices related to gameplay options
    public interface IGameplayMenu
    {
        // shows the gameplay menu and returns the user's choice
        GameplayMenuChoice Show(string username);
    }
}
