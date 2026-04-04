using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Domain.Entities;
using TronWallet.Core.Interfaces.Repositories;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace TronWallet.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITronAddressService _tronAdressService;
    private readonly IEncryptionService _aesEncryptionService;
    private readonly IWalletRepository _walletRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IUserRepository userRepository,
        ITronAddressService tronAdressService,
        IEncryptionService aesEncryptionService,
        IWalletRepository walletRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _tronAdressService = tronAdressService;
        _aesEncryptionService = aesEncryptionService;
        _walletRepository = walletRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    // ===========================
    // Core cookie-based sign-in
    // ===========================
    private async Task SignInUserAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await _httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            });
    }

    // ===========================
    // Refresh token helpers
    // ===========================
    private string GenerateRefreshToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    private void SetRefreshTokenCookie(string token)
    {
        _httpContextAccessor.HttpContext!.Response.Cookies.Append(
            "refreshToken",
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
    }

    private string? GetRefreshTokenFromCookie()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];
    }

    // ===========================
    // Registration
    // ===========================
    public async Task<User> Register(string email, string username, string password)
    {
        if (await _userRepository.ExistsByEmailAsync(email))
            throw new Exception("Email already exists");

        if (await _userRepository.ExistsByUsernameAsync(username))
            throw new Exception("User already exists");

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        password = "";

        var (privateKeyHex, publicKeyHex, base58Address) = _tronAdressService.GenerateWallet();
        var privateKeyEnc = _aesEncryptionService.Encrypt(privateKeyHex);

        var user = new User { Email = email, PasswordHash = hashedPassword, Username = username };
        var userId = await _userRepository.InsertAsync(user);
        user.Id = userId;

        var wallet = new Wallet
        {
            UserId = userId,
            TronAddress = base58Address,
            PrivateKeyEnc = privateKeyEnc,
            PublicKey = publicKeyHex,
        };
        await _walletRepository.InsertAsync(wallet);

        await SignInUserAsync(user);

        // Generate refresh token
        var refreshToken = GenerateRefreshToken();
        var tokenHash = HashToken(refreshToken);

        await _refreshTokenRepository.InsertAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"]
        });

        SetRefreshTokenCookie(refreshToken);

        return user;
    }

    // ===========================
    // Login
    // ===========================
    public async Task<User?> Login(string email, string password)
    {
        var user = await _userRepository.FindUserByEmailAsync(email);
        if (user == null)
            return null;

        bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        password = "";

        if (!isPasswordCorrect)
            return null;

        await SignInUserAsync(user);

        // Generate refresh token
        var refreshToken = GenerateRefreshToken();
        var tokenHash = HashToken(refreshToken);

        await _refreshTokenRepository.InsertAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"]
        });

        SetRefreshTokenCookie(refreshToken);

        return user;
    }

    // ===========================
    // Refresh session (rotation)
    // ===========================
    public async Task<bool> RefreshSession()
    {
        var token = GetRefreshTokenFromCookie();
        if (token == null) return false;

        var hash = HashToken(token);
        var stored = await _refreshTokenRepository.GetByHashAsync(hash);

        if (stored == null || stored.RevokedAt != null || stored.ExpiresAt < DateTime.UtcNow)
            return false;

        // Revoke old token
        stored.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(stored);

        // Create new token
        var newToken = GenerateRefreshToken();
        var newHash = HashToken(newToken);

        await _refreshTokenRepository.InsertAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = stored.UserId,
            TokenHash = newHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"]
        });

        SetRefreshTokenCookie(newToken);

        var user = await _userRepository.FindUserByIdAsync(stored.UserId);
        if (user != null)
        {
            await SignInUserAsync(user);
            return true;
        }

        return false;
    }

    // ===========================
    // Logout
    // ===========================
    public async Task Logout()
    {
        var token = GetRefreshTokenFromCookie();
        if (token != null)
        {
            var hash = HashToken(token);
            await _refreshTokenRepository.RevokeAsync(hash, DateTime.UtcNow);
        }

        _httpContextAccessor.HttpContext!.Response.Cookies.Delete("refreshToken");
        await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}