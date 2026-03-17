using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email);
    Task InsertAsync(User user);
}