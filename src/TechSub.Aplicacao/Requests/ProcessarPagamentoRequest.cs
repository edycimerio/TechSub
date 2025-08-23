namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para processar pagamento
/// </summary>
public class ProcessarPagamentoRequest
{
    public Guid PagamentoId { get; set; }
    public Guid AssinaturaId { get; set; }
    /// <summary>
    /// Método de pagamento (pix, cartao, boleto)
    /// </summary>
    public string MetodoPagamento { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
