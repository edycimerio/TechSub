namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para validação de token
/// </summary>
public class TokenValidationResponse
{
    public bool Valido { get; set; }
    public string? Motivo { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? Email { get; set; }
    public string? Nome { get; set; }
    public string? Role { get; set; }
}
