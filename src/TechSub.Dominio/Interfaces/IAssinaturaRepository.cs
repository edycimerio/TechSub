using TechSub.Dominio.Entidades;
using TechSub.Dominio.Enums;

namespace TechSub.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de assinaturas
/// </summary>
public interface IAssinaturaRepository
{
    /// <summary>
    /// Obtém uma assinatura por ID
    /// </summary>
    Task<Assinatura?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Obtém assinaturas por usuário
    /// </summary>
    Task<IEnumerable<Assinatura>> ObterPorUsuarioAsync(Guid usuarioId);

    /// <summary>
    /// Obtém assinatura ativa do usuário
    /// </summary>
    Task<Assinatura?> ObterAtivaDoUsuarioAsync(Guid usuarioId);

    /// <summary>
    /// Obtém assinaturas por status
    /// </summary>
    Task<IEnumerable<Assinatura>> ObterPorStatusAsync(StatusAssinatura status);

    /// <summary>
    /// Obtém assinaturas que vencem em determinado período
    /// </summary>
    Task<IEnumerable<Assinatura>> ObterVencendoAsync(DateTime dataInicio, DateTime dataFim);

    /// <summary>
    /// Adiciona uma nova assinatura
    /// </summary>
    Task<Assinatura> AdicionarAsync(Assinatura assinatura);

    /// <summary>
    /// Atualiza uma assinatura existente
    /// </summary>
    Task<Assinatura> AtualizarAsync(Assinatura assinatura);

    /// <summary>
    /// Remove uma assinatura
    /// </summary>
    Task RemoverAsync(Guid id);

    /// <summary>
    /// Obtém relatório de usuários ativos por plano
    /// </summary>
    Task<Dictionary<string, int>> ObterUsuariosAtivosPorPlanoAsync();

    /// <summary>
    /// Calcula MRR (Monthly Recurring Revenue) por mês/ano
    /// </summary>
    Task<decimal> CalcularMrrAsync(int mes, int ano);
}
