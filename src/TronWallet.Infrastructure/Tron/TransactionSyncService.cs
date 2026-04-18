using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Interfaces.Repositories;
using Google.Rpc;
using TronWallet.Core.Domain.Entities;

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
        var walletRepo  = scope.ServiceProvider.GetRequiredService<IWalletRepository>();
        var txRepo      = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var tronClient  = scope.ServiceProvider.GetRequiredService<ITronGridClient>();

        var wallets = await walletRepo.GetAllAsync();
        foreach (var wallet in wallets)
        {
            try
            {
                var txList = await tronClient.GetTransactionsAsync(wallet.TronAddress, limit: 20);

                foreach (var tx in txList.Data)
                {
                    var toAddressBase58 = TronAddressService.HexToBase58(tx.ToAddressHex);

                    if (toAddressBase58 != wallet.TronAddress) continue;
                    if (await txRepo.ExistsTxByHashAsync(tx.TxID)) continue;

                    await txRepo.InsertAsync(new WalletTransaction
                    {
                        WalletId    = wallet.Id,
                        TxHash      = tx.TxID,
                        Direction   = "IN",
                        FromAddress = TronAddressService.HexToBase58(tx.FromAddressHex),
                        ToAddress   = wallet.TronAddress,
                        AmountSun   = (long)tx.RawData.Contract[0].Parameter.Value.Amount,
                        Status      = "CONFIRMED",
                        BlockTime   = DateTimeOffset
                                        .FromUnixTimeMilliseconds(tx.BlockTimeStamp)
                                        .UtcDateTime,
                        BlockNumber = tx.BlockNumber,
                        RawData = "{}",
                    });
                }
            }
            catch {}
        }
    }
    private async Task SyncPendingStatuses()
    {
        using var scope    = _scopeFactory.CreateScope();
        var txRepo         = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var tronClient     = scope.ServiceProvider.GetRequiredService<ITronGridClient>();

        var pending = await txRepo.GetPendingAsync();
        foreach (var tx in pending)
        {
            try {
                if (string.IsNullOrWhiteSpace(tx.TxHash)) continue;
                var info = await tronClient.GetTransactionInfoAsync(tx.TxHash!);
                string status;
                if (info?.BlockNumber is null or 0) // still pending
                {
                    continue;
                }
                else if (info.Result == "FAILED")
                {
                    status = "FAILED";
                }
                else if (info.Receipt?.Result == "REVERT" || info.Receipt?.Result == "FAILED")
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
                    DateTimeOffset.FromUnixTimeMilliseconds(info.BlockTimeStamp).UtcDateTime
                );
            }
            catch(Exception ex)
            {
                // _logger.LogError(ex, $"Failed to sync pending txs for tx {tx.TxID}", wallet.TronAddress);
                
            }
        }
    }
}