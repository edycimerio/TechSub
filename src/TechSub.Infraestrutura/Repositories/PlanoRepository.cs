using Microsoft.EntityFrameworkCore;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Infraestrutura.Data;

namespace TechSub.Infraestrutura.Repositories;

/// <summary>
/// Implementação do repositório de planos
/// </summary>
public class PlanoRepository : IPlanoRepository
{
    private readonly TechSubDbContext _context;

    public PlanoRepository(TechSubDbContext context)
    {
        _context = context;
    }

    public async Task<Plano?> ObterPorIdAsync(Guid id)
    {
        return await _context.Planos
            .Include(p => p.Assinaturas)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Plano?> ObterPorNomeAsync(string nome)
    {
        return await _context.Planos
            .FirstOrDefaultAsync(p => p.Nome.ToLower() == nome.ToLower());
    }

    public async Task<IEnumerable<Plano>> ObterAtivosAsync()
    {
        return await _context.Planos
            .Where(p => p.Ativo)
            .OrderBy(p => p.Ordem)
            .ToListAsync();
    }

    public async Task<IEnumerable<Plano>> ObterTodosAsync()
    {
        return await _context.Planos
            .OrderBy(p => p.Ordem)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Plano plano)
    {
        _context.Planos.Add(plano);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Plano plano)
    {
        _context.Planos.Update(plano);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Guid id)
    {
        var plano = await _context.Planos.FindAsync(id);
        if (plano != null)
        {
            _context.Planos.Remove(plano);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> NomeExisteAsync(string nome)
    {
        return await _context.Planos
            .AnyAsync(p => p.Nome == nome);
    }
}
