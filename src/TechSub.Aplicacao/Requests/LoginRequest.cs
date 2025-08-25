namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para login com email e senha
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email do usuário
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário
    /// </summary>
    public string Senha { get; set; } = string.Empty;
}
