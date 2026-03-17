using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Domain.Entities;
using TronWallet.Core.Interfaces.Repositories;


namespace TronWallet.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly TronAdressService _tronAdressService;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public User Register(string email, string username, string password)
    {
         if (_userRepository.ExistsByEmailAsync(email))
            throw new Exception("User already exists");

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        var user = new User { Email = email, PasswordHash = hashedPassword, Username = username };

        _userRepository.InsertAsync(user);
        return user;
    }
    public User Login(string email, string password)
    {
        return new User();
    }
}