namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para dados de usuário
/// </summary>
public class UsuarioResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool Ativo { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataUltimoLogin { get; set; }
}
