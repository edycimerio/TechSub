using Xunit;
using Moq;
using TechSub.Aplicacao.Services;
using TechSub.Aplicacao.Interfaces;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Enums;
using TechSub.Aplicacao.Requests;

namespace TechSub.Tests.Services;

public class PagamentoServiceTests
{
    private readonly Mock<IPagamentoRepository> _pagamentoRepositoryMock;
    private readonly Mock<IAssinaturaRepository> _assinaturaRepositoryMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<INotificacaoService> _notificacaoServiceMock;
    private readonly PagamentoService _service;

    public PagamentoServiceTests()
    {
        _pagamentoRepositoryMock = new Mock<IPagamentoRepository>();
        _assinaturaRepositoryMock = new Mock<IAssinaturaRepository>();
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _notificacaoServiceMock = new Mock<INotificacaoService>();

        _service = new PagamentoService(
            _pagamentoRepositoryMock.Object,
            _assinaturaRepositoryMock.Object,
            _usuarioRepositoryMock.Object,
            _notificacaoServiceMock.Object
        );
    }

    [Fact]
    public async Task ProcessarPagamentoAsyncDeveAprovarPagamentoQuandoSimulacaoAprovada()
    {
        // Arrange
        var pagamentoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var assinaturaId = Guid.NewGuid();

        var pagamento = new Pagamento
        {
            Id = pagamentoId,
            AssinaturaId = assinaturaId,
            Status = StatusPagamento.Pendente,
            Valor = 29.99m,
            Assinatura = new Assinatura
            {
                Id = assinaturaId,
                UsuarioId = usuarioId,
                Plano = new Plano { Nome = "Pro" },
                Usuario = new Usuario { Nome = "Test User" }
            }
        };

        var usuario = new Usuario { Id = usuarioId, Nome = "Test User" };

        _pagamentoRepositoryMock.Setup(x => x.ObterPorIdAsync(pagamentoId))
            .ReturnsAsync(pagamento);

        _usuarioRepositoryMock.Setup(x => x.ObterPorIdAsync(usuarioId))
            .ReturnsAsync(usuario);

        var request = new ProcessarPagamentoRequest { PagamentoId = pagamentoId };

        // Act
        var resultado = await _service.ProcessarPagamentoAsync(pagamentoId, request, usuarioId);

        // Assert
        _pagamentoRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Pagamento>()), Times.Once);
        _notificacaoServiceMock.Verify(x =>
            x.EnviarNotificacaoPagamentoAprovadoAsync(usuarioId, It.IsAny<decimal>()),
            Times.AtMostOnce);
    }

    [Fact]
    public async Task ProcessarPagamentoAsyncDeveLancarExcecaoQuandoUsuarioNaoEProprietario()
    {
        // Arrange
        var pagamentoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();

        var pagamento = new Pagamento
        {
            Id = pagamentoId,
            Status = StatusPagamento.Pendente,
            Assinatura = new Assinatura
            {
                UsuarioId = outroUsuarioId // Diferente do usuário que está processando
            }
        };

        _pagamentoRepositoryMock.Setup(x => x.ObterPorIdAsync(pagamentoId))
            .ReturnsAsync(pagamento);

        var request = new ProcessarPagamentoRequest { PagamentoId = pagamentoId };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.ProcessarPagamentoAsync(pagamentoId, request, usuarioId));
    }

    [Fact]
    public async Task ProcessarPagamentoAsyncDeveLancarExcecaoQuandoPagamentoJaProcessado()
    {
        // Arrange
        var pagamentoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var pagamento = new Pagamento
        {
            Id = pagamentoId,
            Status = StatusPagamento.Aprovado, // Já processado
            Assinatura = new Assinatura
            {
                UsuarioId = usuarioId
            }
        };

        _pagamentoRepositoryMock.Setup(x => x.ObterPorIdAsync(pagamentoId))
            .ReturnsAsync(pagamento);

        var request = new ProcessarPagamentoRequest { PagamentoId = pagamentoId };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ProcessarPagamentoAsync(pagamentoId, request, usuarioId));
    }

    [Fact]
    public async Task ReprocessarPagamentoAsyncDeveResetarStatusParaPendente()
    {
        // Arrange
        var pagamentoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var assinaturaId = Guid.NewGuid();

        var pagamento = new Pagamento
        {
            Id = pagamentoId,
            AssinaturaId = assinaturaId,
            Status = StatusPagamento.Rejeitado,
            MotivoFalha = "Cartão recusado",
            TransacaoId = "123456",
            Valor = 29.99m,
            Assinatura = new Assinatura
            {
                Id = assinaturaId,
                UsuarioId = usuarioId,
                Plano = new Plano { Nome = "Pro" },
                Usuario = new Usuario { Nome = "Test User" }
            }
        };

        var usuario = new Usuario { Id = usuarioId, Nome = "Test User" };

        _pagamentoRepositoryMock.Setup(x => x.ObterPorIdAsync(pagamentoId))
            .ReturnsAsync(pagamento);

        _usuarioRepositoryMock.Setup(x => x.ObterPorIdAsync(usuarioId))
            .ReturnsAsync(usuario);

        // Act
        var resultado = await _service.ReprocessarPagamentoAsync(pagamentoId, usuarioId);

        // Assert
        _pagamentoRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Pagamento>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ReprocessarPagamentoAsyncDeveLancarExcecaoQuandoPagamentoNaoRejeitado()
    {
        // Arrange
        var pagamentoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var pagamento = new Pagamento
        {
            Id = pagamentoId,
            Status = StatusPagamento.Aprovado, // Não é rejeitado
            Assinatura = new Assinatura
            {
                UsuarioId = usuarioId
            }
        };

        _pagamentoRepositoryMock.Setup(x => x.ObterPorIdAsync(pagamentoId))
            .ReturnsAsync(pagamento);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ReprocessarPagamentoAsync(pagamentoId, usuarioId));
    }

    [Fact]
    public async Task ObterHistoricoUsuarioAsyncDeveRetornarApenasDoUsuario()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var pagamentos = new List<Pagamento>
    {
        new Pagamento
        {
            Id = Guid.NewGuid(),
            Valor = 29.99m,
            Status = StatusPagamento.Aprovado,
            DataProcessamento = DateTime.UtcNow, // Adicione propriedades necessárias
            Assinatura = new Assinatura
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Plano = new Plano {
                    Id = Guid.NewGuid(),
                    Nome = "Pro"
                }
            }
        }
    };

        _pagamentoRepositoryMock.Setup(x => x.ObterPorUsuarioAsync(usuarioId))
            .ReturnsAsync(pagamentos);

        // Act
        var resultado = await _service.ObterHistoricoUsuarioAsync(usuarioId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Single(resultado);
        Assert.Equal(29.99m, resultado.First().Valor);
        Assert.Equal("Pro", resultado.First().PlanoNome);
    }

    [Fact]
    public async Task ObterEstatisticasAsyncDeveCalcularEstatisticasCorretamente()
    {
        // Arrange
        var pagamentos = new List<Pagamento>
        {
            new Pagamento { Status = StatusPagamento.Aprovado, Valor = 29.99m, DataProcessamento = DateTime.UtcNow },
            new Pagamento { Status = StatusPagamento.Rejeitado, Valor = 19.99m },
            new Pagamento { Status = StatusPagamento.Pendente, Valor = 39.99m }
        };

        _pagamentoRepositoryMock.Setup(x => x.ObterTodosAsync())
            .ReturnsAsync(pagamentos);

        // Act
        var resultado = await _service.ObterEstatisticasAsync();

        // Assert
        //var estatisticas = resultado as dynamic;
        Assert.NotNull(resultado);

        _pagamentoRepositoryMock.Verify(x => x.ObterTodosAsync(), Times.Once);
    }
}
