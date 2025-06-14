using MoneyBotTelegram.Commands.Common;

namespace MoneyBotTelegram.Commands.Transaction.Models;

public class AddTransactionArgs : BaseArgs
{
    public AddTransactionState? State { get; set; }
    public long? CategoryId { get; set; }
    public long? PlaceId { get; set; }
    public int? Page { get; set; }
    public bool? Cancel { get; set; }
}
