namespace TechSub.Aplicacao.DTOs;

/// <summary>
/// DTO para resposta de login
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Token JWT de acesso
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do token (sempre "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Tempo de expiração em segundos
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Dados do usuário logado
    /// </summary>
    public UsuarioDto Usuario { get; set; } = new();
}

/// <summary>
/// DTO para dados do usuário
/// </summary>
public class UsuarioDto
{
    /// <summary>
    /// ID do usuário
    /// </summary>
    public Guid Id { get; set; }

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
    /// Provedor de autenticação
    /// </summary>
    public string? Provedor { get; set; }

    /// <summary>
    /// Data do último login
    /// </summary>
    public DateTime? DataUltimoLogin { get; set; }
}
