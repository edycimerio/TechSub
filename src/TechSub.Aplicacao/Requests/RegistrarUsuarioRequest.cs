namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para registrar novo usuário
/// </summary>
public class RegistrarUsuarioRequest
{
    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário
    /// </summary>
    public string Senha { get; set; } = string.Empty;
}
