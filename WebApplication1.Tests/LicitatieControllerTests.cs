using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using WebApplication1.Controllers;
using WebApplication1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Http; // 👑 IMPORTANT: Pentru IFormFile

namespace WebApplication1.Tests
{
    public class LicitatiiControllerTests
    {
        private readonly TestDatabaseFixture _fixture;

        public LicitatiiControllerTests()
        {
            _fixture = new TestDatabaseFixture();
        }

        [Fact]
        public async Task Create_ArTrebuiSaReturnezeViewSauEroare_CandModelulEsteInvalid()
        {
            // Arrange
            using var context = _fixture.CreateContext();

            var controller = new LicitatiiController(
                context,
                null, // userManager
                null, // hubContext (SignalR)
                null  // emailSender
            );

            // Setezi o eroare de model manual (simulăm că titlul lipsește)
            controller.ModelState.AddModelError("titlu", "Titlul este obligatoriu.");

            var licitatieInvalida = new Licitatie
            {
                PretPornire = -10, // Preț invalid
                descriere = "Test"
            };

            // Act 
            // 👑 CORECTURĂ: Adăugăm null ca al doilea parametru pentru fisierImagine
            var result = await controller.Create(licitatieInvalida, null);

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
}