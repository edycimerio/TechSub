namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para dados de assinatura
/// </summary>
public class AssinaturaResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid PlanoId { get; set; }
    public string PlanoNome { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Periodicidade { get; set; } = string.Empty;
    public bool EmTrial { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataTermino { get; set; }
    public DateTime? DataTerminoTrial { get; set; }
    public DateTime? DataProximaCobranca { get; set; }
    public decimal ValorMensal { get; set; }
    public decimal ValorAnual { get; set; }
}
