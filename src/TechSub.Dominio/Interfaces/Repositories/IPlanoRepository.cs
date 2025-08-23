using TechSub.Dominio.Entidades;

namespace TechSub.Dominio.Interfaces.Repositories;

/// <summary>
/// Interface para repositório de planos
/// </summary>
public interface IPlanoRepository
{
    /// <summary>
    /// Obtém todos os planos
    /// </summary>
    Task<IEnumerable<Plano>> ObterTodosAsync();

    /// <summary>
    /// Obtém apenas planos ativos
    /// </summary>
    Task<IEnumerable<Plano>> ObterAtivosAsync();

    /// <summary>
    /// Obtém plano por ID
    /// </summary>
    Task<Plano?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Obtém plano por nome
    /// </summary>
    Task<Plano?> ObterPorNomeAsync(string nome);

    /// <summary>
    /// Adiciona novo plano
    /// </summary>
    Task AdicionarAsync(Plano plano);

    /// <summary>
    /// Atualiza plano existente
    /// </summary>
    Task AtualizarAsync(Plano plano);

    /// <summary>
    /// Remove plano
    /// </summary>
    Task RemoverAsync(Guid id);

    /// <summary>
    /// Verifica se nome já existe
    /// </summary>
    Task<bool> NomeExisteAsync(string nome);
}
