using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;

namespace BrickBreaker.Logic;

public sealed class Auth
{
    private readonly IUserStore _users;
    public Auth(IUserStore users) => _users = users;

    public bool Register(string username, string password)
    {
        username = (username ?? "").Trim();
        password = (password ?? "").Trim();
        if (username.Length == 0 || password.Length == 0) return false;
        if (_users.Exists(username)) return false;

        _users.Add(new User(username, password));
        return true;
    }

    public bool Login(string username, string password)
    {
        username = (username ?? "").Trim();
        var u = _users.Get(username);
        return u is not null && u.Password == password;
    }
}
