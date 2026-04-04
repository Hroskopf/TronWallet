using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Services;

public interface IWalletService
{
    Task<Wallet> GetWalletByUserIdAsync(Guid id);
}