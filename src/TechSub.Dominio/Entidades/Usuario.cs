using System.ComponentModel.DataAnnotations;

namespace TechSub.Dominio.Entidades;

/// <summary>
/// Representa um usuário do sistema TechSub
/// </summary>
public class Usuario
{
    /// <summary>
    /// Identificador único do usuário
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário (único no sistema)
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash da senha (apenas para login tradicional)
    /// </summary>
    [MaxLength(500)]
    public string? SenhaHash { get; set; }

    /// <summary>
    /// ID do provedor externo (se houver)
    /// </summary>
    [MaxLength(255)]
    public string? ProvedorId { get; set; }

    /// <summary>
    /// Nome do provedor (local, Google, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? Provedor { get; set; } = "local";

    /// <summary>
    /// URL do avatar do usuário
    /// </summary>
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Data do último login
    /// </summary>
    public DateTime? DataUltimoLogin { get; set; }

    /// <summary>
    /// Data de criação do usuário
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica se o usuário está ativo
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Role do usuário no sistema (User, Admin)
    /// </summary>
    [MaxLength(50)]
    public string Role { get; set; } = "User";


    /// <summary>
    /// Assinatura ativa do usuário (relacionamento 1:1)
    /// </summary>
    public virtual Assinatura? AssinaturaAtiva { get; set; }

    /// <summary>
    /// Histórico de todas as assinaturas do usuário
    /// </summary>
    public virtual ICollection<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();
}
