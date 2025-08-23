using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;
using TechSub.Aplicacao.Interfaces;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para gerenciar pagamentos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PagamentosController : ControllerBase
{
    private readonly IPagamentoService _pagamentoService;

    public PagamentosController(IPagamentoService pagamentoService)
    {
        _pagamentoService = pagamentoService;
    }

    /// <summary>
    /// Obter histórico de pagamentos do usuário
    /// </summary>
    [HttpGet("meus")]
    public async Task<ActionResult<IEnumerable<PagamentoResponse>>> ObterMeusPagamentos()
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var pagamentos = await _pagamentoService.ObterHistoricoUsuarioAsync(usuarioId);
            return Ok(pagamentos);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obter detalhes de um pagamento específico
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PagamentoResponse>> ObterPagamento(Guid id)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var pagamento = await _pagamentoService.ObterPorIdAsync(id, usuarioId);
            
            if (pagamento == null)
                return NotFound();

            return Ok(pagamento);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Simular processamento de pagamento
    /// </summary>
    [HttpPost("{id}/processar")]
    public async Task<ActionResult> ProcessarPagamento(Guid id, [FromBody] ProcessarPagamentoRequest request)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var response = await _pagamentoService.ProcessarPagamentoAsync(id, request, usuarioId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
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
    /// Reprocessar pagamento falhado
    /// </summary>
    [HttpPost("{id}/reprocessar")]
    public async Task<ActionResult> ReprocessarPagamento(Guid id)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var response = await _pagamentoService.ReprocessarPagamentoAsync(id, usuarioId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
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
    /// Listar todos os pagamentos (apenas Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<PagamentoResponse>>> ListarTodosPagamentos(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            var userRole = User.FindFirst("role")?.Value;
            var pagamentos = await _pagamentoService.ObterTodosAsync(userRole, status, dataInicio, dataFim);
            return Ok(pagamentos);
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
    /// Obter estatísticas de pagamentos (apenas Admin)
    /// </summary>
    [HttpGet("estatisticas")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ObterEstatisticas()
    {
        try
        {
            var userRole = User.FindFirst("role")?.Value;
            var estatisticas = await _pagamentoService.ObterEstatisticasAsync(userRole);
            return Ok(estatisticas);
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

