﻿namespace MoneyBotTelegram.Services.Models.ProverkaChekov;

public class Item
{
    public int Nds { get; set; }
    public int Sum { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }
    public int PaymentType { get; set; }
    public int ProductType { get; set; }
    public ProductCodeNew ProductCodeNew { get; set; }
    public int ItemsQuantityMeasure { get; set; }
}
