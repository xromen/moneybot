using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Infrasctructure.Entities;

namespace MoneyBotTelegram.Infrasctructure
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<MoneyTransaction> MoneyTransactions => Set<MoneyTransaction>();
        public DbSet<Place> Places => Set<Place>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Family> Families => Set<Family>();
        public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
        public DbSet<Category> Categories => Set<Category>();
    }
}
