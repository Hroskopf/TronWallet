namespace TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Domain.Entities;

public interface IAuthService
{
    User Register(string email, string password);
    User Login(string email, string password);
}