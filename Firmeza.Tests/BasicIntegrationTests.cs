using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Firmeza.Tests;

public class BasicIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Swagger_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/index.html"); // Swagger está en la raíz

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
    }
}
