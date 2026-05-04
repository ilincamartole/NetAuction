using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using WebApplication1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApplication1.Data
{
    // Moștenim DbContext, care e clasa de bază de la Microsoft
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                    : base(options)
        {
        }


        // Aici definim cum se vor numi tabelele în baza de date
        public DbSet<Licitatie> Licitatii { get; set; }
        public DbSet<Bid> Bids { get; set; }

        // Dacă ai creat modelul Review, adaugă-l și pe el:
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<Licitatie>().Property(l => l.PretCurent).HasColumnType("decimal(18,2)");
            builder.Entity<Licitatie>().Property(l => l.PretPornire).HasColumnType("decimal(18,2)");
            base.OnModelCreating(builder); // Nu șterge asta, e vitală pentru Identity!

            // Îi spunem clar: un Review are un Vanzator, iar Vanzatorul are lista ReviewsPrimite
            builder.Entity<Review>()
                .HasOne(r => r.Seller)
                .WithMany(u => u.ReviewsPrimite)
                .HasForeignKey(r => r.SellerId)
                .OnDelete(DeleteBehavior.Restrict); // Important: Restrict ca să nu se șteargă userul dacă ștergi un review

            // Configurăm și relația cu Cumpărătorul (chiar dacă el nu are o listă în ApplicationUser)
            builder.Entity<Review>()
                .HasOne(r => r.Buyer)
                .WithMany() // Lăsăm parantezele goale dacă nu ai pus o listă de ReviewsTrimise în User
                .HasForeignKey(r => r.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
    }
