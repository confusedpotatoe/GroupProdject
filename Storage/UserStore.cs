using BrickBreaker.Models;
using Newtonsoft.Json;
using System.Security;
using static System.Runtime.InteropServices.JavaScript.JSType;

public sealed class UserStore
{

    string jsonPath = Path.Combine("..", "..", "..", "data\\users.json");

    private readonly string _path;
    public UserStore(string path)
    { 
        _path = path;
    }
    public bool Exists(string username)
    {
        string json = File.ReadAllText(jsonPath);

        List<User> users = JsonConvert.DeserializeObject<List<User>>(json);
        
        foreach (var user in users)
        {
            if (user.Username != null && user.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
        
    }
    public void Add(User user)
    {
        string json = File.ReadAllText(jsonPath);

        List<User> users = JsonConvert.DeserializeObject<List<User>>(json);

        Console.WriteLine("Choose as username (Minimum 3 character)");
        string InputUserName = Console.ReadLine();

        if (InputUserName.Length < 3)
        {
            Console.WriteLine("Username is too short...");
        }
        else
        {
            Console.WriteLine($"Saving username: {InputUserName}!");

            users.Add(new User(InputUserName, ""));

            string updatedJson = JsonConvert.SerializeObject(users, Formatting.Indented);

            File.WriteAllText(jsonPath, updatedJson);
        }
    }
    public User? Get(string username)
    {
        string json = File.ReadAllText(_path);
        List<User> users = JsonConvert.DeserializeObject<List<User>>(json);

        User? user = users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        return user; 
    }
    public List<User> ReadAll()
    {
        string json = File.ReadAllText(jsonPath);

        if (!File.Exists(_path))
        {
            Console.WriteLine("No file found");
            return new List<User>();
        }

        
        if (string.IsNullOrWhiteSpace(json))
        {
            Console.WriteLine("File is empty");
            return new List<User>();
        }

        List<User> users = JsonConvert.DeserializeObject<List<User>>(json);
        return users ?? new List<User>();

    }
}

