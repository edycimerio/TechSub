using Microsoft.EntityFrameworkCore;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Enums;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Infraestrutura.Data;

namespace TechSub.Infraestrutura.Repositories;

/// <summary>
/// Implementação do repositório de assinaturas
/// </summary>
public class AssinaturaRepository : IAssinaturaRepository
{
    private readonly TechSubDbContext _context;

    public AssinaturaRepository(TechSubDbContext context)
    {
        _context = context;
    }

    public async Task<Assinatura?> ObterPorIdAsync(Guid id)
    {
        return await _context.Assinaturas
            .Include(a => a.Usuario)
            .Include(a => a.Plano)
            .Include(a => a.Pagamentos)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Assinatura>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        return await _context.Assinaturas
            .Include(a => a.Plano)
            .Include(a => a.Pagamentos)
            .Where(a => a.UsuarioId == usuarioId)
            .OrderByDescending(a => a.DataInicio)
            .ToListAsync();
    }

    public async Task<Assinatura?> ObterAtivaDoUsuarioAsync(Guid usuarioId)
    {
        return await _context.Assinaturas
            .Include(a => a.Plano)
            .Include(a => a.Pagamentos)
            .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId && a.Status == StatusAssinatura.Ativa);
    }

    public async Task<IEnumerable<Assinatura>> ObterPorStatusAsync(StatusAssinatura status)
    {
        return await _context.Assinaturas
            .Include(a => a.Usuario)
            .Include(a => a.Plano)
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.DataInicio)
            .ToListAsync();
    }

    public async Task<IEnumerable<Assinatura>> ObterVencendoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _context.Assinaturas
            .Include(a => a.Usuario)
            .Include(a => a.Plano)
            .Where(a => a.Status == StatusAssinatura.Ativa && 
                       a.DataProximaCobranca >= dataInicio && 
                       a.DataProximaCobranca <= dataFim)
            .OrderBy(a => a.DataProximaCobranca)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Assinatura assinatura)
    {
        _context.Assinaturas.Add(assinatura);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Assinatura assinatura)
    {
        _context.Assinaturas.Update(assinatura);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Guid id)
    {
        var assinatura = await _context.Assinaturas.FindAsync(id);
        if (assinatura != null)
        {
            _context.Assinaturas.Remove(assinatura);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<string, int>> ObterUsuariosAtivosPorPlanoAsync()
    {
        var resultado = await _context.Assinaturas
            .Include(a => a.Plano)
            .Where(a => a.Status == StatusAssinatura.Ativa)
            .GroupBy(a => a.Plano.Nome)
            .Select(g => new { Plano = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Plano, x => x.Count);

        return resultado;
    }

    public async Task<decimal> CalcularMrrAsync(int mes, int ano)
    {
        var assinaturasAtivas = await _context.Assinaturas
            .Include(a => a.Plano)
            .Where(a => a.Status == StatusAssinatura.Ativa &&
                       a.DataInicio.Year <= ano &&
                       (a.DataTermino == null || a.DataTermino.Value.Year >= ano) &&
                       a.DataInicio.Month <= mes &&
                       (a.DataTermino == null || a.DataTermino.Value.Month >= mes))
            .ToListAsync();

        return assinaturasAtivas.Sum(a => 
            a.Periodicidade == PeriodicidadeCobranca.Mensal 
                ? a.Valor 
                : a.Valor / 12); // Converter anual para mensal
    }

    public async Task<decimal> CalcularMRRAsync()
    {
        var hoje = DateTime.UtcNow;
        return await CalcularMrrAsync(hoje.Month, hoje.Year);
    }

    public async Task<IEnumerable<Assinatura>> ObterTodasAsync()
    {
        return await _context.Assinaturas
            .Include(a => a.Usuario)
            .Include(a => a.Plano)
            .OrderByDescending(a => a.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Assinatura>> ObterTodosAsync()
    {
        return await ObterTodasAsync();
    }

    public async Task<Assinatura?> ObterAtivaPorUsuarioAsync(Guid usuarioId)
    {
        return await ObterAtivaAsync(usuarioId);
    }

    public async Task<IEnumerable<Assinatura>> ObterExpiradasAsync()
    {
        return await _context.Assinaturas
            .Include(a => a.Usuario)
            .Include(a => a.Plano)
            .Where(a => a.Status == StatusAssinatura.Ativa && a.DataTermino <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<IEnumerable<Assinatura>> ObterParaRenovacaoAsync()
    {
        var proximaCobranca = DateTime.UtcNow.AddDays(3);
        return await _context.Assinaturas
            .Include(a => a.Usuario)
            .Include(a => a.Plano)
            .Where(a => a.Status == StatusAssinatura.Ativa && a.DataProximaCobranca <= proximaCobranca)
            .ToListAsync();
    }

    public async Task<IEnumerable<Assinatura>> ObterParaCancelamentoAsync()
    {
        return await _context.Assinaturas
            .Include(a => a.Usuario)
            .Include(a => a.Plano)
            .Where(a => a.Status == StatusAssinatura.CancelamentoPendente)
            .ToListAsync();
    }

    public async Task<object> ObterRelatorioAsync()
    {
        var hoje = DateTime.UtcNow;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        
        return new
        {
            TotalAssinaturas = await _context.Assinaturas.CountAsync(),
            AssinaturasAtivas = await _context.Assinaturas.CountAsync(a => a.Status == StatusAssinatura.Ativa),
            AssinaturasCanceladas = await _context.Assinaturas.CountAsync(a => a.Status == StatusAssinatura.Cancelada),
            NovasAssinaturasMes = await _context.Assinaturas.CountAsync(a => a.DataInicio >= inicioMes),
            MRR = await CalcularMRRAsync()
        };
    }

    public async Task<Assinatura?> ObterAtivaAsync(Guid usuarioId)
    {
        return await _context.Assinaturas
            .Include(a => a.Plano)
            .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId && a.Status == StatusAssinatura.Ativa);
    }

    public async Task<IEnumerable<Assinatura>> ObterRelatorioAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _context.Assinaturas
            .Include(a => a.Usuario)
            .Include(a => a.Plano)
            .Where(a => a.DataCriacao >= dataInicio && a.DataCriacao <= dataFim)
            .OrderByDescending(a => a.DataCriacao)
            .ToListAsync();
    }
}
