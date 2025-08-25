using TechSub.Dominio.Entidades;

namespace TechSub.Dominio.Interfaces.Repositories;

/// <summary>
/// Interface para repositório de usuários
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>
    /// Obtém todos os usuários
    /// </summary>
    Task<IEnumerable<Usuario>> ObterTodosAsync();

    /// <summary>
    /// Obtém usuário por ID
    /// </summary>
    Task<Usuario?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Obtém usuário por email
    /// </summary>
    Task<Usuario?> ObterPorEmailAsync(string email);


    /// <summary>
    /// Adiciona novo usuário
    /// </summary>
    Task AdicionarAsync(Usuario usuario);

    /// <summary>
    /// Atualiza usuário existente
    /// </summary>
    Task AtualizarAsync(Usuario usuario);

    /// <summary>
    /// Remove usuário
    /// </summary>
    Task RemoverAsync(Guid id);

    /// <summary>
    /// Verifica se email já existe
    /// </summary>
    Task<bool> EmailExisteAsync(string email);

    /// <summary>
    /// Obtém usuário por provedor externo
    /// </summary>
    Task<Usuario?> ObterPorProvedorAsync(string provedor, string provedorId);
}
