using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Enums;
using TechSub.Dominio.Interfaces;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para gerenciar assinaturas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssinaturasController : ControllerBase
{
    private readonly IAssinaturaRepository _assinaturaRepository;
    private readonly IPlanoRepository _planoRepository;
    private readonly IPagamentoRepository _pagamentoRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public AssinaturasController(
        IAssinaturaRepository assinaturaRepository,
        IPlanoRepository planoRepository,
        IPagamentoRepository pagamentoRepository,
        IUsuarioRepository usuarioRepository)
    {
        _assinaturaRepository = assinaturaRepository;
        _planoRepository = planoRepository;
        _pagamentoRepository = pagamentoRepository;
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Obter assinaturas do usuário logado
    /// </summary>
    [HttpGet("minhas")]
    public async Task<ActionResult<IEnumerable<AssinaturaResponseDto>>> ObterMinhasAssinaturas()
    {
        var usuarioId = ObterUsuarioId();
        var assinaturas = await _assinaturaRepository.ObterPorUsuarioAsync(usuarioId);

        var response = assinaturas.Select(a => new AssinaturaResponseDto
        {
            Id = a.Id,
            PlanoNome = a.Plano.Nome,
            PlanoValor = a.Plano.PrecoMensal,
            Status = a.Status.ToString(),
            DataInicio = a.DataInicio,
            DataFim = a.DataTermino,
            DataProximaCobranca = a.DataProximaCobranca,
            Ativa = a.Status == StatusAssinatura.Ativa
        });

        return Ok(response);
    }

    /// <summary>
    /// Obter assinatura ativa do usuário
    /// </summary>
    [HttpGet("ativa")]
    public async Task<ActionResult<AssinaturaResponseDto>> ObterAssinaturaAtiva()
    {
        var usuarioId = ObterUsuarioId();
        var assinatura = await _assinaturaRepository.ObterAtivaAsync(usuarioId);

        if (assinatura == null)
            return NotFound("Nenhuma assinatura ativa encontrada");

        var response = new AssinaturaResponseDto
        {
            Id = assinatura.Id,
            PlanoNome = assinatura.Plano.Nome,
            PlanoValor = assinatura.Plano.PrecoMensal,
            Status = assinatura.Status.ToString(),
            DataInicio = assinatura.DataInicio,
            DataFim = assinatura.DataTermino,
            DataProximaCobranca = assinatura.DataProximaCobranca,
            Ativa = assinatura.Status == StatusAssinatura.Ativa
        };

        return Ok(response);
    }

    /// <summary>
    /// Criar nova assinatura
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AssinaturaResponseDto>> CriarAssinatura([FromBody] CriarAssinaturaDto request)
    {
        var usuarioId = ObterUsuarioId();

        // Verificar se já existe assinatura ativa
        var assinaturaExistente = await _assinaturaRepository.ObterAtivaAsync(usuarioId);
        if (assinaturaExistente != null)
            return BadRequest("Usuário já possui uma assinatura ativa");

        // Verificar se o plano existe e está ativo
        var plano = await _planoRepository.ObterPorIdAsync(request.PlanoId);
        if (plano == null || !plano.Ativo)
            return BadRequest("Plano não encontrado ou inativo");

        // Verificar se é o primeiro plano do usuário para trial
        var jaTeveAssinatura = await _assinaturaRepository.ObterPorUsuarioAsync(usuarioId);
        var primeiraAssinatura = !jaTeveAssinatura.Any();
        var temTrial = primeiraAssinatura && plano.TemTrial;

        // Criar nova assinatura
        var assinatura = new Assinatura
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            PlanoId = request.PlanoId,
            Status = temTrial ? StatusAssinatura.Trial : StatusAssinatura.Ativa,
            DataInicio = DateTime.UtcNow,
            DataTermino = null,
            DataProximaCobranca = temTrial 
                ? DateTime.UtcNow.AddDays(plano.DiasTrialGratuito)
                : (request.TipoCobranca == "mensal"
                    ? DateTime.UtcNow.AddMonths(1)
                    : DateTime.UtcNow.AddYears(1)),
            Periodicidade = request.TipoCobranca == "mensal" 
                ? PeriodicidadeCobranca.Mensal 
                : PeriodicidadeCobranca.Anual,
            Valor = request.TipoCobranca == "mensal" ? plano.PrecoMensal : plano.PrecoAnual,
            EmTrial = temTrial,
            DataTerminoTrial = temTrial ? DateTime.UtcNow.AddDays(plano.DiasTrialGratuito) : null,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        await _assinaturaRepository.AdicionarAsync(assinatura);

        // Criar primeiro pagamento apenas se não for trial
        if (!temTrial)
        {
            var pagamento = new Pagamento
            {
                Id = Guid.NewGuid(),
                AssinaturaId = assinatura.Id,
                Valor = assinatura.Valor,
                Status = StatusPagamento.Pendente,
                DataVencimento = DateTime.UtcNow.AddDays(7), // 7 dias para pagamento
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            await _pagamentoRepository.AdicionarAsync(pagamento);
        }

        var response = new AssinaturaResponseDto
        {
            Id = assinatura.Id,
            PlanoNome = plano.Nome,
            PlanoValor = plano.PrecoMensal,
            Status = assinatura.Status.ToString(),
            DataInicio = assinatura.DataInicio,
            DataFim = assinatura.DataTermino,
            DataProximaCobranca = assinatura.DataProximaCobranca,
            Ativa = assinatura.Status == StatusAssinatura.Ativa
        };

        return CreatedAtAction(nameof(ObterAssinaturaAtiva), response);
    }

    /// <summary>
    /// Cancelar assinatura
    /// </summary>
    [HttpPost("{id}/cancelar")]
    public async Task<ActionResult> CancelarAssinatura(Guid id)
    {
        var usuarioId = ObterUsuarioId();
        var assinatura = await _assinaturaRepository.ObterPorIdAsync(id);

        if (assinatura == null)
            return NotFound("Assinatura não encontrada");

        if (assinatura.UsuarioId != usuarioId)
            return Forbid("Acesso negado");

        if (assinatura.Status != StatusAssinatura.Ativa)
            return BadRequest("Assinatura já está inativa");

        // Cancelar assinatura no fim do ciclo
        assinatura.Status = StatusAssinatura.Cancelada;
        // Definir término para o fim do ciclo pago atual
        assinatura.DataTermino = assinatura.DataProximaCobranca ?? DateTime.UtcNow.AddMonths(1);
        assinatura.DataAtualizacao = DateTime.UtcNow;

        await _assinaturaRepository.AtualizarAsync(assinatura);

        return Ok(new { message = "Assinatura cancelada com sucesso" });
    }

    /// <summary>
    /// Renovar assinatura
    /// </summary>
    [HttpPost("{id}/renovar")]
    public async Task<ActionResult> RenovarAssinatura(Guid id)
    {
        var usuarioId = ObterUsuarioId();
        var assinatura = await _assinaturaRepository.ObterPorIdAsync(id);

        if (assinatura == null)
            return NotFound("Assinatura não encontrada");

        if (assinatura.UsuarioId != usuarioId)
            return Forbid("Acesso negado");

        if (assinatura.Status != StatusAssinatura.Expirada)
            return BadRequest("Apenas assinaturas expiradas podem ser renovadas");

        // Renovar assinatura
        assinatura.Status = StatusAssinatura.Ativa;
        assinatura.DataInicio = DateTime.UtcNow;
        assinatura.DataTermino = null;
        assinatura.DataProximaCobranca = DateTime.UtcNow.AddMonths(1);
        assinatura.DataAtualizacao = DateTime.UtcNow;

        await _assinaturaRepository.AtualizarAsync(assinatura);

        // Criar novo pagamento para renovação
        var pagamento = new Pagamento
        {
            Id = Guid.NewGuid(),
            AssinaturaId = assinatura.Id,
            Valor = assinatura.Valor,
            Status = StatusPagamento.Pendente,
            DataVencimento = DateTime.UtcNow.AddDays(7),
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        await _pagamentoRepository.AdicionarAsync(pagamento);

        return Ok(new { message = "Assinatura renovada com sucesso" });
    }

    /// <summary>
    /// Listar todas as assinaturas (apenas Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<AssinaturaAdminDto>>> ListarTodasAssinaturas()
    {
        var assinaturas = await _assinaturaRepository.ObterTodasAsync();

        var response = assinaturas.Select(a => new AssinaturaAdminDto
        {
            Id = a.Id,
            UsuarioNome = a.Usuario.Nome,
            UsuarioEmail = a.Usuario.Email,
            PlanoNome = a.Plano.Nome,
            PlanoValor = a.Plano.PrecoMensal,
            Status = a.Status.ToString(),
            DataInicio = a.DataInicio,
            DataFim = a.DataTermino,
            DataProximaCobranca = a.DataProximaCobranca,
            Ativa = a.Status == StatusAssinatura.Ativa,
            DataCriacao = a.DataCriacao
        });

        return Ok(response);
    }

    /// <summary>
    /// Obter relatório de MRR (apenas Admin)
    /// </summary>
    [HttpGet("relatorio/mrr")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<decimal>> ObterMRR()
    {
        var mrr = await _assinaturaRepository.CalcularMRRAsync();
        return Ok(new { mrr = mrr, periodo = DateTime.UtcNow.ToString("yyyy-MM") });
    }

    private Guid ObterUsuarioId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Token inválido");
        }
        return userId;
    }
}

/// <summary>
/// DTO para resposta de assinatura
/// </summary>
public class AssinaturaResponseDto
{
    public Guid Id { get; set; }
    public string PlanoNome { get; set; } = string.Empty;
    public decimal PlanoValor { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public DateTime? DataProximaCobranca { get; set; }
    public bool Ativa { get; set; }
}

/// <summary>
/// DTO para criar assinatura
/// </summary>
public class CriarAssinaturaDto
{
    public Guid PlanoId { get; set; }
    public string TipoCobranca { get; set; } = "mensal"; // mensal ou anual
}

/// <summary>
/// DTO para visualização admin de assinaturas
/// </summary>
public class AssinaturaAdminDto
{
    public Guid Id { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string UsuarioEmail { get; set; } = string.Empty;
    public string PlanoNome { get; set; } = string.Empty;
    public decimal PlanoValor { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public DateTime? DataProximaCobranca { get; set; }
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
}
