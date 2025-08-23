namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para criar nova assinatura
/// </summary>
public class CriarAssinaturaRequest
{
    public Guid PlanoId { get; set; }
    public string TipoCobranca { get; set; } = "mensal"; // mensal ou anual
}
