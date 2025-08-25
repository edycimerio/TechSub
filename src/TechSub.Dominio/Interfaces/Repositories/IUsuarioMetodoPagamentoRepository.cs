namespace TechSub.Dominio.Interfaces.Repositories;

/// <summary>
/// Interface para repositório de métodos de pagamento do usuário
/// </summary>
public interface IUsuarioMetodoPagamentoRepository
{
    /// <summary>
    /// Verifica se usuário tem método de pagamento válido
    /// </summary>
    Task<bool> TemMetodoPagamentoAsync(Guid usuarioId);
    
    /// <summary>
    /// Define se usuário tem método de pagamento
    /// </summary>
    Task DefinirMetodoPagamentoAsync(Guid usuarioId, bool temMetodo);
}
