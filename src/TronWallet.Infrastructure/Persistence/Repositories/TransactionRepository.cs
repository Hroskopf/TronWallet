using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Domain.Entities;
using TronWallet.Infrastructure.Persistence;
using Dapper;
using System.Text.Json;

namespace TronWallet.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{ 
    private readonly DbConnectionFactory _factory;

    public TransactionRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<Guid> InsertAsync(WalletTransaction tx)
    {
        using var conn = _factory.CreateConnection();

        var sql = @"
            INSERT INTO transactions 
            (wallet_id, tx_hash, direction, from_address, to_address, amount_sun, block_number, block_time, raw_data)
            VALUES 
            (@WalletId, @TxHash, @Direction, @FromAddress, @ToAddress, @AmountSun, @BlockNumber, @BlockTime, @RawData::jsonb)
            RETURNING id;
            ";
        return await conn.QuerySingleAsync<Guid>(sql, new
        {
            WalletId = tx.WalletId,
            TxHash = tx.TxHash, 
            Direction = tx.Direction,
            FromAddress = tx.FromAddress,
            ToAddress = tx.ToAddress,
            AmountSun = tx.AmountSun,
            BlockNumber = tx.BlockNumber,
            BlockTime = tx.BlockTime, 
            RawData = tx.RawData
        });
        
    }

    public async Task<List<WalletTransaction>> GetWalletsTransactionsAsync(Guid walletId)
    {
        var sql = @"
            SELECT * FROM transactions
            WHERE wallet_id = @WalletId
            ORDER BY block_time DESC
            LIMIT 100;
        ";

        using var conn = _factory.CreateConnection(); // your IDbConnection
        var transactions = await conn.QueryAsync<WalletTransaction>(sql, new { WalletId = walletId });

        return transactions.ToList();
    }

    public async Task<List<WalletTransaction>> GetPendingAsync()
    {
        var sql = "SELECT * FROM transactions WHERE status = @Status";

        using var conn = _factory.CreateConnection();

        var pendingTransactions = await conn.QueryAsync<WalletTransaction>(
            sql,
            new { Status = "PENDING" }
        );
        return pendingTransactions.AsList();
    }

    public async Task UpdateStatusAsync(Guid id, string status, long blockNumber, DateTime blockTime)
    {
        const string sql = @"
            UPDATE transactions
            SET status = @Status,
                block_number = @BlockNumber,
                block_time = @BlockTime
            WHERE id = @Id";

        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status,
            BlockNumber = blockNumber,
            BlockTime = blockTime
        });
    }
}