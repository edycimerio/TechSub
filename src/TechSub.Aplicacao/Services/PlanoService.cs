using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Aplicacao.Interfaces;

namespace TechSub.Aplicacao.Services;

/// <summary>
/// Serviço para gerenciar planos de assinatura
/// </summary>
public class PlanoService : IPlanoService
{
    private readonly IPlanoRepository _planoRepository;

    public PlanoService(IPlanoRepository planoRepository)
    {
        _planoRepository = planoRepository;
    }

    /// <summary>
    /// Obtém todos os planos ativos para exibição pública
    /// </summary>
    public async Task<IEnumerable<PlanoResponse>> ObterPlanosAtivosAsync()
    {
        var planos = await _planoRepository.ObterAtivosAsync();
        
        return planos.Select(p => new PlanoResponse
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            PrecoMensal = p.PrecoMensal,
            PrecoAnual = p.PrecoAnual,
            TemTrial = p.TemTrial,
            DiasTrialGratuito = p.DiasTrialGratuito,
            Recursos = p.Recursos,
            Ativo = p.Ativo,
            Ordem = p.Ordem,
            DataCriacao = p.DataCriacao
        });
    }

    /// <summary>
    /// Obtém todos os planos (admin)
    /// </summary>
    public async Task<IEnumerable<PlanoResponse>> ObterTodosAsync(string? userRole)
    {
        // Validação de autorização
        if (userRole != "Admin")
            throw new UnauthorizedAccessException("Acesso negado. Apenas administradores podem listar todos os planos.");
            
        var planos = await _planoRepository.ObterTodosAsync();
        
        return planos.Select(p => new PlanoResponse
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            PrecoMensal = p.PrecoMensal,
            PrecoAnual = p.PrecoAnual,
            TemTrial = p.TemTrial,
            DiasTrialGratuito = p.DiasTrialGratuito,
            Recursos = p.Recursos,
            Ativo = p.Ativo,
            Ordem = p.Ordem,
            DataCriacao = p.DataCriacao
        });
    }

    /// <summary>
    /// Obtém plano por ID
    /// </summary>
    public async Task<PlanoResponse?> ObterPorIdAsync(Guid id)
    {
        var plano = await _planoRepository.ObterPorIdAsync(id);
        if (plano == null) return null;

        return new PlanoResponse
        {
            Id = plano.Id,
            Nome = plano.Nome,
            Descricao = plano.Descricao,
            PrecoMensal = plano.PrecoMensal,
            PrecoAnual = plano.PrecoAnual,
            TemTrial = plano.TemTrial,
            DiasTrialGratuito = plano.DiasTrialGratuito,
            Recursos = plano.Recursos,
            Ativo = plano.Ativo,
            Ordem = plano.Ordem,
            DataCriacao = plano.DataCriacao
        };
    }

    /// <summary>
    /// Cria novo plano
    /// </summary>
    public async Task<PlanoResponse> CriarAsync(CriarPlanoRequest dto, string? userRole)
    {
        // Validação de autorização
        if (userRole != "Admin")
            throw new UnauthorizedAccessException("Acesso negado. Apenas administradores podem criar planos.");
            
        // Validação de nome duplicado
        if (await _planoRepository.NomeExisteAsync(dto.Nome))
            throw new InvalidOperationException("Já existe um plano com este nome");

        var plano = new Plano
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            PrecoMensal = dto.PrecoMensal,
            PrecoAnual = dto.PrecoAnual,
            TemTrial = dto.TemTrial,
            DiasTrialGratuito = dto.DiasTrialGratuito,
            Recursos = dto.Recursos,
            Ativo = true,
            Ordem = dto.Ordem,
            DataCriacao = DateTime.UtcNow
        };

        await _planoRepository.AdicionarAsync(plano);

        return new PlanoResponse
        {
            Id = plano.Id,
            Nome = plano.Nome,
            Descricao = plano.Descricao,
            PrecoMensal = plano.PrecoMensal,
            PrecoAnual = plano.PrecoAnual,
            TemTrial = plano.TemTrial,
            DiasTrialGratuito = plano.DiasTrialGratuito,
            Recursos = plano.Recursos,
            Ativo = plano.Ativo,
            Ordem = plano.Ordem,
            DataCriacao = plano.DataCriacao
        };
    }

    /// <summary>
    /// Atualiza plano existente
    /// </summary>
    public async Task<PlanoResponse?> AtualizarAsync(Guid id, AtualizarPlanoRequest dto, string? userRole)
    {
        // Validação de autorização
        if (userRole != "Admin")
            throw new UnauthorizedAccessException("Acesso negado. Apenas administradores podem atualizar planos.");
            
        var plano = await _planoRepository.ObterPorIdAsync(id);
        if (plano == null) return null;

        // Validação de nome duplicado (exceto o próprio plano)
        var planoExistente = await _planoRepository.ObterPorNomeAsync(dto.Nome);
        if (planoExistente != null && planoExistente.Id != id)
            throw new InvalidOperationException("Já existe um plano com este nome");

        plano.Nome = dto.Nome;
        plano.Descricao = dto.Descricao;
        plano.PrecoMensal = dto.PrecoMensal;
        plano.PrecoAnual = dto.PrecoAnual;
        plano.TemTrial = dto.TemTrial;
        plano.DiasTrialGratuito = dto.DiasTrialGratuito;
        plano.Recursos = dto.Recursos;
        plano.Ativo = dto.Ativo;
        plano.Ordem = dto.Ordem;

        await _planoRepository.AtualizarAsync(plano);

        return new PlanoResponse
        {
            Id = plano.Id,
            Nome = plano.Nome,
            Descricao = plano.Descricao,
            PrecoMensal = plano.PrecoMensal,
            PrecoAnual = plano.PrecoAnual,
            TemTrial = plano.TemTrial,
            DiasTrialGratuito = plano.DiasTrialGratuito,
            Recursos = plano.Recursos,
            Ativo = plano.Ativo,
            Ordem = plano.Ordem,
            DataCriacao = plano.DataCriacao
        };
    }

    /// <summary>
    /// Remove plano
    /// </summary>
    public async Task<bool> RemoverAsync(Guid id, string? userRole)
    {
        // Validação de autorização
        if (userRole != "Admin")
            throw new UnauthorizedAccessException("Acesso negado. Apenas administradores podem remover planos.");
            
        var plano = await _planoRepository.ObterPorIdAsync(id);
        if (plano == null) return false;

        // Verifica se há assinaturas ativas para este plano
        if (plano.Assinaturas?.Any(a => a.Status == TechSub.Dominio.Enums.StatusAssinatura.Ativa) == true)
        {
            throw new InvalidOperationException("Não é possível remover um plano com assinaturas ativas");
        }

        await _planoRepository.RemoverAsync(id);
        return true;
    }
}
