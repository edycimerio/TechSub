using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;

namespace TechSub.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de pagamentos
/// </summary>
public interface IPagamentoService
{
    /// <summary>
    /// Obtém histórico de pagamentos do usuário
    /// </summary>
    Task<IEnumerable<PagamentoResponse>> ObterHistoricoUsuarioAsync(Guid usuarioId);

    /// <summary>
    /// Obtém todos os pagamentos
    /// </summary>
    Task<IEnumerable<PagamentoResponse>> ObterTodosAsync(string? status = null, DateTime? dataInicio = null, DateTime? dataFim = null);

    /// <summary>
    /// Obtém detalhes de um pagamento
    /// </summary>
    Task<PagamentoResponse?> ObterPorIdAsync(Guid id, Guid usuarioId);

    /// <summary>
    /// Processa pagamento (simulação)
    /// </summary>
    Task<object> ProcessarPagamentoAsync(Guid pagamentoId, ProcessarPagamentoRequest dto, Guid usuarioId);

    /// <summary>
    /// Reprocessa pagamento falhado
    /// </summary>
    Task<object> ReprocessarPagamentoAsync(Guid pagamentoId, Guid usuarioId);

    /// <summary>
    /// Obtém estatísticas de pagamentos
    /// </summary>
    Task<object> ObterEstatisticasAsync();
}
