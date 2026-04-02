using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using TronWallet.Core.Domain.Entities;
using TronWallet.Core.Interfaces.Repositories;
using System.Security.Claims;

namespace TronWallet.Web.Pages.Cabinet;

[Authorize]
public class HistoryModel : PageModel
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWalletRepository _walletRepository;
    public List<WalletTransaction> Transactions { get; set; } = new();

    public HistoryModel(ITransactionRepository transactionRepository, IWalletRepository walletRepository)
    {
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
    }
    
    public async Task OnGetAsync()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);

        Transactions = await _transactionRepository.GetWalletsTransactionsAsync(wallet.Id);
    }
}

