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

}