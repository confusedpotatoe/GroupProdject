using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickBreaker.Ui
{
    public interface IGameplayMenu
    {
        GameplayMenuChoice Show(string currentUser);
    }
}
