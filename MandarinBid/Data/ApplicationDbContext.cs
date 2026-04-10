using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MandarinBid.Models;

namespace MandarinBid.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // создаем таблицы
        public DbSet<Mandarin> Mandarins { get; set; }
        public DbSet<Bid> Bids { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // задаем точность для денег
            builder.Entity<Mandarin>()
                .Property(m => m.CurrentPrice)
                .HasPrecision(18, 2);

            builder.Entity<Bid>()
                .Property(b => b.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Mandarin>()
                .Property(m => m.ExpirationDate)
                .HasColumnType("datetimeoffset");

            builder.Entity<Mandarin>()
                .Property(m => m.CreatedAt)
                .HasColumnType("datetimeoffset"); 

        }

    }
}
