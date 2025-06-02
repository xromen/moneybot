using System.ComponentModel.DataAnnotations;

namespace MoneyBotTelegram.Infrasctructure.Entities;

public class User : BaseEntity
{

    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string FirstName { get; set; } = null!;

    [MaxLength(255)]
    public string? LastName { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public long? FamilyParentId { get; set; }
    public User? FamilyParent { get; set; }
}
