using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Interfaces.Repositories;
using Google.Rpc;

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
            await SyncAsync();
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    private async Task SyncAsync()
    {
        using var scope    = _scopeFactory.CreateScope();
        var txRepo         = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var tronClient     = scope.ServiceProvider.GetRequiredService<ITronGridClient>();

        var pending = await txRepo.GetPendingAsync();
        Console.WriteLine(pending.Count);
        foreach (var tx in pending)
        {
            if (string.IsNullOrWhiteSpace(tx.TxHash)) continue;
            Console.WriteLine(tx.Id);
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
    }
}