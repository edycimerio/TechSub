using Xunit;
using Moq;
using TechSub.Aplicacao.Services;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Dominio.Entidades;
using TechSub.Aplicacao.Requests;

namespace TechSub.Tests.Services;

public class PlanoServiceTests
{
    private readonly Mock<IPlanoRepository> _planoRepositoryMock;
    private readonly PlanoService _service;

    public PlanoServiceTests()
    {
        _planoRepositoryMock = new Mock<IPlanoRepository>();
        _service = new PlanoService(_planoRepositoryMock.Object);
    }

    [Fact]
    public async Task CriarAsyncDeveRetornarPlano()
    {
        // Arrange
        var request = new CriarPlanoRequest
        {
            Nome = "Plano Premium",
            PrecoMensal = 99.99m
        };

        _planoRepositoryMock.Setup(x => x.NomeExisteAsync(request.Nome))
            .ReturnsAsync(false);

        // Act
        var resultado = await _service.CriarAsync(request);

        // Assert
        Assert.Equal(request.Nome, resultado.Nome);
        Assert.Equal(request.PrecoMensal, resultado.PrecoMensal);
        _planoRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Plano>()), Times.Once);
    }

    [Fact]
    public async Task ObterTodosAsyncDeveRetornarPlanos()
    {
        // Arrange
        var planos = new List<Plano>
        {
            new Plano { Id = Guid.NewGuid(), Nome = "Free" },
            new Plano { Id = Guid.NewGuid(), Nome = "Basic" }
        };

        _planoRepositoryMock.Setup(x => x.ObterTodosAsync())
            .ReturnsAsync(planos);

        // Act
        var resultado = await _service.ObterTodosAsync();

        // Assert
        Assert.Equal(2, resultado.Count());
    }

    [Fact]
    public async Task ObterPorIdAsyncDeveRetornarPlano()
    {
        // Arrange
        var planoId = Guid.NewGuid();
        var plano = new Plano { Id = planoId, Nome = "Teste" };

        _planoRepositoryMock.Setup(x => x.ObterPorIdAsync(planoId))
            .ReturnsAsync(plano);

        // Act
        var resultado = await _service.ObterPorIdAsync(planoId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(plano.Nome, resultado.Nome);
    }

    [Fact]
    public async Task AtualizarAsyncDeveRetornarPlanoAtualizado()
    {
        // Arrange
        var planoId = Guid.NewGuid();
        var plano = new Plano { Id = planoId, Nome = "Original" };
        var request = new AtualizarPlanoRequest { Nome = "Atualizado" };

        _planoRepositoryMock.Setup(x => x.ObterPorIdAsync(planoId))
            .ReturnsAsync(plano);

        // Act
        var resultado = await _service.AtualizarAsync(planoId, request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(request.Nome, resultado.Nome);
        _planoRepositoryMock.Verify(x => x.AtualizarAsync(plano), Times.Once);
    }
}
