using AdminRazer.Controllers;
using AdminRazer.Models;
using AdminRazer.Repositories.Interfaces;
using AdminRazer.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Firmeza.Tests.AdminRazer.Controllers;

public class AdminProductoControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfProductos()
    {
        // Arrange
        var mockRepository = new Mock<IProductoRepository>();
        var mockExcelService = new Mock<IExcelImportService>();

        var productos = new List<Producto>
        {
            new Producto { Id = 1, Nombre = "AdminProd1", Precio = 100, Categoria = "Test" }
        };

        mockRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(productos);

        var controller = new ProductoController(mockRepository.Object, mockExcelService.Object);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Producto>>(viewResult.Model);
        Assert.Single(model);
    }
}
