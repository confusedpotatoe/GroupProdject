using BrickBreaker.UI.Ui.Enums;

namespace BrickBreaker.UI.Ui.Interfaces
{
    // interface for displaying the login menu
    // handles user choices related to login options
    public interface ILoginMenu
    {
        // shows the login menu and returns the user's choice
        LoginMenuChoice Show();
    }
}
