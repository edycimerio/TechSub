namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para estatísticas de pagamentos
/// </summary>
public class EstatisticasPagamentoResponse
{
    public int TotalPagamentos { get; set; }
    public int PagamentosAprovados { get; set; }
    public int PagamentosRejeitados { get; set; }
    public int PagamentosPendentes { get; set; }
    public decimal ReceitaTotal { get; set; }
    public decimal ReceitaMesAtual { get; set; }
    public double TaxaAprovacao { get; set; }
}
