using AdminRazer.Models;
using AdminRazer.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Api.Controllers;
using Web.Api.DTOs;
using Xunit;

namespace Firmeza.Tests.Controllers;

public class ProductosControllerTests
{
    [Fact]
    public async Task GetProductos_ReturnsOkResult_WithListOfProductos()
    {
        // Arrange
        var mockRepository = new Mock<IProductoRepository>();
        var mockMapper = new Mock<IMapper>();

        var productos = new List<Producto>
        {
            new Producto { Id = 1, Nombre = "Prod1", Precio = 10, Categoria = "Test" },
            new Producto { Id = 2, Nombre = "Prod2", Precio = 20, Categoria = "Test" }
        };

        var productoDtos = new List<ProductoDto>
        {
            new ProductoDto { Id = 1, Nombre = "Prod1" },
            new ProductoDto { Id = 2, Nombre = "Prod2" }
        };

        mockRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(productos);

        mockMapper.Setup(m => m.Map<IEnumerable<ProductoDto>>(It.IsAny<List<Producto>>()))
            .Returns(productoDtos);

        var controller = new ProductosController(mockRepository.Object, mockMapper.Object);

        // Act
        var result = await controller.GetProductos();

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<ProductoDto>>(actionResult.Value);
        Assert.Equal(2, returnValue.Count());
    }
}
