using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechSub.Dominio.Entidades;

/// <summary>
/// Representa um plano de assinatura disponível
/// </summary>
public class Plano
{
    /// <summary>
    /// Identificador único do plano
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nome do plano (Free, Basic, Pro)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do plano
    /// </summary>
    [MaxLength(500)]
    public string? Descricao { get; set; }

    /// <summary>
    /// Preço mensal do plano
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecoMensal { get; set; }

    /// <summary>
    /// Preço anual do plano
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecoAnual { get; set; }

    /// <summary>
    /// Indica se o plano oferece trial gratuito
    /// </summary>
    public bool TemTrial { get; set; }

    /// <summary>
    /// Duração do trial em dias
    /// </summary>
    public int DiasTrialGratuito { get; set; } = 7;

    /// <summary>
    /// Recursos inclusos no plano (JSON)
    /// </summary>
    [MaxLength(2000)]
    public string? Recursos { get; set; }

    /// <summary>
    /// Ordem de exibição do plano
    /// </summary>
    public int Ordem { get; set; }

    /// <summary>
    /// Indica se o plano está ativo para novas assinaturas
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Data de criação do plano
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Assinaturas que utilizam este plano
    /// </summary>
    public virtual ICollection<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();
}
