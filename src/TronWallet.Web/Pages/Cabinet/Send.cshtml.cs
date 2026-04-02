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
    private readonly ITronGridClient _tronGridClient;
    private readonly IWalletRepository _walletRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ITronAdressService _tronAdressService;
    private readonly TronTransactionSigner _tronTransactionSigner;
    private readonly ITransactionRepository _transactionRepository;

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

    public SendModel(ITronGridClient tronGridClient, IWalletRepository walletRepository, IEncryptionService encryptionService, ITronAdressService tronAdressService, TronTransactionSigner tronTransactionSigner, ITransactionRepository transactionRepository)
    {
        _tronGridClient = tronGridClient;
        _walletRepository = walletRepository;
        _encryptionService = encryptionService;
        _tronAdressService = tronAdressService;
        _tronTransactionSigner = tronTransactionSigner;
        _transactionRepository = transactionRepository;
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

            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);

            if (wallet == null)
            {
                Error = "Wallet not found";
                return Page();
            }
            var balanceTRX = (await _tronGridClient.GetAccountAsync(wallet.TronAddress))?.Account?.GetBalanceInTRX() ?? 0m;            

            if(balanceTRX < AmountTRX)
            {
                Error = "Not enough TRX on the balance";
                return Page();
            }

            var privateKeyHex = _encryptionService.Decrypt(wallet.PrivateKeyEnc);

            var toAddressHex = _tronAdressService.Base58ToHex(ToAddress);
            var fromAddressHex = _tronAdressService.Base58ToHex(wallet.TronAddress);

            if(toAddressHex == fromAddressHex)
            {
                throw new Exception("You cannot send to yourself");
            }

            var unsignedTx = await _tronGridClient.CreateTransactionAsync(fromAddressHex, toAddressHex, (long)(AmountTRX * 1000000));

            var txSign = _tronTransactionSigner.Sign(unsignedTx.RawDataHex, privateKeyHex);
            privateKeyHex = null;
            var broadcastResponse = await _tronGridClient.BroadcastTransactionAsync(new
            {
                txID = unsignedTx.TxID,
                raw_data = unsignedTx.RawData,
                raw_data_hex = unsignedTx.RawDataHex,
                signature = new List<string> { txSign }
            });
            if(!broadcastResponse.Result)
            {
                throw new Exception("Oops something went wrong");
            }

            var txHash = unsignedTx.GetTxHash();
            
            await _transactionRepository.InsertAsync(new WalletTransaction
            {
                WalletId = wallet.Id,
                TxHash = txHash,
                FromAddress = wallet.TronAddress,
                ToAddress = ToAddress,
                AmountSun = (long)(AmountTRX * 1_000_000m),
                RawData = unsignedTx.RawDataStr
            });


            Message = $"Transaction sent! TX ID: {unsignedTx.TxID}";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            return Page();
        }

        return Redirect("/Cabinet/History");
    }
}

