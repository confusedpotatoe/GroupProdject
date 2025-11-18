using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;

namespace BrickBreaker.Logic;

public sealed class Auth
{
    private readonly IUserStore _users;
    public Auth(IUserStore users) => _users = users;

    // Checks if username already exists
    public bool UsernameExists(string username)
    {
        username = (username ?? "").Trim();
        return _users.Exists(username);
    }

    public bool Register(string username, string password)
    {
        username = (username ?? "").Trim();
        if (_users.Exists(username) || username.Length == 0) return false;

        password = (password ?? "").Trim();
        if (password.Length == 0) return false;
        

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
