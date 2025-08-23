namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para atualização de usuário
/// </summary>
public class AtualizarUsuarioRequest
{
    /// <summary>
    /// Nome do usuário
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// URL do avatar
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Status ativo do usuário
    /// </summary>
    public bool Ativo { get; set; }

    /// <summary>
    /// Role do usuário
    /// </summary>
    public string Role { get; set; } = "User";
}
