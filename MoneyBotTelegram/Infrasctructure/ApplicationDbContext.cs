using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Infrasctructure.Entities;

namespace MoneyBotTelegram.Infrasctructure
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Place> Places => Set<Place>();
        public DbSet<User> Users => Set<User>();
        public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
        public DbSet<Category> Categories => Set<Category>();
    }
}
