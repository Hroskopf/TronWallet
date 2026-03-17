using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Repositories;

public interface IUserRepository
{
    bool ExistsByEmailAsync(string email);
    void InsertAsync(User user);
}