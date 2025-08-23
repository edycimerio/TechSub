using TechSub.Dominio.Entidades;

namespace TechSub.Dominio.Interfaces.Repositories;

/// <summary>
/// Interface para repositório de pagamentos
/// </summary>
public interface IPagamentoRepository
{
    /// <summary>
    /// Obtém todos os pagamentos
    /// </summary>
    Task<IEnumerable<Pagamento>> ObterTodosAsync();

    /// <summary>
    /// Obtém pagamento por ID
    /// </summary>
    Task<Pagamento?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Obtém pagamentos por usuário
    /// </summary>
    Task<IEnumerable<Pagamento>> ObterPorUsuarioAsync(Guid usuarioId);

    /// <summary>
    /// Obtém pagamentos por assinatura
    /// </summary>
    Task<IEnumerable<Pagamento>> ObterPorAssinaturaAsync(Guid assinaturaId);

    /// <summary>
    /// Obtém pagamentos por período
    /// </summary>
    Task<IEnumerable<Pagamento>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);

    /// <summary>
    /// Obtém pagamentos falhados
    /// </summary>
    Task<IEnumerable<Pagamento>> ObterFalhadosAsync();

    /// <summary>
    /// Adiciona novo pagamento
    /// </summary>
    Task AdicionarAsync(Pagamento pagamento);

    /// <summary>
    /// Atualiza pagamento existente
    /// </summary>
    Task AtualizarAsync(Pagamento pagamento);

    /// <summary>
    /// Remove pagamento
    /// </summary>
    Task RemoverAsync(Guid id);
}
