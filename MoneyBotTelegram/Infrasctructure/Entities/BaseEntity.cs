using System.ComponentModel.DataAnnotations;

namespace MoneyBotTelegram.Infrasctructure.Entities
{
    public class BaseEntity
    {
        [Key]
        public long Id { get; set; }
    }
}
