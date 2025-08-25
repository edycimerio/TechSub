using System.ComponentModel.DataAnnotations;

namespace TechSub.Dominio.Entidades;

/// <summary>
/// Entidade para controlar se usuário tem método de pagamento válido
/// </summary>
public class UsuarioMetodoPagamento
{
    [Key]
    public Guid UsuarioId { get; set; }
    
    public bool TemMetodoPagamento { get; set; }
    
    // Navegação
    public virtual Usuario Usuario { get; set; } = null!;
}
