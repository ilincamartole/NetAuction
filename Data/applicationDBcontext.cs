using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApplication1.Data
{
    // Moștenim DbContext, care e clasa de baza de la Microsoft pentru Identity
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                    : base(options)
        {
        }

        // Aici definim cum se vor numi tabelele în baza de date
        public DbSet<Licitatie> Licitatii { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 👑 CRITIC: Asta trebuie să fie INTOTDEAUNA prima linie din OnModelCreating!
            base.OnModelCreating(builder);

            // Configurări precizie zecimale pentru prețuri
            builder.Entity<Licitatie>().Property(l => l.PretCurent).HasColumnType("decimal(18,2)");
            builder.Entity<Licitatie>().Property(l => l.PretPornire).HasColumnType("decimal(18,2)");

            // Relația 1: Îi spunem clar că un Review are un Vanzator (Seller), iar Vanzatorul are lista ReviewsPrimite
            builder.Entity<Review>()
                .HasOne(r => r.Seller)
                .WithMany(u => u.ReviewsPrimite)
                .HasForeignKey(r => r.SellerId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict ca să prevenim ștergerile în cascadă accidentale

            // Relația 2: Configurăm și relația cu Cumpărătorul (Buyer)
            builder.Entity<Review>()
                .HasOne(r => r.Buyer)
                .WithMany() // Lăsăm gol pentru că nu ai o listă de ReviewsTrimise în ApplicationUser
                .HasForeignKey(r => r.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}