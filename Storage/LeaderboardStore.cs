using BrickBreaker.Models; 
using System.Collections.Generic; 
using System.IO; 
using System.Text.Json; 

public sealed class LeaderboardStore
{
    private readonly string _path; // The path to the leaderboard JSON file, set in the constructor 

    public LeaderboardStore(string path)
    {
        _path = path; // Stores the specified file path in the private field (((_for later use))) :D
    }

    public void Add(ScoreEntry entry)
    {
        var entries = ReadAll(); // Reads the current list of score entries from the fil
        entries.Add(entry); // Adds the new score entry to the list 
        // Serializes the updated list of score entries to a JSON string with indentation for readability
        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        // Writes the JSON string to the file (creates or replaces the file)
        File.WriteAllText(_path, json);
    }

    public List<ScoreEntry> ReadAll()
    {
        // If the file does not exist, return an empty list
        if (!File.Exists(_path))
        {
            return new List<ScoreEntry>();
        }

        var json = File.ReadAllText(_path); // Reads the entire file contents as a string
        // Deserializes the JSON string back into a list of ScoreEntry objects
        return JsonSerializer.Deserialize<List<ScoreEntry>>(json) ?? new List<ScoreEntry>();
    }
}
