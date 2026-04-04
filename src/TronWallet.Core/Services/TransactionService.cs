// using TronWallet.Core.Interfaces.Services;
// using TronWallet.Core.Interfaces.Repositories;
// using TronWallet.Core.Domain.Entities;

// namespace TronWallet.Core.Services;

// public class TransactionService : ITransactionService
// {
//     private readonly IWalletRepository _walletRepository;
//     private readonly ITransactionRepository _transactionRepository;
//     private readonly ITronAddressService _tronService; // for sending on-chain

//     public TransactionService(
//         IWalletRepository walletRepository,
//         ITransactionRepository transactionRepository,
//         ITronAddressService tronService)
//     {
//         _walletRepository = walletRepository;
//         _transactionRepository = transactionRepository;
//         _tronService = tronService;
//     }

//     public async Task<List<WalletTransaction>> GetWalletTransactionsAsync(Guid walletId)
//     {
//         return await _transactionRepository.GetByWalletIdAsync(walletId);
//     }

//     public async Task<WalletTransaction> SendTransactionAsync(Guid fromUserId, string toAddress, decimal amount)
//     {
//         var wallet = await _walletRepository.GetWalletByUserIdAsync(fromUserId);

//         if (wallet == null)
//             throw new Exception("Wallet not found");

//         // Use Tron service to create & broadcast transaction
//         var txHash = await _tronService.SendAsync(wallet.PrivateKeyEnc, toAddress, amount);

//         // Save transaction in DB
//         var tx = new WalletTransaction
//         {
//             Id = Guid.NewGuid(),
//             WalletId = wallet.Id,
//             ToAddress = toAddress,
//             Amount = amount,
//             TxHash = txHash,
//             CreatedAt = DateTime.UtcNow
//         };

//         await _transactionRepository.InsertAsync(tx);

//         return tx;
//     }
// }