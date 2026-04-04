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
public class ReceiveModel : PageModel
{
    private readonly IWalletService _walletService;
    public string WalletAddress { get; set; }

    public ReceiveModel(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task OnGetAsync()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var wallet = await _walletService.GetWalletByUserIdAsync(userId);
        WalletAddress = wallet.TronAddress;

    }
}

