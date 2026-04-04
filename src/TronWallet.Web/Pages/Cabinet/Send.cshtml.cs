using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Infrastructure.Tron;
using TronWallet.Core.Domain.Entities;
using System.Text.Json;
using System.Numerics;
using System.Transactions;


namespace TronWallet.Web.Pages.Cabinet;

[Authorize]
public class SendModel : PageModel
{
    private readonly ITransactionService _transactionService;

    [BindProperty]
    [Required]
    public string ToAddress { get; set; } = "";

    [BindProperty]
    [Required]
    [Range(0.000001, double.MaxValue, ErrorMessage = "Amount must be positive")]
    public decimal AmountTRX { get; set; } // in TRX

    public string? Message { get; set; }
    public string? Error { get; set; }
    public BigInteger TODO { get; private set; }

    public SendModel(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                Error = "User not authenticated";
                return Page();
            }
            await _transactionService.SendTransactionAsync(userId, ToAddress, AmountTRX);

        }
        catch (Exception ex)
        {
            Error = ex.Message;
            return Page();
        }

        return Redirect("/Cabinet/History");
    }
}

