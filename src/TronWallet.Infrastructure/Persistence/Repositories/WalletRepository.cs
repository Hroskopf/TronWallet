using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Domain.Entities;
using Dapper;

namespace TronWallet.Infrastructure.Persistence.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly DbConnectionFactory _factory;

    public WalletRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<Guid> InsertAsync(Wallet wallet)
    {
        using var conn = _factory.CreateConnection();

        var sql = @"
            INSERT INTO wallets (user_id, tron_address, private_key_enc, public_key)
            VALUES (@UserId, @TronAddress, @PrivateKeyEnc, @PublicKey)
            RETURNING id;
        ";

        return await conn.QuerySingleAsync<Guid>(sql, wallet);

    }

    public async Task<Wallet?> GetWalletByIdAsync(Guid walletId)
    {
        using var conn = _factory.CreateConnection(); 
        const string sql = "SELECT * FROM wallets WHERE id = @WalletId LIMIT 1";

        var wallet = await conn.QueryFirstOrDefaultAsync<Wallet>(sql, new { WalletId = walletId });

        return wallet;
    }

    public async Task<Wallet?> GetWalletByUserIdAsync(Guid userId)
    {
        using var conn = _factory.CreateConnection(); 
        const string sql = "SELECT * FROM wallets WHERE user_id = @UserId LIMIT 1";

        var wallet = await conn.QueryFirstOrDefaultAsync<Wallet>(sql, new { UserId = userId });

        return wallet;
    }

    public async Task<List<Wallet>> GetAllAsync()
    {
        using var conn = _factory.CreateConnection(); 
        var sql = "SELECT * FROM wallets";
        var wallets = await conn.QueryAsync<Wallet>(sql);
        return wallets.ToList();
    }
}