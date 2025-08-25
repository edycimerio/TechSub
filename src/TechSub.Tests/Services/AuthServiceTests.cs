using Xunit;
using Moq;
using TechSub.Aplicacao.Services;
using TechSub.Aplicacao.Interfaces;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Dominio.Entidades;
using TechSub.Aplicacao.Requests;
using Microsoft.Extensions.Configuration;

namespace TechSub.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _configurationMock = new Mock<IConfiguration>();
        
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(x => x["SecretKey"]).Returns("test-secret-key-for-jwt-token-generation-minimum-32-chars");
        jwtSectionMock.Setup(x => x["Issuer"]).Returns("TechSub");
        jwtSectionMock.Setup(x => x["Audience"]).Returns("TechSubUsers");
        
        _configurationMock.Setup(x => x.GetSection("Authentication:JwtSettings"))
            .Returns(jwtSectionMock.Object);
        
        _service = new AuthService(_configurationMock.Object, _usuarioRepositoryMock.Object);
    }

    [Fact]
    public async Task RegistrarUsuarioAsyncDeveRetornarToken()
    {
        // Arrange
        _usuarioRepositoryMock.Setup(x => x.ObterPorEmailAsync("joao@teste.com"))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _service.RegistrarUsuarioAsync("João", "joao@teste.com", "123456");

        // Assert
        Assert.NotNull(resultado.AccessToken);
        Assert.Equal("João", resultado.Usuario.Nome);
        _usuarioRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsyncDeveRetornarToken()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "joao@teste.com",
            Nome = "João",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456")
        };

        _usuarioRepositoryMock.Setup(x => x.ObterPorEmailAsync("joao@teste.com"))
            .ReturnsAsync(usuario);

        // Act
        var resultado = await _service.LoginAsync("joao@teste.com", "123456");

        // Assert
        Assert.NotNull(resultado.AccessToken);
        Assert.Equal("João", resultado.Usuario.Nome);
    }
}
