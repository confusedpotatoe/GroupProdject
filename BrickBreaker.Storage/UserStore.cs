using BrickBreaker.Logic.Abstractions;
using BrickBreaker.Models;
using System.Text.Json;
using Npgsql;

namespace BrickBreaker.Storage;

public sealed class UserStore : IUserStore
{

    private const string TableName = "users";
    private readonly string _connectionString;
    public UserStore(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A PostGreSQL connection string is required.", nameof(connectionString));
        }
        _connectionString = connectionString;
    }


    public bool Exists(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }
        const string sql =
             $"""
             SELECT 1
             FROM {TableName}
             WHERE LOWER(username) = LOWER(@username)
             LIMIT 1;
             """;
        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("username", username.Trim());
        connection.Open();
        return command.ExecuteScalar() is not null;
    }
    public void Add(User user)
    {
        if (user is null) throw new ArgumentException(nameof(user));

        var username = (user.Username ?? string.Empty).Trim();
        var password = (user.Password ?? string.Empty).Trim();
        if (username.Length == 0 || password.Length == 0)
        {
            throw new InvalidOperationException("Username and password are required!");
        }

        const string sql =
            $""" 
            INSERT INTO {TableName} (username, password)
            VALUES (@username, @password)
            ON CONFLICT (username) DO NOTHING;
            """;
        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("username", username);
        command.Parameters.AddWithValue("password", password);

        connection.Open();
        command.ExecuteNonQuery();
    }
    public User? Get(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }
        const string sql = $"""
        SELECT username, password
        FROM {TableName}
        WHERE LOWER(username) = LOWER(@username)
        LIMIT 1;
        """;

        using var connection = new NpgsqlConnection(_connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("username", username.Trim());

        connection.Open();
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        var storedUsername = reader.GetString(reader.GetOrdinal("username"));
        var storedPassword = reader.GetString(reader.GetOrdinal("password"));
        return new User(storedUsername, storedPassword);

    }
}
