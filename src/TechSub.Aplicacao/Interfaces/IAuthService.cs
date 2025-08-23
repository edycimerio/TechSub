using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;
using TechSub.Dominio.Entidades;

namespace TechSub.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de autenticação
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Processa login com Google OAuth
    /// </summary>
    Task<Usuario> ProcessarLoginGoogleAsync(string googleId, string email, string nome, string? avatarUrl = null);

    /// <summary>
    /// Gera token JWT
    /// </summary>
    string GerarTokenJWT(Usuario usuario);

    /// <summary>
    /// Valida token JWT
    /// </summary>
    bool ValidarToken(string token);

    /// <summary>
    /// Obtém informações do usuário atual baseado no token
    /// </summary>
    Task<object> ObterInformacoesUsuarioAsync(string token);

    /// <summary>
    /// Obtém usuário atual baseado no ID do usuário
    /// </summary>
    Task<object> ObterUsuarioAtualAsync(string userId);

    /// <summary>
    /// Obtém claims do token JWT
    /// </summary>
    Task<object> ObterClaimsDoTokenAsync(string token);

    /// <summary>
    /// Recupera senha do usuário
    /// </summary>
    Task<object> RecuperarSenhaAsync(string email);

    /// <summary>
    /// Processa recuperação de senha
    /// </summary>
    Task<bool> ProcessarRecuperacaoSenhaAsync(RecuperarSenhaRequest request);

    /// <summary>
    /// Valida token de recuperação
    /// </summary>
    Task<bool> ValidarTokenRecuperacaoAsync(ValidateTokenRequest request);
}
