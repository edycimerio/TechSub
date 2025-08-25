using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;
using TechSub.Aplicacao.Interfaces;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para gerenciamento de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]

public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Obtém todos os usuários (apenas admins)
    /// </summary>
    [HttpGet]    
    public async Task<IActionResult> ObterTodos()
    {
        try
        {
            var userRole = User.FindFirst("role")?.Value;
            var usuarios = await _usuarioService.ObterTodosAsync(userRole);
            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            var usuarioLogadoId = ObterUsuarioId();
            var userRole = User.FindFirst("role")?.Value;
            
            var usuario = await _usuarioService.ObterPorIdAsync(id, usuarioLogadoId, userRole);
            
            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza dados do usuário
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarUsuarioRequest dto)
    {
        try
        {
            var usuarioLogadoId = ObterUsuarioId();
            var userRole = User.FindFirst("role")?.Value;
            
            var usuario = await _usuarioService.AtualizarAsync(id, dto, usuarioLogadoId, userRole);
            
            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Desativa usuário (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Desativar(Guid id)
    {
        try
        {
            var usuarioLogadoId = ObterUsuarioId();
            var userRole = User.FindFirst("role")?.Value;
            
            var result = await _usuarioService.RemoverAsync(id, usuarioLogadoId);
            
            if (!result)
            {
                return NotFound();
            }

            return Ok(new { message = "Usuário desativado com sucesso" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém ID do usuário atual a partir do token
    /// </summary>
    private Guid ObterUsuarioId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new UnauthorizedAccessException("Token inválido");
        }

        return userGuid;
    }
}

