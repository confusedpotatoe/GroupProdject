namespace BrickBreaker.Models;

public sealed class User //define the attributes the class has
{
    public required string Username { get; set; }
    public required string Password { get; set; }

    public User(string username, string password) //constructer for the class
    {
        Username = username;
        Password = password;
    }
    public User() { }
}
