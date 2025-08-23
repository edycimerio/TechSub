using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces;

namespace TechSub.Aplicacao.Services;

/// <summary>
/// Serviço para gerenciamento de autenticação e tokens JWT
/// </summary>
public class AuthService
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
    /// Processa login via Google OAuth
    /// </summary>
    public async Task<(Usuario usuario, string token)> ProcessarLoginGoogleAsync(string googleId, string email, string nome, string? avatarUrl = null)
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
            
            var token = GerarTokenJwt(usuarioExistente);
            return (usuarioExistente, token);
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
            
            var token = GerarTokenJwt(usuarioPorEmail);
            return (usuarioPorEmail, token);
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
        
        var novoToken = GerarTokenJwt(novoUsuario);
        return (novoUsuario, novoToken);
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
}
