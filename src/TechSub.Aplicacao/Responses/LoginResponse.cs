namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para login com token de acesso
/// </summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public DateTime ExpiresAt { get; set; }
    public PerfilUsuarioResponse Usuario { get; set; } = new();
}
