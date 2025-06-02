using System.ComponentModel.DataAnnotations;

namespace MoneyBotTelegram.Infrasctructure.Entities
{
    public class Transaction : BaseEntity
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public long Categoryid { get; set; }
        public Category Category { get; set; } = null!;

        public long? PlaceId { get; set; }
        public Place? Place { get; set; }

        [Required]
        public long UserId {  get; set; }
        public User User { get; set; } = null!;

        public List<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    }
}
