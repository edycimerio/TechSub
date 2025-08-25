using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;

namespace TechSub.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de planos
/// </summary>
public interface IPlanoService
{
    /// <summary>
    /// Obtém todos os planos ativos para exibição pública
    /// </summary>
    Task<IEnumerable<PlanoResponse>> ObterPlanosAtivosAsync();

    /// <summary>
    /// Obtém todos os planos
    /// </summary>
    Task<IEnumerable<PlanoResponse>> ObterTodosAsync();

    /// <summary>
    /// Obtém plano por ID
    /// </summary>
    Task<PlanoResponse?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Cria novo plano
    /// </summary>
    Task<PlanoResponse> CriarAsync(CriarPlanoRequest dto);

    /// <summary>
    /// Atualiza plano existente
    /// </summary>
    Task<PlanoResponse?> AtualizarAsync(Guid id, AtualizarPlanoRequest dto);

    /// <summary>
    /// Remove plano
    /// </summary>
    Task<bool> RemoverAsync(Guid id);
}
