using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;

namespace TechSub.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de assinaturas
/// </summary>
public interface IAssinaturaService
{
    /// <summary>
    /// Obtém assinaturas do usuário
    /// </summary>
    Task<IEnumerable<AssinaturaResponse>> ObterPorUsuarioAsync(Guid usuarioId, Guid usuarioLogadoId);

    /// <summary>
    /// Obtém assinatura ativa do usuário
    /// </summary>
    Task<AssinaturaResponse?> ObterAtivaAsync(Guid usuarioId, Guid usuarioLogadoId);

    /// <summary>
    /// Obtém assinatura por ID
    /// </summary>
    Task<AssinaturaResponse?> ObterPorIdAsync(Guid id, Guid usuarioId);

    /// <summary>
    /// Cria nova assinatura
    /// </summary>
    Task<AssinaturaResponse> CriarAsync(CriarAssinaturaRequest dto, Guid usuarioId);

    /// <summary>
    /// Atualiza assinatura
    /// </summary>
    Task<AssinaturaResponse?> AtualizarAsync(Guid id, AtualizarAssinaturaRequest request, Guid usuarioId);

    /// <summary>
    /// Remove assinatura
    /// </summary>
    Task<bool> RemoverAsync(Guid id, Guid usuarioId);

    /// <summary>
    /// Cancela assinatura
    /// </summary>
    Task<bool> CancelarAsync(Guid assinaturaId, Guid usuarioId);

    /// <summary>
    /// Renova assinatura
    /// </summary>
    Task<bool> RenovarAsync(Guid assinaturaId, Guid usuarioId);

    /// <summary>
    /// Obtém todas as assinaturas
    /// </summary>
    Task<IEnumerable<AssinaturaResponse>> ObterTodasAsync();

    /// <summary>
    /// Calcula MRR (Monthly Recurring Revenue)
    /// </summary>
    Task<decimal> CalcularMRRAsync();


    /// <summary>
    /// Processa trials expirados
    /// </summary>
    Task ProcessarTrialsExpiradosAsync();

    /// <summary>
    /// TESTE: Simula trial expirado
    /// </summary>
    Task SimularTrialExpiradoAsync(Guid assinaturaId, Guid usuarioId);

    /// <summary>
    /// Processa renovações automáticas
    /// </summary>
    Task ProcessarRenovacoesAutomaticasAsync();

    /// <summary>
    /// Processa cancelamentos no fim do ciclo
    /// </summary>
    Task ProcessarCancelamentosAsync();
}
