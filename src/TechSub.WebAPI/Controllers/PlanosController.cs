using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;
using TechSub.Aplicacao.Interfaces;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para gerenciamento de planos de assinatura
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PlanosController : ControllerBase
{
    private readonly IPlanoService _planoService;

    public PlanosController(IPlanoService planoService)
    {
        _planoService = planoService;
    }

    /// <summary>
    /// Obtém todos os planos ativos (público)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterPlanosAtivos()
    {
        try
        {
            var planos = await _planoService.ObterPlanosAtivosAsync();
            return Ok(planos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um plano específico por ID (público)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            var plano = await _planoService.ObterPorIdAsync(id);
            
            if (plano == null)
                return NotFound();

            return Ok(plano);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém todos os planos incluindo inativos (apenas admins)
    /// </summary>
    [HttpGet("admin/todos")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ObterTodosPlanos()
    {
        try
        {
            var userRole = User.FindFirst("role")?.Value;
            var planos = await _planoService.ObterTodosAsync(userRole);
            return Ok(planos);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Cria um novo plano (apenas admins)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CriarPlano([FromBody] CriarPlanoRequest dto)
    {
        try
        {
            var userRole = User.FindFirst("role")?.Value;
            var plano = await _planoService.CriarAsync(dto, userRole);
            return CreatedAtAction(nameof(ObterPorId), new { id = plano.Id }, plano);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um plano existente (apenas admins)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AtualizarPlano(Guid id, [FromBody] AtualizarPlanoRequest dto)
    {
        try
        {
            var userRole = User.FindFirst("role")?.Value;
            var plano = await _planoService.AtualizarAsync(id, dto, userRole);
            if (plano == null)
                return NotFound();

            return Ok(plano);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Remove um plano (apenas admins)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoverPlano(Guid id)
    {
        try
        {
            var userRole = User.FindFirst("role")?.Value;
            var sucesso = await _planoService.RemoverAsync(id, userRole);
            
            if (!sucesso)
                return NotFound(new { message = "Plano não encontrado" });

            return Ok(new { message = "Plano removido com sucesso" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }
}

