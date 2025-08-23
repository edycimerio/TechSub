using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechSub.Dominio.Enums;

namespace TechSub.Dominio.Entidades;

/// <summary>
/// Representa uma assinatura de um usuário a um plano
/// </summary>
public class Assinatura
{
    /// <summary>
    /// Identificador único da assinatura
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID do usuário proprietário da assinatura
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// ID do plano assinado
    /// </summary>
    public Guid PlanoId { get; set; }

    /// <summary>
    /// Status atual da assinatura
    /// </summary>
    public StatusAssinatura Status { get; set; } = StatusAssinatura.Ativa;

    /// <summary>
    /// Periodicidade da cobrança
    /// </summary>
    public PeriodicidadeCobranca Periodicidade { get; set; } = PeriodicidadeCobranca.Mensal;

    /// <summary>
    /// Data de início da assinatura
    /// </summary>
    public DateTime DataInicio { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data de término da assinatura (se cancelada)
    /// </summary>
    public DateTime? DataTermino { get; set; }

    /// <summary>
    /// Data da próxima cobrança
    /// </summary>
    public DateTime? DataProximaCobranca { get; set; }

    /// <summary>
    /// Indica se está em período de trial
    /// </summary>
    public bool EmTrial { get; set; }

    /// <summary>
    /// Data de término do trial
    /// </summary>
    public DateTime? DataTerminoTrial { get; set; }

    /// <summary>
    /// Valor da assinatura (pode diferir do plano por promoções)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    /// <summary>
    /// Data de criação da assinatura
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário proprietário da assinatura
    /// </summary>
    public virtual Usuario Usuario { get; set; } = null!;

    /// <summary>
    /// Plano assinado
    /// </summary>
    public virtual Plano Plano { get; set; } = null!;

    /// <summary>
    /// Histórico de pagamentos da assinatura
    /// </summary>
    public virtual ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
}
