namespace MoneyBotTelegram.Services.Models.ProverkaChekov;

public class ProductCodeNew
{
    public Undefined Undefined { get; set; }
}
public class Undefined
{
    public string Gtin { get; set; }
    public string Sernum { get; set; }
    public int ProductIdType { get; set; }
    public string RawProductCode { get; set; }
}
