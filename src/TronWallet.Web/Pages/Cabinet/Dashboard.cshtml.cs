using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using TronWallet.Infrastructure.Persistence.Repositories;
using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Infrastructure.Tron;
using TronWallet.Core.Domain.Entities;
using TronWallet.Core.Interfaces.Services;

namespace TronWallet.Web.Pages.Cabinet;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly IWalletService _walletService;
    private readonly ITransactionService _txService;
    public string Username { get; set; }
    public Wallet Wallet { get; set; }
    public decimal? Balance { get; set; } = 0;
    public IEnumerable<WalletTransaction> Recent { get; private set; } = [];


    public DashboardModel(IWalletService walletService, ITransactionService txService)
    {
        _walletService = walletService;
        _txService = txService;
    }
    public async Task OnGetAsync()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Username = User.FindFirst(ClaimTypes.Name)?.Value;

        Wallet = await _walletService.GetWalletByUserIdAsync(userId);
        Balance = await _walletService.GetBalanceTrxAsync(Wallet.TronAddress);
        Recent = await _txService.GetAccountsTransactionsAsync(Wallet.TronAddress, limit: 10, offset: 0);
    } 
}

