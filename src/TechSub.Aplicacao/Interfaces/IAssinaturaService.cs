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
    Task<IEnumerable<AssinaturaResponse>> ObterPorUsuarioAsync(Guid usuarioId, Guid usuarioLogadoId, string? userRole);

    /// <summary>
    /// Obtém assinatura ativa do usuário
    /// </summary>
    Task<AssinaturaResponse?> ObterAtivaAsync(Guid usuarioId, Guid usuarioLogadoId, string? userRole);

    /// <summary>
    /// Cria nova assinatura
    /// </summary>
    Task<AssinaturaResponse> CriarAsync(CriarAssinaturaRequest dto, Guid usuarioId);

    /// <summary>
    /// Cancela assinatura
    /// </summary>
    Task<bool> CancelarAsync(Guid assinaturaId, Guid usuarioId, string? userRole);

    /// <summary>
    /// Renova assinatura
    /// </summary>
    Task<bool> RenovarAsync(Guid assinaturaId, Guid usuarioId, string? userRole);

    /// <summary>
    /// Obtém todas as assinaturas (admin)
    /// </summary>
    Task<IEnumerable<AssinaturaResponse>> ObterTodasAsync(string? userRole);

    /// <summary>
    /// Calcula MRR (admin)
    /// </summary>
    Task<object> CalcularMRRAsync(string? userRole);

    /// <summary>
    /// Processa trials expirados
    /// </summary>
    Task ProcessarTrialsExpiradosAsync();

    /// <summary>
    /// Processa renovações automáticas
    /// </summary>
    Task ProcessarRenovacoesAutomaticasAsync();

    /// <summary>
    /// Processa cancelamentos no fim do ciclo
    /// </summary>
    Task ProcessarCancelamentosAsync();
}
