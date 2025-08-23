namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para atualizar plano existente
/// </summary>
public class AtualizarPlanoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoMensal { get; set; }
    public decimal PrecoAnual { get; set; }
    public bool TemTrial { get; set; }
    public int DiasTrialGratuito { get; set; }
    public bool Ativo { get; set; }
    public string? Recursos { get; set; }
    public int Ordem { get; set; }
}
