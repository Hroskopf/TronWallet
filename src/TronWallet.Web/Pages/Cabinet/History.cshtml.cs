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
    public Wallet Wallet { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }

    public HistoryModel(ITransactionService transactionService, IWalletService walletService)
    {
        _transactionService = transactionService;
        _walletService = walletService;
    }

    // =========================
    // SAFE PAGINATION HELPER
    // =========================
    private (int page, int pageSize, int offset) NormalizePagination(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var offset = (page - 1) * pageSize;

        return (page, pageSize, offset);
    }

    // =========================
    // FULL PAGE LOAD
    // =========================
    public async Task OnGetAsync()
    {
        if (!int.TryParse(Request.Query["page"], out var parsedPage))
        {
            parsedPage = 1;
        }

        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new Exception("Invalid user identity");
        }

        Wallet = await _walletService.GetWalletByUserIdAsync(userId);

        var totalCount = await _transactionService.GetWalletTransactionsCountAsync(Wallet.Id);

        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        if (TotalPages <= 0)
            TotalPages = 1;

        var (safePage, safePageSize, offset) = NormalizePagination(parsedPage, PageSize);

        if (safePage > TotalPages)
            safePage = TotalPages;

        Page = safePage;
        PageSize = safePageSize;

        Transactions = await _transactionService.GetAccountsTransactionsAsync(
            Wallet.TronAddress,
            PageSize,
            offset
        );
    }

    // =========================
    // AJAX PAGINATION
    // =========================
    public async Task<JsonResult> OnGetDataAsync(int page = 1)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new Exception("Invalid user identity");
        }

        var wallet = await _walletService.GetWalletByUserIdAsync(userId);

        var totalCount = await _transactionService.GetWalletTransactionsCountAsync(wallet.Id);

        var pageSize = 10;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages <= 0)
            totalPages = 1;

        var (safePage, safePageSize, offset) = NormalizePagination(page, pageSize);

        if (safePage > totalPages)
            safePage = totalPages;

        var txs = await _transactionService.GetAccountsTransactionsAsync(
            wallet.TronAddress,
            safePageSize,
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