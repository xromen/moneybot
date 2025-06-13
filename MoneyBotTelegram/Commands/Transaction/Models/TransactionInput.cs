namespace MoneyBotTelegram.Commands.Transaction.Models;

public class TransactionInput
{
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public long? CategoryId { get; set; }
    public long? PlaceId { get; set; }
}
