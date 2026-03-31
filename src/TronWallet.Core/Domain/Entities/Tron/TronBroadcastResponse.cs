

namespace TronWallet.Core.Domain.Entities.Tron;

public class TronBroadcastResponse
{
    public bool Result { get; set; }
    public string TxId { get; set; }
    public string Code { get; set; }
    public string Message { get; set; }


}