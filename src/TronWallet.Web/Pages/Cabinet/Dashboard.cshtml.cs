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
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITronGridClient _tronGridClient;
    public User UserInfo { get; set; }
    public Wallet Wallet { get; set; }
    public decimal? Balance { get; set; } = 0;

    public DashboardModel(IUserRepository userRepository, IWalletRepository walletRepository, ITronGridClient tronGridClient)
    {
        _walletRepository = walletRepository;
        _userRepository = userRepository;
        _tronGridClient = tronGridClient;
    }
    public async Task OnGetAsync()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        UserInfo = await _userRepository.FindUserByIdAsync(userId);
        Wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
        
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

