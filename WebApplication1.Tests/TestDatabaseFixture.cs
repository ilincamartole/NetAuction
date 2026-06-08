using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System;

namespace WebApplication1.Tests
{
    public class TestDatabaseFixture
    {
        public ApplicationDbContext CreateContext()
        {
            // Generăm un nume unic pentru baza de date din memorie la fiecare test ca să nu se amestece datele
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            // Seed-uim (populăm) baza de date temporară cu doi utilizatori de test
            SeedData(context);

            return context;
        }

        private void SeedData(ApplicationDbContext context)
        {
            var seller = new ApplicationUser
            {
                Id = "seller_id_123",
                UserName = "ilinca@test.com",
                Email = "ilinca@test.com",
                nume = "Ilinca",
                prenume = "Test",
                balance = 100,
                adresa = "Strada Test nr. 1" // 👑 CORECTURĂ: Adăugat adresă obligatorie
            };

            var buyer = new ApplicationUser
            {
                Id = "buyer_id_456",
                UserName = "cumparator@test.com",
                Email = "cumparator@test.com",
                nume = "George",
                prenume = "Cumparator",
                balance = 500,
                adresa = "Strada Cumparaturilor nr. 5" // 👑 CORECTURĂ: Adăugat adresă obligatorie
            };

            context.Users.AddRange(seller, buyer);
            context.SaveChanges();
        }
    }
}