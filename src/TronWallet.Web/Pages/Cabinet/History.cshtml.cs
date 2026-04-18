using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using TronWallet.Core.Domain.Entities;
using System.Security.Claims;
using TronWallet.Core.Interfaces.Services;

namespace TronWallet.Web.Pages.Cabinet;

[Authorize]
public class HistoryModel : PageModel
{
    private readonly ITransactionService _transactionService;
    private readonly IWalletService _walletService;

    public List<WalletTransaction> Transactions { get; set; } = new();

    public int Page { get; set; } = 1;    
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }

    public HistoryModel(ITransactionService transactionService, IWalletService walletService)
    {
        _transactionService = transactionService;
        _walletService = walletService;
    }

    public async Task OnGetAsync()
    {

        if (int.TryParse(Request.Query["page"], out var parsedPage))
        {
            Page = Math.Max(parsedPage, 1);
        }


        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var wallet = await _walletService.GetWalletByUserIdAsync(userId);

        var totalCount = await _transactionService.GetWalletTransactionsCountAsync(wallet.Id);

        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        if (TotalPages == 0)
            TotalPages = 1;

        if (Page > TotalPages)
            Page = TotalPages;

        int offset = (Page - 1) * PageSize;

        Transactions = await _transactionService.GetWalletsTransactionsAsync(
            wallet.Id,
            PageSize,
            offset
        );
    }

    public async Task<JsonResult> OnGetDataAsync(int page = 1)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var wallet = await _walletService.GetWalletByUserIdAsync(userId);

        var totalCount = await _transactionService.GetWalletTransactionsCountAsync(wallet.Id);

        var pageSize = 10;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;

        var offset = (page - 1) * pageSize;

        var txs = await _transactionService.GetWalletsTransactionsAsync(
            wallet.Id,
            pageSize,
            offset
        );

        return new JsonResult(new
        {
            transactions = txs.Select(tx => new
            {
                tx.TxHash,
                tx.FromAddress,
                tx.ToAddress,
                tx.Status,
                tx.CreatedAt,
                amount = tx.GetAmountTrx(),
                direction = tx.Direction
            })
        });
    }
}