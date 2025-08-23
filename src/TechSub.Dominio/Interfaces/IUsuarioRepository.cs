using TechSub.Dominio.Entidades;

namespace TechSub.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de usuários
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    Task<Usuario?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Obtém um usuário por email
    /// </summary>
    Task<Usuario?> ObterPorEmailAsync(string email);

    /// <summary>
    /// Obtém um usuário por provedor externo
    /// </summary>
    Task<Usuario?> ObterPorProvedorAsync(string provedor, string provedorId);

    /// <summary>
    /// Obtém todos os usuários ativos
    /// </summary>
    Task<IEnumerable<Usuario>> ObterTodosAtivosAsync();

    /// <summary>
    /// Adiciona um novo usuário
    /// </summary>
    Task<Usuario> AdicionarAsync(Usuario usuario);

    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    Task<Usuario> AtualizarAsync(Usuario usuario);

    /// <summary>
    /// Remove um usuário
    /// </summary>
    Task RemoverAsync(Guid id);

    /// <summary>
    /// Verifica se um email já está em uso
    /// </summary>
    Task<bool> EmailExisteAsync(string email);
}
