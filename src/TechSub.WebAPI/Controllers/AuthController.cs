using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
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
    /// Inicia o processo de login com Google OAuth
    /// </summary>
    [HttpGet("google-login")]
    public IActionResult GoogleLogin(string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Callback do Google OAuth - processa o retorno da autenticação
    /// </summary>
    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
    {
        try
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            
            if (!authenticateResult.Succeeded)
            {
                return BadRequest(new { message = "Falha na autenticação com Google" });
            }

            var claims = authenticateResult.Principal?.Claims;
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var nome = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var avatarUrl = claims?.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(nome))
            {
                return BadRequest(new { message = "Dados incompletos do Google" });
            }

            var response = await _authService.ProcessarLoginGoogleAsync(googleId, email, nome, avatarUrl);

            // Se for uma requisição de API, retorna JSON
            if (Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return Ok(response);
            }

            // Se for navegador, redireciona com token (para desenvolvimento)
            var frontendUrl = returnUrl ?? "http://localhost:5000";
            return Redirect($"{frontendUrl}?token={response.AccessToken}");
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

