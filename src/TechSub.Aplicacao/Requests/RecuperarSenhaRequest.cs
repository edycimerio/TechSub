namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para recuperação de senha
/// </summary>
public class RecuperarSenhaRequest
{
    public string Email { get; set; } = string.Empty;
}
