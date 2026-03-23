using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Repositories;

public interface IWalletRepository
{
    Task<Guid> InsertAsync(Wallet wallet);
    Task<Wallet?> GetWalletByIdAsync(Guid walletId);
    Task<Wallet?> GetWalletByUserIdAsync(Guid userId);
}