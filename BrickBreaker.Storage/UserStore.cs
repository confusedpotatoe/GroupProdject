using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;
using System.Text.Json;

namespace BrickBreaker.Storage;

public sealed class UserStore : IUserStore
{

    private readonly string _path;
    private static readonly JsonSerializerOptions _jsonoptions = new()
    {
        WriteIndented = true
    };
    public UserStore(string path) => _path = path;

   
    public bool Exists(string username)
    {
        var users = ReadAll();

        return users.Any(u => u.Username != null && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }
    public void Add(User user)
    {
        var users = ReadAll();
        users.Add(user);
        WriteAll(users);
        
    }
    public User? Get(string username)
    {
        var users = ReadAll();

        return users.FirstOrDefault(u => u.Username != null && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public List<User> ReadAll()
    {
        if (!File.Exists(_path))
            return new List<User>();

        try
        {
            var json = File.ReadAllText(_path);
            if (string.IsNullOrWhiteSpace(json))
                return new List<User>();

            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }
        catch (JsonException)
        {
            Console.WriteLine("UserStore: users.json is invalid or corrupted. Returning an empty list.");
            return new List<User>();
        }
    }


    private void WriteAll(List<User> users)
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(users, _jsonoptions);

        File.WriteAllText(_path, json);
    }
}

