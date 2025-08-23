using TechSub.Dominio.Entidades;
using TechSub.Dominio.Enums;

namespace TechSub.Dominio.Interfaces.Repositories;

/// <summary>
/// Interface para repositório de assinaturas
/// </summary>
public interface IAssinaturaRepository
{
    /// <summary>
    /// Obtém todas as assinaturas
    /// </summary>
    Task<IEnumerable<Assinatura>> ObterTodosAsync();

    /// <summary>
    /// Obtém todas as assinaturas
    /// </summary>
    Task<IEnumerable<Assinatura>> ObterTodasAsync();

    /// <summary>
    /// Obtém assinatura por ID
    /// </summary>
    Task<Assinatura?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Obtém assinaturas por usuário
    /// </summary>
    Task<IEnumerable<Assinatura>> ObterPorUsuarioAsync(Guid usuarioId);

    /// <summary>
    /// Obtém assinatura ativa do usuário
    /// </summary>
    Task<Assinatura?> ObterAtivaAsync(Guid usuarioId);

    /// <summary>
    /// Obtém assinatura ativa por usuário
    /// </summary>
    Task<Assinatura?> ObterAtivaPorUsuarioAsync(Guid usuarioId);

    /// <summary>
    /// Obtém assinaturas por status
    /// </summary>
    Task<IEnumerable<Assinatura>> ObterPorStatusAsync(StatusAssinatura status);

    /// <summary>
    /// Obtém assinaturas expiradas
    /// </summary>
    Task<IEnumerable<Assinatura>> ObterExpiradasAsync();

    /// <summary>
    /// Obtém assinaturas para renovação
    /// </summary>
    Task<IEnumerable<Assinatura>> ObterParaRenovacaoAsync();

    /// <summary>
    /// Obtém assinaturas para cancelamento
    /// </summary>
    Task<IEnumerable<Assinatura>> ObterParaCancelamentoAsync();

    /// <summary>
    /// Adiciona nova assinatura
    /// </summary>
    Task AdicionarAsync(Assinatura assinatura);

    /// <summary>
    /// Atualiza assinatura existente
    /// </summary>
    Task AtualizarAsync(Assinatura assinatura);

    /// <summary>
    /// Remove assinatura
    /// </summary>
    Task RemoverAsync(Guid id);

    /// <summary>
    /// Calcula MRR (Monthly Recurring Revenue)
    /// </summary>
    Task<decimal> CalcularMRRAsync();

    /// <summary>
    /// Obtém relatório de assinaturas
    /// </summary>
    Task<object> ObterRelatorioAsync();
}
