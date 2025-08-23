namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para recuperação de senha
/// </summary>
public class RecuperarSenhaResponse
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string? TokenRecuperacao { get; set; }
}
