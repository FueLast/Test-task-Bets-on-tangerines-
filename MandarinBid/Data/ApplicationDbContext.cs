using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MandarinBid.Models;

namespace MandarinBid.Data
{
    // основной контекст базы данных (включает identity)
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // таблица лотов (мандаринов)
        public DbSet<Mandarin> Mandarins { get; set; }

        // таблица ставок
        public DbSet<Bid> Bids { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // задаём точность decimal для денег (важно для финансов)
            builder.Entity<Mandarin>()
                .Property(m => m.CurrentPrice)
                .HasPrecision(18, 2);

            builder.Entity<Bid>()
                .Property(b => b.Amount)
                .HasPrecision(18, 2);

            // явно указываем тип datetimeoffset (чтобы избежать проблем с таймзонами)
            builder.Entity<Mandarin>()
                .Property(m => m.ExpirationDate)
                .HasColumnType("datetimeoffset");

            builder.Entity<Mandarin>()
                .Property(m => m.CreatedAt)
                .HasColumnType("datetimeoffset");
        }
    }
}