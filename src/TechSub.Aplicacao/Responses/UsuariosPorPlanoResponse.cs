namespace TechSub.Aplicacao.Responses;

/// <summary>
/// Response para relatório de usuários por plano
/// </summary>
public class UsuariosPorPlanoResponse
{
    public string PlanoNome { get; set; } = string.Empty;
    public int TotalUsuarios { get; set; }
    public List<UsuarioPlanoDetalhes> Usuarios { get; set; } = new();
}

/// <summary>
/// Detalhes do usuário no plano
/// </summary>
public class UsuarioPlanoDetalhes
{
    public Guid UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string UsuarioEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool EmTrial { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataTermino { get; set; }
}
