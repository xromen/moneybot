using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyBotTelegram.Infrasctructure.Entities;

public class Family : BaseEntity
{
    [Required]
    public long OwnerId { get; set; }
    [ForeignKey(nameof(OwnerId))]
    public User Owner { get; set; } = null!;

    [InverseProperty(nameof(User.Family))]
    public virtual ICollection<User> Members { get; set; } = new List<User>();
}
