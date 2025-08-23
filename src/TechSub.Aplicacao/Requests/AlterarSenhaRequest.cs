namespace TechSub.Aplicacao.Requests;

/// <summary>
/// Request para alteração de senha
/// </summary>
public class AlterarSenhaRequest
{
    public string TokenRecuperacao { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}
