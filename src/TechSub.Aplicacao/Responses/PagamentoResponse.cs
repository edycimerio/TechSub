namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para dados de pagamento
/// </summary>
public class PagamentoResponse
{
    public Guid Id { get; set; }
    public Guid AssinaturaId { get; set; }
    public string PlanoNome { get; set; } = string.Empty;
    public string? UsuarioNome { get; set; }
    public string? UsuarioEmail { get; set; }
    public decimal Valor { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MetodoPagamento { get; set; }
    public string? TransacaoId { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public string? MotivoFalha { get; set; }
}
