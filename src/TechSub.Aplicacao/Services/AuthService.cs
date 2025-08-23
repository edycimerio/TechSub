using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;
using TechSub.Dominio.Enums;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Aplicacao.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;

namespace TechSub.Aplicacao.Services;

/// <summary>
/// Serviço para autenticação e autorização
/// </summary>
public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUsuarioRepository _usuarioRepository;

    public AuthService(IConfiguration configuration, IUsuarioRepository usuarioRepository)
    {
        _configuration = configuration;
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Gera token JWT para um usuário
    /// </summary>
    public string GerarTokenJwt(Usuario usuario)
    {
        var jwtSettings = _configuration.GetSection("Authentication:JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationHours = int.Parse(jwtSettings["ExpirationHours"] ?? "24");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim("provider", usuario.Provedor ?? "local"),
            new Claim("provider_id", usuario.ProvedorId ?? ""),
            new Claim(ClaimTypes.Role, "User") // Por enquanto todos são User
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Obtém informações do usuário
    /// </summary>
    public async Task<PerfilUsuarioResponse?> ObterInfoUsuarioAsync(Guid usuarioId)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null) return null;

        return new PerfilUsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            AvatarUrl = usuario.AvatarUrl,
            DataCriacao = usuario.DataCriacao,
            DataUltimoLogin = usuario.DataUltimoLogin,
            Ativo = usuario.Ativo
        };
    }

    /// <summary>
    /// Processa recuperação de senha
    /// </summary>
    public async Task<bool> ProcessarRecuperacaoSenhaAsync(RecuperarSenhaRequest request)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email);
        if (usuario == null) return false;

        // Simulação - em produção enviaria email real
        return true;
    }

    /// <summary>
    /// Valida token de recuperação
    /// </summary>
    public async Task<bool> ValidarTokenRecuperacaoAsync(ValidateTokenRequest request)
    {
        // Simulação - em produção validaria token real
        return !string.IsNullOrEmpty(request.Token);
    }

    /// <summary>
    /// Processa login com Google OAuth
    /// </summary>
    public async Task<Usuario> ProcessarLoginGoogleAsync(string googleId, string email, string nome, string? avatarUrl = null)
    {
        // Verifica se usuário já existe pelo provedor Google
        var usuarioExistente = await _usuarioRepository.ObterPorProvedorAsync("Google", googleId);
        
        if (usuarioExistente != null)
        {
            // Atualiza dados se necessário
            usuarioExistente.Nome = nome;
            usuarioExistente.AvatarUrl = avatarUrl;
            usuarioExistente.DataUltimoLogin = DateTime.UtcNow;
            
            await _usuarioRepository.AtualizarAsync(usuarioExistente);
            
            return usuarioExistente;
        }

        // Verifica se já existe usuário com mesmo email
        var usuarioPorEmail = await _usuarioRepository.ObterPorEmailAsync(email);
        if (usuarioPorEmail != null)
        {
            // Vincula conta Google ao usuário existente
            usuarioPorEmail.Provedor = "Google";
            usuarioPorEmail.ProvedorId = googleId;
            usuarioPorEmail.DataUltimoLogin = DateTime.UtcNow;
            
            await _usuarioRepository.AtualizarAsync(usuarioPorEmail);
            
            return usuarioPorEmail;
        }

        // Cria novo usuário
        var novoUsuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Email = email,
            Provedor = "Google",
            ProvedorId = googleId,
            AvatarUrl = avatarUrl,
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataUltimoLogin = DateTime.UtcNow
        };

        await _usuarioRepository.AdicionarAsync(novoUsuario);
        
        return novoUsuario;
    }

    /// <summary>
    /// Gera token JWT
    /// </summary>
    public string GerarTokenJWT(Usuario usuario)
    {
        return GerarTokenJwt(usuario);
    }

    /// <summary>
    /// Valida se um token JWT é válido
    /// </summary>
    public bool ValidarToken(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("Authentication:JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extrai claims de um token JWT
    /// </summary>
    public ClaimsPrincipal? ObterClaimsDoToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            var identity = new ClaimsIdentity(jsonToken.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Obtém informações do usuário atual baseado no token
    /// </summary>
    public async Task<object> ObterInformacoesUsuarioAsync(string token)
    {
        var claims = ObterClaimsDoToken(token);
        if (claims == null) return new { Error = "Token inválido" };

        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return new { Error = "Usuário não encontrado no token" };

        var usuario = await _usuarioRepository.ObterPorIdAsync(Guid.Parse(userId));
        if (usuario == null) return new { Error = "Usuário não encontrado" };

        return new
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Role = usuario.Role,
            AvatarUrl = usuario.AvatarUrl
        };
    }

    /// <summary>
    /// Obtém usuário atual baseado no ID do usuário
    /// </summary>
    public async Task<object> ObterUsuarioAtualAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return new { Error = "ID de usuário inválido" };
        }

        var usuario = await _usuarioRepository.ObterPorIdAsync(userGuid);
        if (usuario == null) return new { Error = "Usuário não encontrado" };

        return new
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Role = usuario.Role,
            AvatarUrl = usuario.AvatarUrl
        };
    }

    /// <summary>
    /// Obtém claims do token JWT (versão async para interface)
    /// </summary>
    public async Task<object> ObterClaimsDoTokenAsync(string token)
    {
        var claims = ObterClaimsDoToken(token);
        if (claims == null) return new { Error = "Token inválido" };

        return claims.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
    }

    /// <summary>
    /// Recupera senha do usuário
    /// </summary>
    public async Task<object> RecuperarSenhaAsync(string email)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
        if (usuario == null) return new { Error = "Usuário não encontrado" };

        // Simulação - em produção enviaria email real
        return new { Message = "Email de recuperação enviado", Success = true };
    }
}
