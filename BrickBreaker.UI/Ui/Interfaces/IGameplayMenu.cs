using BrickBreaker.UI.Ui.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.UI.Ui.Interfaces
{
    public interface IGameplayMenu
    {
        GameplayMenuChoice Show(string username);
    }
}
