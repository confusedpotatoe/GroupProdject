using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace BrickBreaker.Storage;

public sealed class LeaderboardStore : ILeaderboardStore
{
    private const string TableName = "leaderboard";
    private readonly string _connectionString;

    public LeaderboardStore(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A PostgreSQL connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }
    public void Add(ScoreEntry entry)
    {
       if (entry is null) throw new ArgumentException(nameof(entry));

        const string sql = $"""
        INSERT INTO {TableName} (username, score, at)
        VALUES (@username, @score, @at);
        """;

        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("username", (entry.Username ?? string.Empty).Trim());
        command.Parameters.AddWithValue("score", entry.Score);
        command.Parameters.AddWithValue("at", NpgsqlDbType.TimestampTz, entry.At.UtcDateTime);

        connection.Open();
        command.ExecuteNonQuery();
    }

    public List<ScoreEntry> ReadAll()
    {
        const string sql = $"""
    SELECT username, score, at
    FROM {TableName};
    """;

        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);

        connection.Open();

        using var reader = command.ExecuteReader();
        var entries = new List<ScoreEntry>();

        while (reader.Read())
        {
            var username = reader.GetString(reader.GetOrdinal("username"));
            var score = reader.GetInt32(reader.GetOrdinal("score"));

            var atOrdinal = reader.GetOrdinal("at");
            var atValue = reader.GetFieldValue<DateTime>(atOrdinal);
            var at = DateTime.SpecifyKind(atValue, DateTimeKind.Utc);

            entries.Add(new ScoreEntry(username, score, new DateTimeOffset(at)));
        }
        return entries;
    }
}
