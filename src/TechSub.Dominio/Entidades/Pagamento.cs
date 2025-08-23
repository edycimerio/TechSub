using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechSub.Dominio.Enums;

namespace TechSub.Dominio.Entidades;

/// <summary>
/// Representa um pagamento de assinatura
/// </summary>
public class Pagamento
{
    /// <summary>
    /// Identificador único do pagamento
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID da assinatura relacionada
    /// </summary>
    public Guid AssinaturaId { get; set; }

    /// <summary>
    /// Valor do pagamento
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    /// <summary>
    /// Status do pagamento
    /// </summary>
    public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;

    /// <summary>
    /// Data de vencimento do pagamento
    /// </summary>
    public DateTime DataVencimento { get; set; }

    /// <summary>
    /// Data de processamento do pagamento
    /// </summary>
    public DateTime? DataProcessamento { get; set; }

    /// <summary>
    /// Método de pagamento utilizado
    /// </summary>
    [MaxLength(50)]
    public string? MetodoPagamento { get; set; }

    /// <summary>
    /// ID da transação no provedor de pagamento
    /// </summary>
    [MaxLength(255)]
    public string? TransacaoId { get; set; }

    /// <summary>
    /// Mensagem de erro (se houver falha)
    /// </summary>
    [MaxLength(500)]
    public string? MensagemErro { get; set; }

    /// <summary>
    /// Motivo da falha no pagamento
    /// </summary>
    [MaxLength(500)]
    public string? MotivoFalha { get; set; }

    /// <summary>
    /// Número de tentativas de cobrança
    /// </summary>
    public int TentativasCobranca { get; set; } = 0;

    /// <summary>
    /// Data de criação do pagamento
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Assinatura relacionada ao pagamento
    /// </summary>
    public virtual Assinatura Assinatura { get; set; } = null!;
}
