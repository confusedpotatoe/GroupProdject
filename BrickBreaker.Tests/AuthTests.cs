using BrickBreaker.Logic;
using BrickBreaker.Models;

namespace BrickBreaker.Tests;

public sealed class AuthTests
{
    [Fact]
    public void UsernameExists_TrimsInputBeforeDelegating()
    {
        var store = new FakeUserStore();
        store.Seed(new User("Alice", "secret"));
        var sut = new Auth(store);

        var exists = sut.UsernameExists("  Alice  ");

        Assert.True(exists);
        Assert.Equal("Alice", store.LastExistsUsername);
    }

    [Fact]
    public void Register_Fails_WhenUsernameAlreadyExists()
    {
        // Username is already seeded, so Register should reject the duplicate.
        var store = new FakeUserStore();
        store.Seed(new User("Bob", "pw"));
        var sut = new Auth(store);

        var result = sut.Register("Bob", "newpw");

        Assert.False(result);
        Assert.Empty(store.AddedUsers);
    }

    [Fact]
    public void Register_Fails_WhenUsernameMissing()
    {
        // Whitespace usernames are invalid.
        var sut = new Auth(new FakeUserStore());

        var result = sut.Register("   ", "pw");

        Assert.False(result);
    }

    [Fact]
    public void Register_Fails_WhenPasswordMissing()
    {
        // Password cannot be empty, so the call should fail.
        var sut = new Auth(new FakeUserStore());

        var result = sut.Register("Alice", "");

        Assert.False(result);
    }

    [Fact]
    public void Register_Succeeds_AndPersistsTrimmedUser()
    {
        // Valid username/password should succeed and be stored without extra whitespace.
        var store = new FakeUserStore();
        var sut = new Auth(store);

        var result = sut.Register("  Alice  ", "  pw  ");

        Assert.True(result);
        var savedUser = Assert.Single(store.AddedUsers);
        Assert.Equal("Alice", savedUser.Username);
        Assert.Equal("pw", savedUser.Password);
    }

    [Fact]
    public void Login_ReturnsFalse_WhenUserMissing()
    {
        // If the backing store does not contain the user, login should fail.
        var sut = new Auth(new FakeUserStore());

        var loggedIn = sut.Login("unknown", "pw");

        Assert.False(loggedIn);
    }

    [Fact]
    public void Login_ReturnsFalse_WhenPasswordMismatch()
    {
        // Valid username but wrong password should fail to log in.
        var store = new FakeUserStore();
        store.Seed(new User("Alice", "pw"));
        var sut = new Auth(store);

        var loggedIn = sut.Login("Alice", "wrong");

        Assert.False(loggedIn);
    }

    [Fact]
    public void Login_ReturnsTrue_WhenCredentialsMatch()
    {
        // Exact match of username/password should succeed.
        var store = new FakeUserStore();
        store.Seed(new User("Alice", "pw"));
        var sut = new Auth(store);

        var loggedIn = sut.Login("Alice", "pw");

        Assert.True(loggedIn);
    }

    [Fact]
    public void Login_TrimsUsernameBeforeLookup()
    {
        // Ensure the repository is called with a trimmed username.
        var store = new FakeUserStore();
        store.Seed(new User("Alice", "pw"));
        var sut = new Auth(store);

        _ = sut.Login("  Alice  ", "pw");

        Assert.Equal("Alice", store.LastGetUsername);
    }
}
