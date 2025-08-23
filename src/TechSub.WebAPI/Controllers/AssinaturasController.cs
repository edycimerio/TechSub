using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;
using TechSub.Aplicacao.Interfaces;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para gerenciar assinaturas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AssinaturasController : ControllerBase
{
    private readonly IAssinaturaService _assinaturaService;

    public AssinaturasController(IAssinaturaService assinaturaService)
    {
        _assinaturaService = assinaturaService;
    }

    /// <summary>
    /// Obter assinaturas do usuário logado
    /// </summary>
    [HttpGet("minhas")]
    public async Task<ActionResult<IEnumerable<AssinaturaResponse>>> ObterMinhasAssinaturas()
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var userRole = User.FindFirst("role")?.Value;
            var assinaturas = await _assinaturaService.ObterPorUsuarioAsync(usuarioId, usuarioId, userRole);
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
    /// Obter assinatura ativa do usuário
    /// </summary>
    [HttpGet("ativa")]
    public async Task<ActionResult<AssinaturaResponse>> ObterAssinaturaAtiva()
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var userRole = User.FindFirst("role")?.Value;
            var assinatura = await _assinaturaService.ObterAtivaAsync(usuarioId, usuarioId, userRole);
            
            if (assinatura == null)
                return NotFound("Nenhuma assinatura ativa encontrada");
                
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
            var userRole = User.FindFirst("role")?.Value;
            var assinatura = await _assinaturaService.CriarAsync(request, usuarioId);
            return CreatedAtAction(nameof(ObterAssinaturaAtiva), assinatura);
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
    /// Cancelar assinatura
    /// </summary>
    [HttpPost("{id}/cancelar")]
    public async Task<ActionResult> CancelarAssinatura(Guid id)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var userRole = User.FindFirst("role")?.Value;
            var sucesso = await _assinaturaService.CancelarAsync(id, usuarioId, userRole);
            
            if (!sucesso)
                return NotFound("Assinatura não encontrada");
                
            return Ok(new { message = "Assinatura cancelada com sucesso" });
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
    /// Renovar assinatura
    /// </summary>
    [HttpPost("{id}/renovar")]
    public async Task<ActionResult> RenovarAssinatura(Guid id)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var userRole = User.FindFirst("role")?.Value;
            var sucesso = await _assinaturaService.RenovarAsync(id, usuarioId, userRole);
            
            if (!sucesso)
                return NotFound("Assinatura não encontrada");
                
            return Ok(new { message = "Assinatura renovada com sucesso" });
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
    /// Listar todas as assinaturas (apenas Admin)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssinaturaAdminResponse>>> ListarTodasAssinaturas()
    {
        try
        {
            var userRole = User.FindFirst("role")?.Value;
            var assinaturas = await _assinaturaService.ObterTodasAsync(userRole);
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
    /// Obter relatório de MRR (apenas Admin)
    /// </summary>
    [HttpGet("relatorio/mrr")]
    public async Task<ActionResult<decimal>> ObterMRR()
    {
        try
        {
            var userRole = User.FindFirst("role")?.Value;
            var mrr = await _assinaturaService.CalcularMRRAsync(userRole);
            return Ok(new { mrr = mrr, periodo = DateTime.UtcNow.ToString("yyyy-MM") });
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

