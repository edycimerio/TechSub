using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Aplicacao.Interfaces;

namespace TechSub.Aplicacao.Services;

/// <summary>
/// Serviço para gerenciar usuários
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Obtém todos os usuários (admin)
    /// </summary>
    public async Task<IEnumerable<UsuarioResponse>> ObterTodosAsync(string? userRole = null)
    {
        // Validar autorização - apenas Admin pode listar todos os usuários
        if (userRole != "Admin")
        {
            throw new UnauthorizedAccessException("Acesso negado. Apenas administradores podem listar usuários.");
        }

        var usuarios = await _usuarioRepository.ObterTodosAsync();
        
        return usuarios.Select(u => new UsuarioResponse
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            GoogleId = u.GoogleId,
            AvatarUrl = u.AvatarUrl,
            Ativo = u.Ativo,
            Role = u.Role,
            DataCriacao = u.DataCriacao,
            DataUltimoLogin = u.DataUltimoLogin
        });
    }

    /// <summary>
    /// Obtém usuário por ID
    /// </summary>
    public async Task<UsuarioResponse?> ObterPorIdAsync(Guid id, Guid usuarioLogadoId, string? userRole = null)
    {
        // Validar autorização - usuário só pode ver seus próprios dados, exceto Admin
        if (userRole != "Admin" && usuarioLogadoId != id)
        {
            throw new UnauthorizedAccessException("Acesso negado. Você só pode acessar seus próprios dados.");
        }

        var usuario = await _usuarioRepository.ObterPorIdAsync(id);
        if (usuario == null) return null;

        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            GoogleId = usuario.GoogleId,
            AvatarUrl = usuario.AvatarUrl,
            Ativo = usuario.Ativo,
            Role = usuario.Role,
            DataCriacao = usuario.DataCriacao,
            DataUltimoLogin = usuario.DataUltimoLogin
        };
    }

    /// <summary>
    /// Obtém perfil do usuário logado
    /// </summary>
    public async Task<PerfilUsuarioResponse?> ObterPerfilAsync(Guid usuarioId)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null) return null;

        return new PerfilUsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            AvatarUrl = usuario.AvatarUrl,
            DataCriacao = usuario.DataCriacao,
            DataUltimoLogin = usuario.DataUltimoLogin
        };
    }

    /// <summary>
    /// Atualiza perfil do usuário
    /// </summary>
    public async Task<PerfilUsuarioResponse?> AtualizarPerfilAsync(Guid usuarioId, AtualizarUsuarioRequest dto)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null) return null;

        usuario.Nome = dto.Nome;
        usuario.AvatarUrl = dto.AvatarUrl;

        await _usuarioRepository.AtualizarAsync(usuario);

        return new PerfilUsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            AvatarUrl = usuario.AvatarUrl,
            DataCriacao = usuario.DataCriacao,
            DataUltimoLogin = usuario.DataUltimoLogin
        };
    }

    /// <summary>
    /// Atualiza usuário (admin)
    /// </summary>
    public async Task<UsuarioResponse?> AtualizarAsync(Guid id, AtualizarUsuarioRequest dto, Guid usuarioLogadoId, string? userRole = null)
    {
        // Validar autorização - usuário só pode atualizar seus próprios dados, exceto Admin
        if (userRole != "Admin" && usuarioLogadoId != id)
        {
            throw new UnauthorizedAccessException("Acesso negado. Você só pode atualizar seus próprios dados.");
        }

        var usuario = await _usuarioRepository.ObterPorIdAsync(id);
        if (usuario == null) return null;

        usuario.Nome = dto.Nome;
        usuario.Email = dto.Email;
        usuario.Ativo = dto.Ativo;
        usuario.Role = dto.Role;

        await _usuarioRepository.AtualizarAsync(usuario);

        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            GoogleId = usuario.GoogleId,
            AvatarUrl = usuario.AvatarUrl,
            Ativo = usuario.Ativo,
            Role = usuario.Role,
            DataCriacao = usuario.DataCriacao,
            DataUltimoLogin = usuario.DataUltimoLogin
        };
    }

    /// <summary>
    /// Remove usuário (admin)
    /// </summary>
    public async Task<bool> RemoverAsync(Guid id, Guid usuarioLogadoId)
    {
        // Validar autorização - usuário não pode remover a si mesmo
        if (id == usuarioLogadoId)
        {
            throw new UnauthorizedAccessException("Usuário não pode remover a si mesmo.");
        }

        var usuario = await _usuarioRepository.ObterPorIdAsync(id);
        if (usuario == null) return false;

        await _usuarioRepository.RemoverAsync(id);
        return true;
    }

    /// <summary>
    /// Obtém estatísticas de usuários
    /// </summary>
    public async Task<EstatisticasUsuarioResponse> ObterEstatisticasAsync(string? userRole = null)
    {
        // Validar autorização - apenas Admin pode ver estatísticas
        if (userRole != "Admin")
        {
            throw new UnauthorizedAccessException("Acesso negado. Apenas administradores podem ver estatísticas.");
        }

        var usuarios = await _usuarioRepository.ObterTodosAsync();
        
        return new EstatisticasUsuarioResponse
        {
            TotalUsuarios = usuarios.Count(),
            UsuariosAtivos = usuarios.Count(u => u.Ativo),
            UsuariosInativos = usuarios.Count(u => !u.Ativo),
            NovosMes = usuarios.Count(u => u.DataCriacao >= DateTime.UtcNow.AddDays(-30))
        };
    }
}
