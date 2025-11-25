using AdminRazer.Data;
using AdminRazer.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_Productos_Get")
            .Options;

        // Seed database
        using (var context = new ApplicationDbContext(options))
        {
            context.Productos.Add(new Producto { Id = 1, Nombre = "Prod1", Precio = 10, Categoria = "Test" });
            context.Productos.Add(new Producto { Id = 2, Nombre = "Prod2", Precio = 20, Categoria = "Test" });
            await context.SaveChangesAsync();
        }

        // Mock Mapper
        var mockMapper = new Mock<IMapper>();
        mockMapper.Setup(m => m.Map<IEnumerable<ProductoDto>>(It.IsAny<List<Producto>>()))
            .Returns((List<Producto> src) => src.Select(p => new ProductoDto { Id = p.Id, Nombre = p.Nombre }).ToList());

        using (var context = new ApplicationDbContext(options))
        {
            var controller = new ProductosController(context, mockMapper.Object);

            // Act
            var result = await controller.GetProductos();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<ProductoDto>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }
    }
}
