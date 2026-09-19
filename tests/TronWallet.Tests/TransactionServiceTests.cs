using NSubstitute;
using TronWallet.Core.Domain.Entities;
using TronWallet.Core.Domain.Entities.Tron;
using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Services;

namespace TronWallet.Tests;

public class TransactionServiceTests
{
    private readonly ITronGridClient _tronGrid = Substitute.For<ITronGridClient>();
    private readonly IWalletRepository _wallets = Substitute.For<IWalletRepository>();
    private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
    private readonly ITronAddressService _addresses = Substitute.For<ITronAddressService>();
    private readonly ITransactionSigner _signer = Substitute.For<ITransactionSigner>();
    private readonly ITransactionRepository _transactions = Substitute.For<ITransactionRepository>();
    private readonly TransactionService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Wallet _wallet;

    public TransactionServiceTests()
    {
        _service = new TransactionService(
            _tronGrid, _wallets, _encryption, _addresses, _signer, _transactions);

        _wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            TronAddress = "TSenderAddress",
            PrivateKeyEnc = "priv-enc",
            PublicKey = "pub-hex"
        };

        _addresses.Base58ToHex("TSenderAddress").Returns("41sender");
        _addresses.Base58ToHex("TReceiverAddress").Returns("41receiver");
        _encryption.Decrypt("priv-enc").Returns("priv-hex");
    }

    private void GivenBalanceSun(decimal balanceSun)
    {
        _tronGrid.GetAccountAsync("TSenderAddress").Returns(new TronAccountResponse
        {
            Account = new Account { Balance = balanceSun }
        });
    }

    [Fact]
    public async Task Send_WhenUserHasNoWallet_Throws()
    {
        _wallets.GetWalletByUserIdAsync(_userId).Returns((Wallet?)null);

        var ex = await Assert.ThrowsAsync<Exception>(
            () => _service.SendTransactionAsync(_userId, "TReceiverAddress", 1m));
        Assert.Contains("wallet", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Send_WhenBalanceInsufficient_Throws()
    {
        _wallets.GetWalletByUserIdAsync(_userId).Returns(_wallet);
        GivenBalanceSun(5_000_000m); // 5 TRX

        await Assert.ThrowsAsync<Exception>(
            () => _service.SendTransactionAsync(_userId, "TReceiverAddress", 10m));

        await _tronGrid.DidNotReceiveWithAnyArgs().CreateTransactionAsync(default!, default!, default);
    }

    [Fact]
    public async Task Send_ToOwnAddress_Throws()
    {
        _wallets.GetWalletByUserIdAsync(_userId).Returns(_wallet);
        GivenBalanceSun(100_000_000m);
        _addresses.Base58ToHex("TSenderAddress").Returns("41same");
        _addresses.Base58ToHex("TReceiverAddress").Returns("41same");

        await Assert.ThrowsAsync<Exception>(
            () => _service.SendTransactionAsync(_userId, "TReceiverAddress", 1m));
    }

    [Fact]
    public async Task Send_HappyPath_SignsBroadcastsAndRecords()
    {
        _wallets.GetWalletByUserIdAsync(_userId).Returns(_wallet);
        GivenBalanceSun(100_000_000m); // 100 TRX

        var unsigned = new TronUnsignedTx { TxID = "tx1", RawDataHex = "aabbcc", RawDataStr = "{}" };
        _tronGrid.CreateTransactionAsync("41sender", "41receiver", 2_500_000L).Returns(unsigned);
        _signer.Sign("aabbcc", "priv-hex").Returns("signature");
        _tronGrid.BroadcastTransactionAsync(Arg.Any<object>())
                 .Returns(new TronBroadcastResponse { Result = true });

        await _service.SendTransactionAsync(_userId, "TReceiverAddress", 2.5m);

        await _transactions.Received(1).InsertAsync(Arg.Is<WalletTransaction>(t =>
            t.WalletId == _wallet.Id &&
            t.FromAddress == "TSenderAddress" &&
            t.ToAddress == "TReceiverAddress" &&
            t.AmountSun == 2_500_000L &&
            t.TxHash == unsigned.GetTxHash()));
    }

    [Fact]
    public async Task Send_WhenBroadcastFails_Throws_AndRecordsNothing()
    {
        _wallets.GetWalletByUserIdAsync(_userId).Returns(_wallet);
        GivenBalanceSun(100_000_000m);
        _tronGrid.CreateTransactionAsync(default!, default!, default)
                 .ReturnsForAnyArgs(new TronUnsignedTx { TxID = "tx1", RawDataHex = "aabbcc" });
        _signer.Sign(default!, default!).ReturnsForAnyArgs("signature");
        _tronGrid.BroadcastTransactionAsync(Arg.Any<object>())
                 .Returns(new TronBroadcastResponse { Result = false });

        await Assert.ThrowsAsync<Exception>(
            () => _service.SendTransactionAsync(_userId, "TReceiverAddress", 1m));

        await _transactions.DidNotReceiveWithAnyArgs().InsertAsync(default!);
    }
}
