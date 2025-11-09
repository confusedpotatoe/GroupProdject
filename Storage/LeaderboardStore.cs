using BrickBreaker.Models; 
using System.Collections.Generic; 
using System.IO; 
using System.Text.Json;

namespace BrickBreaker.Storage; //namespace MISSING i think that did the big part, but some other changes aswell

public sealed class LeaderboardStore
{
    private readonly string _path;

    // Simplified constructor using expression body
    public LeaderboardStore(string path) => _path = path;

    public void Add(ScoreEntry entry)
    {
        var entries = ReadAll(); // Read existing entries
        entries.Add(entry); // Add new entry

        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });

        // Ensure the directory exists before writing the file
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir); // Create directory if it does not exist

        File.WriteAllText(_path, json); // Actually write the JSON content to file
    }

    public List<ScoreEntry> ReadAll()
    {
        if (!File.Exists(_path))
            return new List<ScoreEntry>(); // Return empty list if file missing

        var json = File.ReadAllText(_path);
        if (string.IsNullOrWhiteSpace(json))
            return new List<ScoreEntry>(); // Handle empty file gracefully

        try
        {
            return JsonSerializer.Deserialize<List<ScoreEntry>>(json) ?? new List<ScoreEntry>();
        }
        catch (JsonException)
        {
            // On invalid JSON, avoid crashing and return empty list instead
            return new List<ScoreEntry>();
        }
    }
}
