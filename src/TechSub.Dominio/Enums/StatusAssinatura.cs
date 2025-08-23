namespace TechSub.Dominio.Enums;

/// <summary>
/// Status possíveis de uma assinatura
/// </summary>
public enum StatusAssinatura
{
    /// <summary>
    /// Assinatura ativa e funcionando
    /// </summary>
    Ativa = 1,

    /// <summary>
    /// Assinatura em período de trial
    /// </summary>
    Trial = 2,

    /// <summary>
    /// Assinatura cancelada pelo usuário
    /// </summary>
    Cancelada = 3,

    /// <summary>
    /// Assinatura suspensa por falta de pagamento
    /// </summary>
    Suspensa = 4,

    /// <summary>
    /// Assinatura expirada
    /// </summary>
    Expirada = 5
}
