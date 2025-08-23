namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para login via Google OAuth
/// </summary>
public class LoginGoogleRequest
{
    public string GoogleId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
