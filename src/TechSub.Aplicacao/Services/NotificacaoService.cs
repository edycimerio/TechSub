using TechSub.Dominio.Entidades;
using TechSub.Dominio.Enums;

namespace TechSub.Aplicacao.Services;

/// <summary>
/// Serviço para envio de notificações
/// </summary>
public class NotificacaoService
{
    /// <summary>
    /// Notifica sobre pagamento bem-sucedido
    /// </summary>
    public async Task NotificarPagamentoSucessoAsync(Pagamento pagamento, Usuario usuario)
    {
        // Simulação de envio de notificação
        var mensagem = $"Pagamento de R$ {pagamento.Valor:F2} processado com sucesso para {usuario.Nome}";
        
        // TODO: Implementar envio real (email, SMS, etc.)
        Console.WriteLine($"[NOTIFICAÇÃO SUCESSO] {mensagem}");
        
        // Simular delay de envio
        await Task.Delay(100);
    }

    /// <summary>
    /// Notifica sobre falha no pagamento
    /// </summary>
    public async Task NotificarPagamentoFalhaAsync(Pagamento pagamento, Usuario usuario)
    {
        // Simulação de envio de notificação
        var mensagem = $"Falha no pagamento de R$ {pagamento.Valor:F2} para {usuario.Nome}. Motivo: {pagamento.MensagemErro}";
        
        // TODO: Implementar envio real (email, SMS, etc.)
        Console.WriteLine($"[NOTIFICAÇÃO FALHA] {mensagem}");
        
        // Simular delay de envio
        await Task.Delay(100);
    }

    /// <summary>
    /// Notifica sobre trial prestes a expirar
    /// </summary>
    public async Task NotificarTrialExpirandoAsync(Assinatura assinatura, Usuario usuario)
    {
        var diasRestantes = (assinatura.DataTerminoTrial - DateTime.UtcNow)?.Days ?? 0;
        var mensagem = $"Seu trial expira em {diasRestantes} dias, {usuario.Nome}. Cadastre um método de pagamento para continuar.";
        
        // TODO: Implementar envio real
        Console.WriteLine($"[NOTIFICAÇÃO TRIAL] {mensagem}");
        
        await Task.Delay(100);
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
