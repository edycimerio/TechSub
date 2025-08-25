using Xunit;
using Moq;
using TechSub.Aplicacao.Services;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Dominio.Entidades;
using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Interfaces;

namespace TechSub.Tests.Services;

public class AssinaturaServiceTests
{
    private readonly Mock<IAssinaturaRepository> _assinaturaRepositoryMock;
    private readonly Mock<IPagamentoRepository> _pagamentoRepositoryMock;
    private readonly Mock<IPlanoRepository> _planoRepositoryMock;
    private readonly Mock<INotificacaoService> _notificacaoServiceMock;
    private readonly Mock<IUsuarioMetodoPagamentoRepository> _usuarioMetodoPagamentoRepositoryMock;
    private readonly AssinaturaService _service;

    public AssinaturaServiceTests()
    {
        _assinaturaRepositoryMock = new Mock<IAssinaturaRepository>();
        _pagamentoRepositoryMock = new Mock<IPagamentoRepository>();
        _planoRepositoryMock = new Mock<IPlanoRepository>();
        _notificacaoServiceMock = new Mock<INotificacaoService>();
        _usuarioMetodoPagamentoRepositoryMock = new Mock<IUsuarioMetodoPagamentoRepository>();

        _service = new AssinaturaService(
            _assinaturaRepositoryMock.Object,
            _pagamentoRepositoryMock.Object,
            _planoRepositoryMock.Object,
            _notificacaoServiceMock.Object,
            _usuarioMetodoPagamentoRepositoryMock.Object
        );
    }

    [Fact]
    public async Task CriarAsyncDeveRetornarAssinatura()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var request = new CriarAssinaturaRequest { PlanoId = Guid.NewGuid() };
        var plano = new Plano { Id = request.PlanoId, Nome = "Basic" };

        _planoRepositoryMock.Setup(x => x.ObterPorIdAsync(request.PlanoId))
            .ReturnsAsync(plano);
        _assinaturaRepositoryMock.Setup(x => x.ObterAtivaPorUsuarioAsync(usuarioId))
            .ReturnsAsync((Assinatura?)null);

        // Act
        var resultado = await _service.CriarAsync(request, usuarioId);

        // Assert
        Assert.Equal(plano.Nome, resultado.PlanoNome);
        _assinaturaRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Assinatura>()), Times.Once);
    }

    [Fact]
    public async Task CancelarAsyncDeveRetornarTrue()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var assinaturaId = Guid.NewGuid();
        var assinatura = new Assinatura { Id = assinaturaId, UsuarioId = usuarioId };

        _assinaturaRepositoryMock.Setup(x => x.ObterPorIdAsync(assinaturaId))
            .ReturnsAsync(assinatura);

        // Act
        var resultado = await _service.CancelarAsync(assinaturaId, usuarioId);

        // Assert
        Assert.True(resultado);
        _assinaturaRepositoryMock.Verify(x => x.AtualizarAsync(assinatura), Times.Once);
    }

    [Fact]
    public async Task ObterAtivaAsyncDeveRetornarAssinatura()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var assinatura = new Assinatura
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Plano = new Plano { Nome = "Basic" }
        };

        _assinaturaRepositoryMock.Setup(x => x.ObterPorUsuarioAsync(usuarioId))
            .ReturnsAsync(new List<Assinatura> { assinatura });

        // Act
        var resultado = await _service.ObterAtivaAsync(usuarioId, usuarioId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(assinatura.Plano.Nome, resultado.PlanoNome);
    }
}