using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;

namespace BrickBreaker.Logic;

public sealed class Auth
{
    private readonly IUserStore _users;
    public Auth(IUserStore users) => _users = users;

   
    public bool UsernameExists(string username)  // Checks if username already exists
    {
        username = (username ?? "").Trim();
        return _users.Exists(username);
    }

    public bool Register(string username, string password)
    {
        username = (username ?? "").Trim();
        if (_users.Exists(username) || username.Length == 0) return false; //uses the method above to check if the username already exists

        password = (password ?? "").Trim();
        if (password.Length == 0) return false;


        var hashedPassword = PasswordHasher.HashPassword(password);
        _users.Add(new User(username, hashedPassword));
        return true;
    }

    public bool Login(string username, string password) //methos that checks that the username exists, if it does it checks if the username and password match
    {
        username = (username ?? "").Trim();
        var u = _users.Get(username);
        return u is not null && PasswordHasher.Verify(u.Password, password ?? string.Empty);
    }
}
