using TechSub.Aplicacao.Interfaces;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces.Repositories;

namespace TechSub.Aplicacao.Services;

/// <summary>
/// Serviço para envio de notificações
/// </summary>
public class NotificacaoService : INotificacaoService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public NotificacaoService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Envia notificação de pagamento aprovado
    /// </summary>
    public async Task EnviarNotificacaoPagamentoAprovadoAsync(Guid usuarioId, decimal valor)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario != null)
        {
            // Simulação - em produção enviaria email/SMS real
            Console.WriteLine($"Notificação: Pagamento de R$ {valor:F2} aprovado para {usuario.Email}");
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Envia notificação de pagamento rejeitado
    /// </summary>
    public async Task EnviarNotificacaoPagamentoRejeitadoAsync(Guid usuarioId, decimal valor, string motivo)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario != null)
        {
            // Simulação - em produção enviaria email/SMS real
            Console.WriteLine($"Notificação: Pagamento de R$ {valor:F2} rejeitado para {usuario.Email}. Motivo: {motivo}");
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Envia notificação de trial expirando
    /// </summary>
    public async Task EnviarNotificacaoTrialExpirandoAsync(Guid usuarioId, int diasRestantes)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario != null)
        {
            // Simulação - em produção enviaria email/SMS real
            Console.WriteLine($"Notificação: Trial expira em {diasRestantes} dias para {usuario.Email}");
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Envia notificação de assinatura cancelada
    /// </summary>
    public async Task EnviarNotificacaoAssinaturaCanceladaAsync(Guid usuarioId, string planoNome)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario != null)
        {
            // Simulação - em produção enviaria email/SMS real
            Console.WriteLine($"Notificação: Assinatura do plano {planoNome} cancelada para {usuario.Email}");
        }
        await Task.CompletedTask;
    }

    // Métodos de compatibilidade para uso interno
    public async Task EnviarNotificacaoPagamentoSucessoAsync(Usuario usuario, decimal valor)
    {
        await EnviarNotificacaoPagamentoAprovadoAsync(usuario.Id, valor);
    }

    public async Task EnviarNotificacaoPagamentoFalhaAsync(Usuario usuario, string motivo)
    {
        await EnviarNotificacaoPagamentoRejeitadoAsync(usuario.Id, 0, motivo);
    }

    /// <summary>
    /// Notifica sobre assinatura cancelada
    /// </summary>
    public async Task NotificarAssinaturaCanceladaAsync(Assinatura assinatura, Usuario usuario)
    {
        var dataTermino = assinatura.DataTermino?.ToString("dd/MM/yyyy") ?? "fim do ciclo";
        var mensagem = $"Assinatura cancelada, {usuario.Nome}. Acesso mantido até {dataTermino}.";
        
        // TODO: Implementar envio real
        Console.WriteLine($"[NOTIFICAÇÃO CANCELAMENTO] {mensagem}");
        
        await Task.Delay(100);
    }

    /// <summary>
    /// Notifica sobre renovação automática
    /// </summary>
    public async Task NotificarRenovacaoAsync(Assinatura assinatura, Usuario usuario)
    {
        var proximaCobranca = assinatura.DataProximaCobranca?.ToString("dd/MM/yyyy") ?? "próximo ciclo";
        var mensagem = $"Assinatura renovada automaticamente, {usuario.Nome}. Próxima cobrança em {proximaCobranca}.";
        
        // TODO: Implementar envio real
        Console.WriteLine($"[NOTIFICAÇÃO RENOVAÇÃO] {mensagem}");
        
        await Task.Delay(100);
    }
}
