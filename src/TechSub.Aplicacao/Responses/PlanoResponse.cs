namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para dados de plano
/// </summary>
public class PlanoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoMensal { get; set; }
    public decimal PrecoAnual { get; set; }
    public bool TemTrial { get; set; }
    public int DiasTrialGratuito { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public string? Recursos { get; set; }
    public int Ordem { get; set; }
}
