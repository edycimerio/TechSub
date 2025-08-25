using Xunit;
using Moq;
using TechSub.Aplicacao.Services;
using TechSub.Aplicacao.Interfaces;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Dominio.Entidades;
using TechSub.Aplicacao.Requests;

namespace TechSub.Tests.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _service = new UsuarioService(_usuarioRepositoryMock.Object);
    }

    [Fact]
    public async Task ObterPerfilAsyncDeveRetornarUsuarioQuandoExiste()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario
        {
            Id = usuarioId,
            Nome = "João Silva",
            Email = "joao@teste.com",
            DataCriacao = DateTime.UtcNow.AddDays(-30)
        };

        _usuarioRepositoryMock.Setup(x => x.ObterPorIdAsync(usuarioId))
            .ReturnsAsync(usuario);

        // Act
        var resultado = await _service.ObterPerfilAsync(usuarioId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(usuario.Nome, resultado.Nome);
        Assert.Equal(usuario.Email, resultado.Email);
    }

    [Fact]
    public async Task ObterPerfilAsyncDeveRetornarNullQuandoUsuarioNaoExiste()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        _usuarioRepositoryMock.Setup(x => x.ObterPorIdAsync(usuarioId))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _service.ObterPerfilAsync(usuarioId);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task AtualizarPerfilAsyncDeveRetornarUsuarioAtualizadoQuandoExiste()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var usuarioExistente = new Usuario
        {
            Id = usuarioId,
            Nome = "João Silva",
            Email = "joao@teste.com"
        };

        var request = new AtualizarUsuarioRequest
        {
            Nome = "João Santos Silva",
            Email = "joao.santos@teste.com"
        };

        _usuarioRepositoryMock.Setup(x => x.ObterPorIdAsync(usuarioId))
            .ReturnsAsync(usuarioExistente);

        _usuarioRepositoryMock.Setup(x => x.ObterPorEmailAsync(request.Email))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _service.AtualizarAsync(usuarioId, request, usuarioId);

        // Assert
        Assert.Equal(request.Nome, resultado.Nome);
        Assert.Equal(request.Email, resultado.Email);
        _usuarioRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task ObterTodosAsyncDeveRetornarListaDeUsuarios()
    {
        // Arrange
        var usuarios = new List<Usuario>
    {
        new Usuario {
            Id = Guid.NewGuid(),
            Nome = "João",
            Email = "joao@teste.com",
            DataCriacao = DateTime.UtcNow, // Adicione se necessário
            Ativo = true // Adicione se necessário
        },
        new Usuario {
            Id = Guid.NewGuid(),
            Nome = "Maria",
            Email = "maria@teste.com",
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        },
        new Usuario {
            Id = Guid.NewGuid(),
            Nome = "Pedro",
            Email = "pedro@teste.com",
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        }
    };

        _usuarioRepositoryMock.Setup(x => x.ObterTodosAsync())
            .ReturnsAsync(usuarios);

        // Act
        var resultado = await _service.ObterTodosAsync();

        // Assert
        Assert.NotNull(resultado); // Adicione esta verificação primeiro
        var listaResultado = resultado.ToList(); // Materialize a lista

        Assert.Equal(3, listaResultado.Count);
        Assert.Contains(listaResultado, u => u.Nome == "João");
        Assert.Contains(listaResultado, u => u.Nome == "Maria");
        Assert.Contains(listaResultado, u => u.Nome == "Pedro");

        // Verificar se o método foi chamado
        _usuarioRepositoryMock.Verify(x => x.ObterTodosAsync(), Times.Once);
    }

}
