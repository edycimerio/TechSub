using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;
using TechSub.Aplicacao.Interfaces;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para autenticação e autorização
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Login com email e senha
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Senha))
            {
                return BadRequest(new { message = "Email e senha são obrigatórios" });
            }

            var response = await _authService.LoginAsync(request.Email, request.Senha);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Email ou senha inválidos" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Registrar novo usuário
    /// </summary>
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Senha) || string.IsNullOrEmpty(request.Nome))
            {
                return BadRequest(new { message = "Nome, email e senha são obrigatórios" });
            }

            var response = await _authService.RegistrarUsuarioAsync(request.Nome, request.Email, request.Senha);
            return Ok(response);
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
    /// Obtém informações do usuário autenticado
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> ObterUsuarioAtual()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var usuario = await _authService.ObterUsuarioAtualAsync(userId);
            return Ok(usuario);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Valida se um token JWT é válido
    /// </summary>
    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidarToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(new { message = "Token é obrigatório" });
            }

            var isValid = _authService.ValidarToken(request.Token);
            
            if (!isValid)
            {
                return BadRequest(new { message = "Token inválido ou expirado" });
            }

            var claims = await _authService.ObterClaimsDoTokenAsync(request.Token);
            
            return Ok(new 
            { 
                valid = true,
                claims = claims
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Logout do usuário (placeholder - implementar blacklist de tokens)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        // TODO: Implementar blacklist de tokens JWT
        return Ok(new { message = "Logout realizado com sucesso" });
    }

    /// <summary>
    /// Recuperação de senha (simulada)
    /// </summary>
    [HttpPost("recuperar-senha")]
    public async Task<ActionResult> RecuperarSenha([FromBody] RecuperarSenhaRequest request)
    {
        try
        {
            await _authService.RecuperarSenhaAsync(request.Email);
            return Ok(new { message = "Se o email existir, você receberá instruções para recuperação" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }
}

