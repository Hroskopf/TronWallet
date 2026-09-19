using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using TronWallet.Core.Domain.Entities;
using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Services;

namespace TronWallet.Tests;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITronAddressService _addresses = Substitute.For<ITronAddressService>();
    private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
    private readonly IWalletRepository _wallets = Substitute.For<IWalletRepository>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly DefaultHttpContext _httpContext;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        // SignInAsync/SignOutAsync are extension methods that resolve
        // IAuthenticationService from HttpContext.RequestServices.
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IAuthenticationService))
                       .Returns(Substitute.For<IAuthenticationService>());
        _httpContext = new DefaultHttpContext { RequestServices = serviceProvider };

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(_httpContext);

        _addresses.GenerateWallet().Returns(("priv-hex", "pub-hex", "TFakeAddress"));
        _encryption.Encrypt("priv-hex").Returns("priv-enc");

        _service = new AuthService(_users, _addresses, _encryption, _wallets, _refreshTokens, accessor);
    }

    private static string Sha256Base64(string value) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private string? RefreshTokenCookieValue()
    {
        var setCookie = _httpContext.Response.Headers["Set-Cookie"]
            .FirstOrDefault(h => h!.StartsWith("refreshToken="));
        var encoded = setCookie?.Split(';')[0]["refreshToken=".Length..];
        return encoded == null ? null : Uri.UnescapeDataString(encoded);
    }

    [Fact]
    public async Task Register_WhenEmailExists_Throws()
    {
        _users.ExistsByEmailAsync("taken@x.com").Returns(true);

        await Assert.ThrowsAsync<Exception>(
            () => _service.Register("taken@x.com", "user", "password"));
    }

    [Fact]
    public async Task Register_StoresBcryptHash_NotPlaintextPassword()
    {
        User? inserted = null;
        _users.InsertAsync(Arg.Do<User>(u => inserted = u)).Returns(Guid.NewGuid());

        await _service.Register("new@x.com", "user", "s3cret");

        Assert.NotNull(inserted);
        Assert.NotEqual("s3cret", inserted!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("s3cret", inserted.PasswordHash));
    }

    [Fact]
    public async Task Register_StoresEncryptedPrivateKey()
    {
        Wallet? inserted = null;
        _wallets.InsertAsync(Arg.Do<Wallet>(w => inserted = w)).Returns(Guid.NewGuid());

        await _service.Register("new@x.com", "user", "password");

        Assert.NotNull(inserted);
        Assert.Equal("priv-enc", inserted!.PrivateKeyEnc);
        Assert.Equal("TFakeAddress", inserted.TronAddress);
    }

    [Fact]
    public async Task Register_StoresHashOfRefreshToken_NotTheTokenItself()
    {
        RefreshToken? stored = null;
        _refreshTokens.InsertAsync(Arg.Do<RefreshToken>(t => stored = t)).Returns(Task.CompletedTask);

        await _service.Register("new@x.com", "user", "password");

        var cookieToken = RefreshTokenCookieValue();
        Assert.NotNull(cookieToken);
        Assert.NotNull(stored);
        Assert.NotEqual(cookieToken, stored!.TokenHash);
        Assert.Equal(Sha256Base64(cookieToken!), stored.TokenHash);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsNull()
    {
        _users.FindUserByEmailAsync("nobody@x.com").Returns((User?)null);

        Assert.Null(await _service.Login("nobody@x.com", "password"));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsNull()
    {
        _users.FindUserByEmailAsync("user@x.com").Returns(new User
        {
            Id = Guid.NewGuid(),
            Email = "user@x.com",
            Username = "user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("right", workFactor: 4)
        });

        Assert.Null(await _service.Login("user@x.com", "wrong"));
    }

    [Fact]
    public async Task Login_CorrectPassword_ReturnsUser_AndIssuesRefreshToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@x.com",
            Username = "user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("right", workFactor: 4)
        };
        _users.FindUserByEmailAsync("user@x.com").Returns(user);

        var result = await _service.Login("user@x.com", "right");

        Assert.Same(user, result);
        await _refreshTokens.Received(1).InsertAsync(Arg.Is<RefreshToken>(t => t.UserId == user.Id));
        Assert.NotNull(RefreshTokenCookieValue());
    }

    [Fact]
    public async Task RefreshSession_WithoutCookie_ReturnsFalse()
    {
        Assert.False(await _service.RefreshSession());
    }

    private void SetRequestRefreshToken(string token) =>
        _httpContext.Request.Headers["Cookie"] = $"refreshToken={token}";

    [Fact]
    public async Task RefreshSession_ExpiredToken_ReturnsFalse()
    {
        SetRequestRefreshToken("expired-token");
        _refreshTokens.GetByHashAsync(Sha256Base64("expired-token")).Returns(new RefreshToken
        {
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        });

        Assert.False(await _service.RefreshSession());
    }

    [Fact]
    public async Task RefreshSession_RevokedToken_ReturnsFalse()
    {
        SetRequestRefreshToken("revoked-token");
        _refreshTokens.GetByHashAsync(Sha256Base64("revoked-token")).Returns(new RefreshToken
        {
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            RevokedAt = DateTime.UtcNow.AddHours(-1)
        });

        Assert.False(await _service.RefreshSession());
    }

    [Fact]
    public async Task RefreshSession_ValidToken_RotatesIt()
    {
        var userId = Guid.NewGuid();
        var oldToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = Sha256Base64("valid-token"),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        SetRequestRefreshToken("valid-token");
        _refreshTokens.GetByHashAsync(oldToken.TokenHash).Returns(oldToken);
        _users.FindUserByIdAsync(userId).Returns(new User { Id = userId, Username = "user" });

        var result = await _service.RefreshSession();

        Assert.True(result);
        // old token revoked, single-use
        await _refreshTokens.Received(1).UpdateAsync(Arg.Is<RefreshToken>(t =>
            t.TokenHash == oldToken.TokenHash && t.RevokedAt != null));
        // a fresh token stored for the same user, under a new hash
        await _refreshTokens.Received(1).InsertAsync(Arg.Is<RefreshToken>(t =>
            t.UserId == userId && t.TokenHash != oldToken.TokenHash));
        // and its raw value went out as a cookie
        var newCookieToken = RefreshTokenCookieValue();
        Assert.NotNull(newCookieToken);
        Assert.NotEqual("valid-token", newCookieToken);
    }
}
