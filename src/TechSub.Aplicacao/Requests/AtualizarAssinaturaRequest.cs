namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para atualizar assinatura
/// </summary>
public class AtualizarAssinaturaRequest
{
    public Guid PlanoId { get; set; }
    public string TipoCobranca { get; set; } = "mensal"; // mensal ou anual
}
