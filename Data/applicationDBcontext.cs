using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    // Moștenim DbContext, care e clasa de bază de la Microsoft
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Aici definim cum se vor numi tabelele în baza de date
        public DbSet<Licitatie> Licitatii { get; set; }
        public DbSet<Bid> Bids { get; set; }
    }
}