using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Web.Api.Services;
using Xunit;

namespace Firmeza.Tests.Services;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_ReturnsTokenString_WhenConfigIsValid()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "SuperSecretKey12345678901234567890"}, // Must be at least 16 chars usually, 32 bytes for HmacSha256
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpireMinutes", "60"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var jwtService = new JwtService(configuration);
        var roles = new List<string> { "Cliente" };

        // Act
        var token = jwtService.GenerateToken("user123", "test@example.com", roles);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Equal("TestAudience", jwtToken.Audiences.First());
        Assert.Contains(jwtToken.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Cliente");
    }

    [Fact]
    public void GenerateToken_ThrowsException_WhenKeyIsMissing()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Issuer", "TestIssuer"}
            // Missing Key
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var jwtService = new JwtService(configuration);

        // Act & Assert
        Assert.Throws<Exception>(() => jwtService.GenerateToken("user1", "email@test.com", new List<string>()));
    }
}
