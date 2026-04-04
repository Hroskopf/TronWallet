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
    private readonly ITronGridClient _tronGridClient;
    public string Username { get; set; }
    public Wallet Wallet { get; set; }
    public decimal? Balance { get; set; } = 0;

    public DashboardModel(IWalletService walletService, ITronGridClient tronGridClient)
    {
        _walletService = walletService;
        _tronGridClient = tronGridClient;
    }
    public async Task OnGetAsync()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Username = User.FindFirst(ClaimTypes.Name)?.Value;

        Wallet = await _walletService.GetWalletByUserIdAsync(userId);
        
        var response =  await _tronGridClient.GetAccountAsync(Wallet.TronAddress);
        if(response == null || response.Account == null)
        {
            Balance = 0;
        }
        else
        {
            Balance = response.Account.GetBalanceInTRX();
        }
    } 
}

