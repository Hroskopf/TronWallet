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
}