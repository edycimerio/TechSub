using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSub.Aplicacao.DTOs;
using TechSub.Aplicacao.Services;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para autenticação e autorização
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly IUsuarioRepository _usuarioRepository;

    public AuthController(AuthService authService, IConfiguration configuration, IUsuarioRepository usuarioRepository)
    {
        _authService = authService;
        _configuration = configuration;
        _usuarioRepository = usuarioRepository;
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

            var (usuario, token) = await _authService.ProcessarLoginGoogleAsync(googleId, email, nome, avatarUrl);

            var response = new LoginResponseDto
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = 24 * 3600, // 24 horas em segundos
                Usuario = new UsuarioDto
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    AvatarUrl = usuario.AvatarUrl,
                    Provedor = usuario.Provedor,
                    DataUltimoLogin = usuario.DataUltimoLogin
                }
            };

            // Se for uma requisição de API, retorna JSON
            if (Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return Ok(response);
            }

            // Se for navegador, redireciona com token (para desenvolvimento)
            var frontendUrl = returnUrl ?? "http://localhost:5000";
            return Redirect($"{frontendUrl}?token={token}");
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

            // Aqui você buscaria o usuário no repositório
            // Por enquanto, retornamos os dados do token
            var usuario = new UsuarioDto
            {
                Id = userGuid,
                Nome = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                Provedor = User.FindFirst("provider")?.Value
            };

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
    public IActionResult ValidarToken([FromBody] ValidateTokenRequest request)
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

            var claims = _authService.ObterClaimsDoToken(request.Token);
            
            return Ok(new 
            { 
                valid = true,
                userId = claims?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                email = claims?.FindFirst(ClaimTypes.Email)?.Value,
                name = claims?.FindFirst(ClaimTypes.Name)?.Value
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
    public async Task<ActionResult> RecuperarSenha([FromBody] RecuperarSenhaDto request)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email);
        if (usuario == null)
        {
            // Por segurança, sempre retorna sucesso mesmo se email não existir
            return Ok(new { message = "Se o email existir, você receberá instruções para recuperação" });
        }

        // TODO: Implementar envio real de email
        // Por enquanto, apenas simula o envio
        Console.WriteLine($"[RECUPERAÇÃO SENHA] Email enviado para {request.Email}");

        return Ok(new { message = "Se o email existir, você receberá instruções para recuperação" });
    }
}

/// <summary>
/// DTO para recuperação de senha
/// </summary>
public class RecuperarSenhaDto
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request para validação de token
/// </summary>
public class ValidateTokenRequest
{
    /// <summary>
    /// Token JWT para validar
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
