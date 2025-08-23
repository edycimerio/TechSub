namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para visualização admin de assinaturas
/// </summary>
public class AssinaturaAdminResponse
{
    public Guid Id { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string UsuarioEmail { get; set; } = string.Empty;
    public string PlanoNome { get; set; } = string.Empty;
    public decimal PlanoValor { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public DateTime? DataProximaCobranca { get; set; }
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
}
