using TronWallet.Core.Domain.Entities;
using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Interfaces.Services;

namespace TronWallet.Core.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;

    public WalletService(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Wallet> GetWalletByUserIdAsync(Guid id)
    {
        var wallet = await _walletRepository.GetWalletByUserIdAsync(id);
        if (wallet == null)
        {
            throw new Exception("Wallet not found for the user.");
        }

        return wallet;
    }
}