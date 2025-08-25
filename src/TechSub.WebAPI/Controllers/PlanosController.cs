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
[Authorize]
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
    /// Cria um novo plano
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CriarPlano([FromBody] CriarPlanoRequest dto)
    {
        try
        {
            var plano = await _planoService.CriarAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = plano.Id }, plano);
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
    /// Atualiza um plano existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> AtualizarPlano(Guid id, [FromBody] AtualizarPlanoRequest dto)
    {
        try
        {
            var plano = await _planoService.AtualizarAsync(id, dto);
            if (plano == null)
                return NotFound();

            return Ok(plano);
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
    /// Remove um plano
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoverPlano(Guid id)
    {
        try
        {
            var sucesso = await _planoService.RemoverAsync(id);
            
            if (!sucesso)
                return NotFound(new { message = "Plano não encontrado" });

            return Ok(new { message = "Plano removido com sucesso" });
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

