using TechSub.Dominio.Entidades;
using TechSub.Dominio.Enums;

namespace TechSub.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de pagamentos
/// </summary>
public interface IPagamentoRepository
{
    /// <summary>
    /// Obtém um pagamento por ID
    /// </summary>
    Task<Pagamento?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Obtém pagamentos por assinatura
    /// </summary>
    Task<IEnumerable<Pagamento>> ObterPorAssinaturaAsync(Guid assinaturaId);

    /// <summary>
    /// Obtém pagamentos por status
    /// </summary>
    Task<IEnumerable<Pagamento>> ObterPorStatusAsync(StatusPagamento status);

    /// <summary>
    /// Obtém pagamentos por período
    /// </summary>
    Task<IEnumerable<Pagamento>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);

    /// <summary>
    /// Adiciona um novo pagamento
    /// </summary>
    Task<Pagamento> AdicionarAsync(Pagamento pagamento);

    /// <summary>
    /// Atualiza um pagamento existente
    /// </summary>
    Task<Pagamento> AtualizarAsync(Pagamento pagamento);

    /// <summary>
    /// Remove um pagamento
    /// </summary>
    Task RemoverAsync(Guid id);

    /// <summary>
    /// Obtém último pagamento de uma assinatura
    /// </summary>
    Task<Pagamento?> ObterUltimoPagamentoAsync(Guid assinaturaId);
}
