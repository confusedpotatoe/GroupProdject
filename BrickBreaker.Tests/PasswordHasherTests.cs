using BrickBreaker.Logic;

namespace BrickBreaker.Tests;

public sealed class PasswordHasherTests
{
    [Fact]
    public void HashPassword_RoundTripsUsingVerify()
    {
        var hashed = PasswordHasher.HashPassword("s3cret!");

        Assert.False(string.IsNullOrWhiteSpace(hashed));
        Assert.StartsWith($"{PasswordHasher.AlgorithmName}$", hashed);
        Assert.True(PasswordHasher.Verify(hashed, "s3cret!"));
        Assert.False(PasswordHasher.Verify(hashed, "wrong"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("PBKDF2$1000$onlythreeparts")]
    [InlineData("PBKDF2$notanumber$salt$hash")]
    [InlineData("OtherAlgo$1000$salt$hash")]
    public void Verify_ReturnsFalse_ForMalformedHashes(string malformed)
    {
        Assert.False(PasswordHasher.Verify(malformed, "pw"));
    }

    [Fact]
    public void TryParse_RejectsHashesWithUnknownAlgorithm()
    {
        var hash = "Unknown$5000$c2FsdA==$aGFzaA==";

        var parsed = PasswordHasher.TryParse(hash, out var components);

        Assert.False(parsed);
        Assert.Equal(default, components);
    }
}
