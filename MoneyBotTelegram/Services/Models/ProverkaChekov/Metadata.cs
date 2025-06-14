namespace MoneyBotTelegram.Services.Models.ProverkaChekov;

public class Metadata
{
    public long Id { get; set; }
    public string OfdId { get; set; }
    public string Address { get; set; }
    public string Subtype { get; set; }
    public DateTime ReceiveDate { get; set; }
}
