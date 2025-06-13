using System.ComponentModel.DataAnnotations;

namespace MoneyBotTelegram.Infrasctructure.Entities
{
    public class MoneyTransaction : BaseEntity
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Required]
        public long CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public long? PlaceId { get; set; }
        public Place? Place { get; set; }

        [Required]
        public long UserId {  get; set; }
        public User User { get; set; } = null!;

        public string? Description { get; set; }

        public List<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    }
}
