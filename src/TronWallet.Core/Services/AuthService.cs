using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Domain.Entities;
using TronWallet.Core.Interfaces.Repositories;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;



namespace TronWallet.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITronAdressService _tronAdressService;
    private readonly IEncryptionService _aesEncryptionService;
    private readonly IWalletRepository _walletRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IUserRepository userRepository, ITronAdressService tronAdressService, IEncryptionService aesEncryptionService, IWalletRepository walletRepository, IHttpContextAccessor httpContextAccessor)
    {

        _userRepository = userRepository;
        _tronAdressService = tronAdressService;
        _aesEncryptionService = aesEncryptionService;
        _walletRepository = walletRepository;
        _httpContextAccessor = httpContextAccessor;
    }

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

    public async Task<User> Register(string email, string username, string password)
    {
        if (await _userRepository.ExistsByEmailAsync(email))
            throw new Exception("Email already exists");

        if (await _userRepository.ExistsByUsernameAsync(username))
            throw new Exception("User already exists");



        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        var (privateKeyHex, publicKeyHex, base58Address) = _tronAdressService.GenerateWallet();

        var privateKeyEnc = _aesEncryptionService.Encrypt(privateKeyHex);

        var user = new User { Email = email, PasswordHash = hashedPassword, Username = username };

        var userId = await _userRepository.InsertAsync(user);

        var wallet = new Wallet
        {
            UserId = userId,
            TronAddress = base58Address,
            PrivateKeyEnc = privateKeyEnc,
            PublicKey = publicKeyHex,
        };

        await _walletRepository.InsertAsync(wallet);

        await SignInUserAsync(user);

        return user;
    }
    public async Task<User?> Login(string email, string password)
    {
        
        var user = await _userRepository.FindUserByEmailAsync(email);

        if(user == null)
        {
            return null;
        }

        bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isPasswordCorrect)
        {
            return null; 
        }

        await SignInUserAsync(user);

        return user;

    }

    public async Task Logout()
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

}