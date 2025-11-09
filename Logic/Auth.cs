using BrickBreaker.Models;
using BrickBreaker.Storage;

namespace BrickBreaker.Logic;

public sealed class Auth
{
    private readonly UserStore _users;
    public Auth(UserStore users) => _users = users;

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
