using BrickBreaker.Models;

namespace BrickBreaker.Logic.Abstractions;

public interface IUserStore
{
    bool Exists(string username);
    void Add(User user);
    User? Get(string username);
}
