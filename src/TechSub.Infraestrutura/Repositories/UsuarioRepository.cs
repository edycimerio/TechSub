using Microsoft.EntityFrameworkCore;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Infraestrutura.Data;

namespace TechSub.Infraestrutura.Repositories;

/// <summary>
/// Implementação do repositório de usuários
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly TechSubDbContext _context;

    public UsuarioRepository(TechSubDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id)
    {
        return await _context.Usuarios
            .Include(u => u.Assinaturas)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .Include(u => u.Assinaturas)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Usuario?> ObterPorProvedorAsync(string provedor, string provedorId)
    {
        return await _context.Usuarios
            .Include(u => u.Assinaturas)
            .FirstOrDefaultAsync(u => u.Provedor == provedor && u.ProvedorId == provedorId);
    }

    public async Task<Usuario?> ObterPorGoogleIdAsync(string googleId)
    {
        return await _context.Usuarios
            .Include(u => u.Assinaturas)
            .FirstOrDefaultAsync(u => u.GoogleId == googleId);
    }

    public async Task<IEnumerable<Usuario>> ObterTodosAsync()
    {
        return await _context.Usuarios
            .Include(u => u.Assinaturas)
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Email == email);
    }
}
