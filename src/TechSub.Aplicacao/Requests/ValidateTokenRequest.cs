namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para validação de token
/// </summary>
public class ValidateTokenRequest
{
    /// <summary>
    /// Token JWT para validar
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
