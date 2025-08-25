using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSub.Aplicacao.Interfaces;
using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para gerenciar assinaturas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssinaturasController : ControllerBase
{
    private readonly IAssinaturaService _assinaturaService;

    public AssinaturasController(IAssinaturaService assinaturaService)
    {
        _assinaturaService = assinaturaService;
    }

    /// <summary>
    /// Obter assinatura por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AssinaturaResponse>> ObterPorId(Guid id)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var assinatura = await _assinaturaService.ObterPorIdAsync(id, usuarioId);
            if (assinatura == null)
                return NotFound();
            return Ok(assinatura);
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
    /// Atualizar assinatura
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<AssinaturaResponse>> AtualizarAssinatura(Guid id, [FromBody] AtualizarAssinaturaRequest request)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var assinatura = await _assinaturaService.AtualizarAsync(id, request, usuarioId);
            if (assinatura == null)
                return NotFound();
            return Ok(assinatura);
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
    /// Criar nova assinatura
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AssinaturaResponse>> CriarAssinatura([FromBody] CriarAssinaturaRequest request)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var assinatura = await _assinaturaService.CriarAsync(request, usuarioId);
            return CreatedAtAction(nameof(ObterPorId), new { id = assinatura.Id }, assinatura);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Remover assinatura
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> RemoverAssinatura(Guid id)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var sucesso = await _assinaturaService.RemoverAsync(id, usuarioId);
            
            if (!sucesso)
                return NotFound("Assinatura não encontrada");
                
            return Ok(new { message = "Assinatura removida com sucesso" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }


    /// <summary>
    /// Listar todas as assinaturas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssinaturaResponse>>> ListarTodasAssinaturas()
    {
        try
        {
            var assinaturas = await _assinaturaService.ObterTodasAsync();
            return Ok(assinaturas);
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
    /// Processar ciclo de assinaturas (trials expirados, renovações, etc.)
    /// </summary>
    [HttpPost("processar-ciclo")]
    public async Task<ActionResult> ProcessarCicloAssinaturas()
    {
        try
        {
            await _assinaturaService.ProcessarTrialsExpiradosAsync();
            await _assinaturaService.ProcessarRenovacoesAutomaticasAsync();
            await _assinaturaService.ProcessarCancelamentosAsync();
            
            return Ok(new { message = "Ciclo de assinaturas processado com sucesso" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao processar ciclo", error = ex.Message });
        }
    }

    /// <summary>
    /// TESTE: Simular trial expirado (modificar data para ontem)
    /// </summary>
    [HttpPost("{id}/simular-trial-expirado")]
    public async Task<ActionResult> SimularTrialExpirado(Guid id)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            await _assinaturaService.SimularTrialExpiradoAsync(id, usuarioId);
            return Ok(new { message = "Trial simulado como expirado" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao simular trial", error = ex.Message });
        }
    }

    private Guid ObterUsuarioId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Token inválido");
        }
        return userId;
    }
}

