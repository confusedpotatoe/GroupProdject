using System.Security.Cryptography;

namespace BrickBreaker.Logic;

// Provides PBKDF2-based hashing helpers used by the storage layer to persist credentials.
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    public const int DefaultIterations = 100_000;
    public const string AlgorithmName = "PBKDF2";

    // Hashes the provided password and returns the algorithm/iteration/salt/hash tuple.
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(password));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Pbkdf2(password, salt, DefaultIterations, KeySize);
        var saltText = Convert.ToBase64String(salt);
        var keyText = Convert.ToBase64String(key);
        return Compose(new HashComponents(AlgorithmName, DefaultIterations, saltText, keyText));
    }

    // Compares a password against a stored hash using constant-time equality.
    public static bool Verify(string hashedPassword, string password)
    {
        if (!TryParse(hashedPassword, out var components) || password is null)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(components.Salt);
            var expectedKey = Convert.FromBase64String(components.Hash);
            var actualKey = Pbkdf2(password, salt, components.Iterations, expectedKey.Length);
            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static bool TryParse(string hashedPassword, out HashComponents components)
    {
        components = default;
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            return false;
        }

        var segments = hashedPassword.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 4 || segments[0] != AlgorithmName)
        {
            return false;
        }

        if (!int.TryParse(segments[1], out var iterations))
        {
            return false;
        }

        components = new HashComponents(segments[0], iterations, segments[2], segments[3]);
        return true;
    }

    public static string Compose(HashComponents components)
    {
        return $"{components.Algorithm}${components.Iterations}${components.Salt}${components.Hash}";
    }

    public readonly record struct HashComponents(string Algorithm, int Iterations, string Salt, string Hash)
    {
        public bool IsValid => !string.IsNullOrWhiteSpace(Algorithm) &&
                               !string.IsNullOrWhiteSpace(Salt) &&
                               !string.IsNullOrWhiteSpace(Hash) &&
                               Iterations > 0;
    }

    private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int outputBytes)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(outputBytes);
    }
}

