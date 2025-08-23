namespace TechSub.Dominio.Enums;

/// <summary>
/// Status possíveis de um pagamento
/// </summary>
public enum StatusPagamento
{
    /// <summary>
    /// Pagamento aguardando processamento
    /// </summary>
    Pendente = 1,

    /// <summary>
    /// Pagamento processado com sucesso
    /// </summary>
    Aprovado = 2,

    /// <summary>
    /// Pagamento rejeitado/falhou
    /// </summary>
    Rejeitado = 3,

    /// <summary>
    /// Pagamento cancelado
    /// </summary>
    Cancelado = 4,

    /// <summary>
    /// Pagamento estornado
    /// </summary>
    Estornado = 5
}
