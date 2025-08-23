using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;

namespace TechSub.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de usuários
/// </summary>
public interface IUsuarioService
{
    /// <summary>
    /// Obtém todos os usuários (admin)
    /// </summary>
    Task<IEnumerable<UsuarioResponse>> ObterTodosAsync(string? userRole);

    /// <summary>
    /// Obtém usuário por ID
    /// </summary>
    Task<UsuarioResponse?> ObterPorIdAsync(Guid id, Guid usuarioLogadoId, string? userRole);

    /// <summary>
    /// Obtém perfil do usuário logado
    /// </summary>
    Task<PerfilUsuarioResponse?> ObterPerfilAsync(Guid usuarioId);

    /// <summary>
    /// Atualiza perfil do usuário
    /// </summary>
    Task<PerfilUsuarioResponse?> AtualizarPerfilAsync(Guid usuarioId, AtualizarUsuarioRequest dto);

    /// <summary>
    /// Atualiza usuário (admin)
    /// </summary>
    Task<UsuarioResponse?> AtualizarAsync(Guid id, AtualizarUsuarioRequest dto, Guid usuarioLogadoId, string? userRole);

    /// <summary>
    /// Remove usuário (admin)
    /// </summary>
    Task<bool> RemoverAsync(Guid id, Guid usuarioLogadoId);

    /// <summary>
    /// Obtém estatísticas de usuários
    /// </summary>
    Task<EstatisticasUsuarioResponse> ObterEstatisticasAsync(string? userRole);
}
