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
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWalletService _walletService;
    public List<WalletTransaction> Transactions { get; set; } = new();

    public HistoryModel(ITransactionRepository transactionRepository, IWalletService walletService)
    {
        _transactionRepository = transactionRepository;
        _walletService = walletService;
    }
    
    public async Task OnGetAsync()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var wallet = await _walletService.GetWalletByUserIdAsync(userId);

        Transactions = await _transactionRepository.GetWalletsTransactionsAsync(wallet.Id);
    }
}

