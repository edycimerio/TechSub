namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para alteração de senha
/// </summary>
public class AlterarSenhaResponse
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}
