using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSub.Aplicacao.Interfaces;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para relatório de assinaturas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RelatoriosController : ControllerBase
{
    private readonly IAssinaturaService _assinaturaService;

    public RelatoriosController(IAssinaturaService assinaturaService)
    {
        _assinaturaService = assinaturaService;
    }

    /// <summary>
    /// Obtém relatório de usuários ativos por plano
    /// </summary>
    [HttpGet("assinaturas-por-plano")]
    public async Task<IActionResult> ObterUsuariosAtivosPorPlano()
    {
        try
        {
            var assinaturas = await _assinaturaService.ObterTodasAsync();
            
            var relatorio = assinaturas
                .Where(a => a.Status == "Ativa")
                .GroupBy(a => a.PlanoNome)
                .Select(g => new
                {
                    PlanoNome = g.Key,
                    TotalUsuariosAtivos = g.Count(),
                    UsuariosAtivos = g.Select(a => new
                    {
                        UsuarioId = a.UsuarioId,
                        PlanoNome = a.PlanoNome,
                        Status = a.Status,
                        EmTrial = a.EmTrial,
                        DataInicio = a.DataInicio,
                        DataProximaCobranca = a.DataProximaCobranca
                    }).ToList()
                })
                .OrderByDescending(r => r.TotalUsuariosAtivos)
                .ToList();

            return Ok(relatorio);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
        }
    }
}
