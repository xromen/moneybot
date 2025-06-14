using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public long? FamilyId {  get; set; }
    [ForeignKey(nameof(FamilyId))]
    public Family? Family { get; set; }

    [InverseProperty(nameof(Family.Owner))]
    public virtual Family? OwnedFamily { get; set; }
}
