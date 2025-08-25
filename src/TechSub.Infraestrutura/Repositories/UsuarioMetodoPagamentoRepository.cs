using Microsoft.EntityFrameworkCore;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Infraestrutura.Data;

namespace TechSub.Infraestrutura.Repositories;

/// <summary>
/// Repositório para métodos de pagamento do usuário
/// </summary>
public class UsuarioMetodoPagamentoRepository : IUsuarioMetodoPagamentoRepository
{
    private readonly TechSubDbContext _context;

    public UsuarioMetodoPagamentoRepository(TechSubDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Verifica se usuário tem método de pagamento válido
    /// </summary>
    public async Task<bool> TemMetodoPagamentoAsync(Guid usuarioId)
    {
        var metodo = await _context.UsuarioMetodoPagamento
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
            
        return metodo?.TemMetodoPagamento ?? false;
    }

    /// <summary>
    /// Define se usuário tem método de pagamento
    /// </summary>
    public async Task DefinirMetodoPagamentoAsync(Guid usuarioId, bool temMetodo)
    {
        var metodo = await _context.UsuarioMetodoPagamento
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        if (metodo == null)
        {
            metodo = new UsuarioMetodoPagamento
            {
                UsuarioId = usuarioId,
                TemMetodoPagamento = temMetodo
            };
            _context.UsuarioMetodoPagamento.Add(metodo);
        }
        else
        {
            metodo.TemMetodoPagamento = temMetodo;
        }

        await _context.SaveChangesAsync();
    }
}
