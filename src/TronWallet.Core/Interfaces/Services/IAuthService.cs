namespace TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Domain.Entities;

public interface IAuthService
{
    Task<User> Register(string email, string username, string password);
    Task<User> Login(string email, string password);
}