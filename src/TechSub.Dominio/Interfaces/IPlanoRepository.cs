using TechSub.Dominio.Entidades;

namespace TechSub.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de planos
/// </summary>
public interface IPlanoRepository
{
    /// <summary>
    /// Obtém um plano por ID
    /// </summary>
    Task<Plano?> ObterPorIdAsync(Guid id);

    /// <summary>
    /// Obtém planos ativos ordenados
    /// </summary>
    Task<IEnumerable<Plano>> ObterAtivosAsync();

    /// <summary>
    /// Obtém plano por nome
    /// </summary>
    Task<Plano?> ObterPorNomeAsync(string nome);

    /// <summary>
    /// Obtém todos os planos ordenados por ordem de exibição
    /// </summary>
    Task<IEnumerable<Plano>> ObterTodosOrdenadosAsync();

    /// <summary>
    /// Adiciona um novo plano
    /// </summary>
    Task<Plano> AdicionarAsync(Plano plano);

    /// <summary>
    /// Atualiza um plano existente
    /// </summary>
    Task<Plano> AtualizarAsync(Plano plano);

    /// <summary>
    /// Remove um plano
    /// </summary>
    Task RemoverAsync(Guid id);

    /// <summary>
    /// Verifica se um nome de plano já existe
    /// </summary>
    Task<bool> NomeExisteAsync(string nome);
}
