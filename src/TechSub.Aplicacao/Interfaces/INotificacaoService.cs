namespace TechSub.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de notificações
/// </summary>
public interface INotificacaoService
{
    /// <summary>
    /// Envia notificação de pagamento aprovado
    /// </summary>
    Task EnviarNotificacaoPagamentoAprovadoAsync(Guid usuarioId, decimal valor);

    /// <summary>
    /// Envia notificação de pagamento rejeitado
    /// </summary>
    Task EnviarNotificacaoPagamentoRejeitadoAsync(Guid usuarioId, decimal valor, string motivo);

    /// <summary>
    /// Envia notificação de trial expirando
    /// </summary>
    Task EnviarNotificacaoTrialExpirandoAsync(Guid usuarioId, int diasRestantes);

    /// <summary>
    /// Envia notificação de assinatura cancelada
    /// </summary>
    Task EnviarNotificacaoAssinaturaCanceladaAsync(Guid usuarioId, string planoNome);
}
