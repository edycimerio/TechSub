namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para criar novo plano
/// </summary>
public class CriarPlanoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoMensal { get; set; }
    public decimal PrecoAnual { get; set; }
    public bool TemTrial { get; set; }
    public int DiasTrialGratuito { get; set; }
    public string? Recursos { get; set; }
    public int Ordem { get; set; }
}
