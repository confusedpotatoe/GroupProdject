using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace BrickBreaker.Storage;

public sealed class LeaderboardStore : ILeaderboardStore
{
    //Declare the table name in the database and use the connectionstrin to connect to the database
    private const string TableName = "leaderboard";
    private readonly string _connectionString;

    //Method makes sure there is always a PostGreSQL connection
    public LeaderboardStore(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A PostgreSQL connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    //Adds a highscore to the table leaderboard 
    public void Add(ScoreEntry entry)
    {
        if (entry is null) throw new ArgumentException(nameof(entry));

       //insert values into the table
        const string sql = $"""
        INSERT INTO {TableName} (username, score, at)
        VALUES (@username, @score, @at);
        """;

        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);

        //takes data from entry. and sends it to the db
        command.Parameters.AddWithValue("username", (entry.Username ?? string.Empty).Trim());
        command.Parameters.AddWithValue("score", entry.Score);
        command.Parameters.AddWithValue("at", NpgsqlDbType.TimestampTz, entry.At.UtcDateTime);

        connection.Open();
        command.ExecuteNonQuery();
    }

    //methods that reads the db using sql command select
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


        //reads data from the tables, using reader to iterate rows and GetOrdinal to find the right column/row, then the value gets read wíth GetString or GetInt32
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
