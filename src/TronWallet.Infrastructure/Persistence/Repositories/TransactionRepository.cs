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

        var sql = @"
            INSERT INTO transactions 
            (wallet_id, tx_hash, direction, from_address, to_address, amount_sun, block_number, block_time, raw_data, status)
            VALUES 
            (@WalletId, @TxHash, @Direction, @FromAddress, @ToAddress, @AmountSun, @BlockNumber, @BlockTime, @RawData::jsonb, @Status)
            RETURNING id;
            ";
        using var conn = _factory.CreateConnection();
        var result = await conn.QuerySingleAsync<Guid>(sql, tx);
        return result;
        
    }

    public async Task<List<WalletTransaction>> GetAccountsTransactionsAsync(
        string address,
        int limit = 50,
        int offset = 0)
    { 
        var sql = @"
            SELECT *
            FROM transactions
            WHERE from_address = @Address
            OR to_address = @Address
            ORDER BY created_at DESC
            LIMIT @Limit OFFSET @Offset;
        ";

        using var conn = _factory.CreateConnection();
        var transactions = await conn.QueryAsync<WalletTransaction>(
            sql,
            new 
            { 
                Address = address,
                Limit = limit,
                Offset = offset
            }
        );

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
                block_time = @BlockTime,
                confirmed_at = @BlockTime
            WHERE id = @Id";

        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status,
            BlockNumber = blockNumber,
            BlockTime = blockTime,
        });
    }
    public async Task<bool> ExistsTxByHashAsync(string txHash)
    {
        
        var sql = "SELECT EXISTS (SELECT 1 FROM transactions WHERE tx_hash = @txHash)";
        using var conn = _factory.CreateConnection();
        var result = await conn.ExecuteScalarAsync<bool>(sql, new { TxHash = txHash });
        return result;
    }

    public async Task<int> GetWalletTransactionsCountAsync(Guid walletId)
{
    var sql = @"
        SELECT COUNT(*)
        FROM transactions
        WHERE wallet_id = @WalletId;
    ";

    using var conn = _factory.CreateConnection();

    return await conn.ExecuteScalarAsync<int>(
        sql,
        new { WalletId = walletId }
    );
}

}