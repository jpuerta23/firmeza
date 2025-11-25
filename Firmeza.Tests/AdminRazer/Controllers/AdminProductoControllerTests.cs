using AdminRazer.Controllers;
using AdminRazer.Data;
using AdminRazer.Models;
using AdminRazer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Firmeza.Tests.AdminRazer.Controllers;

public class AdminProductoControllerTests
{
    [Fact]
    public void Index_ReturnsViewResult_WithListOfProductos()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_Admin_Productos_Index")
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            context.Productos.Add(new Producto { Id = 1, Nombre = "AdminProd1", Precio = 100, Categoria = "Test" });
            context.SaveChanges();
        }

        var mockExcelService = new Mock<IExcelImportService>();

        using (var context = new ApplicationDbContext(options))
        {
            var controller = new ProductoController(context, mockExcelService.Object);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Producto>>(viewResult.Model);
            Assert.Single(model);
        }
    }
}
