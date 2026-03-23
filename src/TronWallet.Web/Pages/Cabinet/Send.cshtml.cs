using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Interfaces.Repositories;


namespace TronWallet.Web.Pages.Cabinet;

[Authorize]

public class SendModel : PageModel
{
    private readonly ITronGridClient _tronClient;
    private readonly IWalletRepository _walletRepository;

    [BindProperty]
    [Required]
    public string ToAddress { get; set; } = "";

    [BindProperty]
    [Required]
    [Range(0.000001, double.MaxValue, ErrorMessage = "Amount must be positive")]
    public decimal Amount { get; set; }

    public string? Message { get; set; }
    public string? Error { get; set; }

    public SendModel(ITronGridClient tronClient, IWalletRepository walletRepository)
    {
        _tronClient = tronClient;
        _walletRepository = walletRepository;
    }
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        

        return Page();
    }
}

