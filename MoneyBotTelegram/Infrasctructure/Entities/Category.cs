using System.ComponentModel.DataAnnotations;

namespace MoneyBotTelegram.Infrasctructure.Entities;

public class Category : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    public long? ParentId { get; set; }
    public Category? Parent { get; set; }
}
