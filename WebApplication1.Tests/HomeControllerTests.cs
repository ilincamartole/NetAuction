using Xunit;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Controllers;

namespace WebApplication1.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ArTrebuiSaReturnezeViewResult()
        {
            // Arrange
            var controller = new HomeController(null); // Înlocuiește null cu logger-ul dacă ai ILogger în constructor

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
}