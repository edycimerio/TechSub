namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para estatísticas de usuários
/// </summary>
public class EstatisticasUsuarioResponse
{
    public int TotalUsuarios { get; set; }
    public int UsuariosAtivos { get; set; }
    public int UsuariosInativos { get; set; }
    public int NovosMes { get; set; }
}
