namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para perfil completo do usuário
/// </summary>
public class PerfilUsuarioResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataUltimoLogin { get; set; }
    public bool Ativo { get; set; }
}
