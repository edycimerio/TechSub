using Microsoft.EntityFrameworkCore;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Enums;
using TechSub.Dominio.Interfaces;
using TechSub.Infraestrutura.Data;

namespace TechSub.Infraestrutura.Repositories;

/// <summary>
/// Implementação do repositório de pagamentos
/// </summary>
public class PagamentoRepository : IPagamentoRepository
{
    private readonly TechSubDbContext _context;

    public PagamentoRepository(TechSubDbContext context)
    {
        _context = context;
    }

    public async Task<Pagamento?> ObterPorIdAsync(Guid id)
    {
        return await _context.Pagamentos
            .Include(p => p.Assinatura)
            .ThenInclude(a => a.Usuario)
            .Include(p => p.Assinatura)
            .ThenInclude(a => a.Plano)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Pagamento>> ObterPorAssinaturaAsync(Guid assinaturaId)
    {
        return await _context.Pagamentos
            .Where(p => p.AssinaturaId == assinaturaId)
            .OrderByDescending(p => p.DataProcessamento ?? p.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pagamento>> ObterPorStatusAsync(StatusPagamento status)
    {
        return await _context.Pagamentos
            .Include(p => p.Assinatura)
            .ThenInclude(a => a.Usuario)
            .Include(p => p.Assinatura)
            .ThenInclude(a => a.Plano)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.DataProcessamento ?? p.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pagamento>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _context.Pagamentos
            .Include(p => p.Assinatura)
            .ThenInclude(a => a.Usuario)
            .Include(p => p.Assinatura)
            .ThenInclude(a => a.Plano)
            .Where(p => (p.DataProcessamento ?? p.DataCriacao) >= dataInicio && 
                       (p.DataProcessamento ?? p.DataCriacao) <= dataFim)
            .OrderByDescending(p => p.DataProcessamento ?? p.DataCriacao)
            .ToListAsync();
    }

    public async Task<Pagamento> AdicionarAsync(Pagamento pagamento)
    {
        _context.Pagamentos.Add(pagamento);
        await _context.SaveChangesAsync();
        return pagamento;
    }

    public async Task<Pagamento> AtualizarAsync(Pagamento pagamento)
    {
        _context.Pagamentos.Update(pagamento);
        await _context.SaveChangesAsync();
        return pagamento;
    }

    public async Task RemoverAsync(Guid id)
    {
        var pagamento = await _context.Pagamentos.FindAsync(id);
        if (pagamento != null)
        {
            _context.Pagamentos.Remove(pagamento);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Pagamento?> ObterUltimoPagamentoAsync(Guid assinaturaId)
    {
        return await _context.Pagamentos
            .Where(p => p.AssinaturaId == assinaturaId)
            .OrderByDescending(p => p.DataProcessamento ?? p.DataCriacao)
            .FirstOrDefaultAsync();
    }
}
