using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebApplication1.Controllers;
using System.Linq;

namespace WebApplication1.Tests
{
    public class ReviewsControllerTests
    {
        private readonly TestDatabaseFixture _fixture;

        public ReviewsControllerTests()
        {
            _fixture = new TestDatabaseFixture();
        }

        [Fact]
        public async Task PostReview_TrebuieSaReturnezeBadRequest_CandUserulIsiLasaReviewLuiInsusi()
        {
            // Arrange (Pregătim contextul de test)
            using var context = _fixture.CreateContext();
            var controller = new ReviewsController(context);

            // Simulăm că utilizatorul logat curent este "seller_id_123"
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "seller_id_123")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // DTO-ul trimis din React: Vânzătorul are același ID ca cel logat ("seller_id_123")
            var dto = new CreateReviewDto
            {
                Nota = 5,
                Comentariu = "Sunt cel mai bun vanzator!",
                SellerId = "seller_id_123"
            };

            // Act (Rulăm metoda din controller)
            var result = await controller.PostReview(dto);

            // Assert (Verificăm dacă regulile noastre din C# au funcționat)
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Nu îți poți lăsa review singur.", badRequestResult.Value);
        }

        [Fact]
        public async Task PostReview_TrebuieSaSalvezeInBazaDeDate_CandReviewUlEsteValid()
        {
            // Arrange (Pregătim contextul de test)
            using var context = _fixture.CreateContext();
            var controller = new ReviewsController(context);

            // Simulăm că utilizatorul logat curent este Cumpărătorul ("buyer_id_456")
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "buyer_id_456")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // DTO-ul trimis: Review legitim de la Cumpărător (456) către Vânzător (123)
            var dto = new CreateReviewDto
            {
                Nota = 4,
                Comentariu = "Tranzactie rapida, recomand!",
                SellerId = "seller_id_123"
            };

            // Act (Tritem review-ul)
            var result = await controller.PostReview(dto);

            // Assert (Verificăm rezultatul)
            Assert.IsType<OkObjectResult>(result);

            // Verificăm dacă review-ul s-a scris fizic în tabelul din memorie
            var reviewSalvat = context.Reviews.FirstOrDefault(r => r.SellerId == "seller_id_123");
            Assert.NotNull(reviewSalvat);
            Assert.Equal(4, reviewSalvat.Nota);
            Assert.Equal("Tranzactie rapida, recomand!", reviewSalvat.Comentariu);
            Assert.Equal("buyer_id_456", reviewSalvat.BuyerId);
        }
    }
}