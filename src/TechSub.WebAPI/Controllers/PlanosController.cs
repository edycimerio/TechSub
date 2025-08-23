using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSub.Dominio.Interfaces;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para gerenciamento de planos de assinatura
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PlanosController : ControllerBase
{
    private readonly IPlanoRepository _planoRepository;

    public PlanosController(IPlanoRepository planoRepository)
    {
        _planoRepository = planoRepository;
    }

    /// <summary>
    /// Obtém todos os planos ativos (público)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterPlanosAtivos()
    {
        try
        {
            var planos = await _planoRepository.ObterAtivosAsync();
            
            var planosDto = planos.Select(p => new PlanoPublicoDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao,
                PrecoMensal = p.PrecoMensal,
                PrecoAnual = p.PrecoAnual,
                TemTrial = p.TemTrial,
                DiasTrialGratuito = p.DiasTrialGratuito,
                Recursos = p.Recursos,
                Ordem = p.Ordem
            });

            return Ok(planosDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um plano específico por ID (público)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            var plano = await _planoRepository.ObterPorIdAsync(id);
            
            if (plano == null || !plano.Ativo)
            {
                return NotFound(new { message = "Plano não encontrado" });
            }

            var planoDto = new PlanoPublicoDto
            {
                Id = plano.Id,
                Nome = plano.Nome,
                Descricao = plano.Descricao,
                PrecoMensal = plano.PrecoMensal,
                PrecoAnual = plano.PrecoAnual,
                TemTrial = plano.TemTrial,
                DiasTrialGratuito = plano.DiasTrialGratuito,
                Recursos = plano.Recursos,
                Ordem = plano.Ordem
            };

            return Ok(planoDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém todos os planos incluindo inativos (apenas admins)
    /// </summary>
    [HttpGet("admin/todos")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ObterTodosPlanos()
    {
        try
        {
            var planos = await _planoRepository.ObterTodosOrdenadosAsync();
            
            var planosDto = planos.Select(p => new PlanoAdminDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao,
                PrecoMensal = p.PrecoMensal,
                PrecoAnual = p.PrecoAnual,
                TemTrial = p.TemTrial,
                DiasTrialGratuito = p.DiasTrialGratuito,
                Recursos = p.Recursos,
                Ordem = p.Ordem,
                Ativo = p.Ativo,
                DataCriacao = p.DataCriacao,
                DataAtualizacao = p.DataAtualizacao
            });

            return Ok(planosDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Cria um novo plano (apenas admins)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CriarPlano([FromBody] CriarPlanoDto dto)
    {
        try
        {
            // Verifica se nome já existe
            var planoExistente = await _planoRepository.ObterPorNomeAsync(dto.Nome);
            if (planoExistente != null)
            {
                return BadRequest(new { message = "Já existe um plano com este nome" });
            }

            var novoPlano = new TechSub.Dominio.Entidades.Plano
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                PrecoMensal = dto.PrecoMensal,
                PrecoAnual = dto.PrecoAnual,
                TemTrial = dto.TemTrial,
                DiasTrialGratuito = dto.DiasTrialGratuito,
                Recursos = dto.Recursos,
                Ordem = dto.Ordem,
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            await _planoRepository.AdicionarAsync(novoPlano);

            var planoDto = new PlanoAdminDto
            {
                Id = novoPlano.Id,
                Nome = novoPlano.Nome,
                Descricao = novoPlano.Descricao,
                PrecoMensal = novoPlano.PrecoMensal,
                PrecoAnual = novoPlano.PrecoAnual,
                TemTrial = novoPlano.TemTrial,
                DiasTrialGratuito = novoPlano.DiasTrialGratuito,
                Recursos = novoPlano.Recursos,
                Ordem = novoPlano.Ordem,
                Ativo = novoPlano.Ativo,
                DataCriacao = novoPlano.DataCriacao,
                DataAtualizacao = novoPlano.DataAtualizacao
            };

            return CreatedAtAction(nameof(ObterPorId), new { id = novoPlano.Id }, planoDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um plano existente (apenas admins)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AtualizarPlano(Guid id, [FromBody] AtualizarPlanoDto dto)
    {
        try
        {
            var plano = await _planoRepository.ObterPorIdAsync(id);
            
            if (plano == null)
            {
                return NotFound(new { message = "Plano não encontrado" });
            }

            // Verifica se nome já existe (se foi alterado)
            if (dto.Nome != plano.Nome)
            {
                var nomeExiste = await _planoRepository.NomeExisteAsync(dto.Nome);
                if (nomeExiste)
                {
                    return BadRequest(new { message = "Já existe um plano com este nome" });
                }
            }

            // Atualiza dados
            plano.Nome = dto.Nome;
            plano.Descricao = dto.Descricao;
            plano.PrecoMensal = dto.PrecoMensal;
            plano.PrecoAnual = dto.PrecoAnual;
            plano.TemTrial = dto.TemTrial;
            plano.DiasTrialGratuito = dto.DiasTrialGratuito;
            plano.Recursos = dto.Recursos;
            plano.Ordem = dto.Ordem;
            plano.Ativo = dto.Ativo;
            plano.DataAtualizacao = DateTime.UtcNow;

            await _planoRepository.AtualizarAsync(plano);

            var planoDto = new PlanoAdminDto
            {
                Id = plano.Id,
                Nome = plano.Nome,
                Descricao = plano.Descricao,
                PrecoMensal = plano.PrecoMensal,
                PrecoAnual = plano.PrecoAnual,
                TemTrial = plano.TemTrial,
                DiasTrialGratuito = plano.DiasTrialGratuito,
                Recursos = plano.Recursos,
                Ordem = plano.Ordem,
                Ativo = plano.Ativo,
                DataCriacao = plano.DataCriacao,
                DataAtualizacao = plano.DataAtualizacao
            };

            return Ok(planoDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Remove um plano (apenas admins)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoverPlano(Guid id)
    {
        try
        {
            var plano = await _planoRepository.ObterPorIdAsync(id);
            
            if (plano == null)
            {
                return NotFound(new { message = "Plano não encontrado" });
            }

            // Verifica se há assinaturas ativas para este plano
            if (plano.Assinaturas?.Any(a => a.Status == TechSub.Dominio.Enums.StatusAssinatura.Ativa) == true)
            {
                return BadRequest(new { message = "Não é possível remover um plano com assinaturas ativas" });
            }

            await _planoRepository.RemoverAsync(id);

            return Ok(new { message = "Plano removido com sucesso" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }
}

/// <summary>
/// DTO para exibição pública de planos
/// </summary>
public class PlanoPublicoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PrecoMensal { get; set; }
    public decimal PrecoAnual { get; set; }
    public bool TemTrial { get; set; }
    public int DiasTrialGratuito { get; set; }
    public string? Recursos { get; set; }
    public int Ordem { get; set; }
}

/// <summary>
/// DTO para administração de planos
/// </summary>
public class PlanoAdminDto : PlanoPublicoDto
{
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para criação de plano
/// </summary>
public class CriarPlanoDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PrecoMensal { get; set; }
    public decimal PrecoAnual { get; set; }
    public bool TemTrial { get; set; }
    public int DiasTrialGratuito { get; set; } = 7;
    public string? Recursos { get; set; }
    public int Ordem { get; set; }
}

/// <summary>
/// DTO para atualização de plano
/// </summary>
public class AtualizarPlanoDto : CriarPlanoDto
{
    public bool Ativo { get; set; } = true;
}
