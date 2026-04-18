using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Domain.Entities;
using System.Text.Json;

namespace TronWallet.Infrastructure.Tron;

public sealed class TransactionSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TransactionSyncService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncPendingStatuses();
            await SyncIncomingTransactionsAsync();

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    private async Task SyncIncomingTransactionsAsync()
    {
        using var scope = _scopeFactory.CreateScope();

        var walletRepo = scope.ServiceProvider.GetRequiredService<IWalletRepository>();
        var txRepo     = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var tronClient = scope.ServiceProvider.GetRequiredService<ITronGridClient>();

        var wallets = await walletRepo.GetAllAsync();

        foreach (var wallet in wallets)
        {
            try
            {
                var txList = await tronClient.GetTransactionsAsync(wallet.TronAddress, limit: 20);

                if (txList?.Data == null) continue;

                foreach (var tx in txList.Data)
                {
                    if (tx == null) continue;

                    var toAddressBase58 = TronAddressService.HexToBase58(tx.ToAddressHex);

                    // Only incoming transactions
                    if (toAddressBase58 != wallet.TronAddress) continue;

                    // Skip duplicates
                    if (await txRepo.ExistsTxByHashAsync(tx.TxID)) continue;

                    // Safe amount extraction
                    long amount = 0;
                    try
                    {
                        amount = (long)(tx.RawData?.Contract?[0]?.Parameter?.Value?.Amount ?? 0);
                    }
                    catch { }

                    // Serialize FULL transaction (recommended)
                    string rawJson = "{}";
                    try
                    {
                        rawJson = JsonSerializer.Serialize(tx);
                    }
                    catch { }

                    await txRepo.InsertAsync(new WalletTransaction
                    {
                        WalletId    = wallet.Id,
                        TxHash      = tx.TxID,
                        Direction   = "IN", // optional, you can remove later
                        FromAddress = TronAddressService.HexToBase58(tx.FromAddressHex),
                        ToAddress   = wallet.TronAddress,
                        AmountSun   = amount,
                        Status      = "CONFIRMED",
                        BlockTime   = DateTimeOffset
                                        .FromUnixTimeMilliseconds(tx.BlockTimeStamp)
                                        .UtcDateTime,
                        BlockNumber = tx.BlockNumber,
                        RawData     = rawJson
                    });
                }
            }
            catch
            {
                // optionally log
            }
        }
    }

    private async Task SyncPendingStatuses()
    {
        using var scope = _scopeFactory.CreateScope();

        var txRepo     = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var tronClient = scope.ServiceProvider.GetRequiredService<ITronGridClient>();

        var pending = await txRepo.GetPendingAsync();

        foreach (var tx in pending)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tx.TxHash)) continue;

                var info = await tronClient.GetTransactionInfoAsync(tx.TxHash);

                if (info == null) continue;


                string status;

                if (info.Result == "FAILED" ||
                    info.Receipt?.Result == "FAILED" ||
                    info.Receipt?.Result == "REVERT")
                {
                    status = "FAILED";
                }
                else
                {
                    status = "CONFIRMED";
                }

                await txRepo.UpdateStatusAsync(
                    tx.Id,
                    status,
                    info.BlockNumber,
                    DateTimeOffset
                        .FromUnixTimeMilliseconds(info.BlockTimeStamp)
                        .UtcDateTime
                );
            }
            catch
            {
                // optionally log
            }
        }
    }
}