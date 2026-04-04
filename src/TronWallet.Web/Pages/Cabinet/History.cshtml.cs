using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using TronWallet.Core.Domain.Entities;
using TronWallet.Core.Interfaces.Repositories;
using System.Security.Claims;
using TronWallet.Core.Interfaces.Services;

namespace TronWallet.Web.Pages.Cabinet;

[Authorize]
public class HistoryModel : PageModel
{
    private readonly ITransactionService _transactionService;
    private readonly IWalletService _walletService;
    public List<WalletTransaction> Transactions { get; set; } = new();

    public HistoryModel(ITransactionService transactionService, IWalletService walletService)
    {
        _transactionService = transactionService;
        _walletService = walletService;
    }
    
    public async Task OnGetAsync()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var wallet = await _walletService.GetWalletByUserIdAsync(userId);

        Transactions = await _transactionService.GetWalletsTransactionsAsync(wallet.Id);
    }
}

