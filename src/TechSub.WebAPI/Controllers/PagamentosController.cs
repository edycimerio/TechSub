using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSub.Aplicacao.Services;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Enums;
using TechSub.Dominio.Interfaces;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para gerenciar pagamentos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PagamentosController : ControllerBase
{
    private readonly IPagamentoRepository _pagamentoRepository;
    private readonly IAssinaturaRepository _assinaturaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly NotificacaoService _notificacaoService;

    public PagamentosController(
        IPagamentoRepository pagamentoRepository,
        IAssinaturaRepository assinaturaRepository,
        IUsuarioRepository usuarioRepository,
        NotificacaoService notificacaoService)
    {
        _pagamentoRepository = pagamentoRepository;
        _assinaturaRepository = assinaturaRepository;
        _usuarioRepository = usuarioRepository;
        _notificacaoService = notificacaoService;
    }

    /// <summary>
    /// Obter histórico de pagamentos do usuário
    /// </summary>
    [HttpGet("meus")]
    public async Task<ActionResult<IEnumerable<PagamentoResponseDto>>> ObterMeusPagamentos()
    {
        var usuarioId = ObterUsuarioId();
        var assinaturas = await _assinaturaRepository.ObterPorUsuarioAsync(usuarioId);
        var assinaturaIds = assinaturas.Select(a => a.Id).ToList();

        var pagamentos = new List<Pagamento>();
        foreach (var assinaturaId in assinaturaIds)
        {
            var pagamentosAssinatura = await _pagamentoRepository.ObterPorAssinaturaAsync(assinaturaId);
            pagamentos.AddRange(pagamentosAssinatura);
        }

        var response = pagamentos.Select(p => new PagamentoResponseDto
        {
            Id = p.Id,
            AssinaturaId = p.AssinaturaId,
            PlanoNome = p.Assinatura?.Plano?.Nome ?? "N/A",
            Valor = p.Valor,
            Status = p.Status.ToString(),
            DataVencimento = p.DataVencimento,
            DataProcessamento = p.DataProcessamento,
            MetodoPagamento = p.MetodoPagamento,
            TransacaoId = p.TransacaoId,
            MensagemErro = p.MensagemErro
        }).OrderByDescending(p => p.DataVencimento);

        return Ok(response);
    }

    /// <summary>
    /// Obter detalhes de um pagamento específico
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PagamentoResponseDto>> ObterPagamento(Guid id)
    {
        var pagamento = await _pagamentoRepository.ObterPorIdAsync(id);
        if (pagamento == null)
            return NotFound("Pagamento não encontrado");

        var usuarioId = ObterUsuarioId();
        if (pagamento.Assinatura.UsuarioId != usuarioId && !User.IsInRole("Admin"))
            return Forbid("Acesso negado");

        var response = new PagamentoResponseDto
        {
            Id = pagamento.Id,
            AssinaturaId = pagamento.AssinaturaId,
            PlanoNome = pagamento.Assinatura?.Plano?.Nome ?? "N/A",
            Valor = pagamento.Valor,
            Status = pagamento.Status.ToString(),
            DataVencimento = pagamento.DataVencimento,
            DataProcessamento = pagamento.DataProcessamento,
            MetodoPagamento = pagamento.MetodoPagamento,
            TransacaoId = pagamento.TransacaoId,
            MensagemErro = pagamento.MensagemErro
        };

        return Ok(response);
    }

    /// <summary>
    /// Simular processamento de pagamento
    /// </summary>
    [HttpPost("{id}/processar")]
    public async Task<ActionResult> ProcessarPagamento(Guid id, [FromBody] ProcessarPagamentoDto request)
    {
        var pagamento = await _pagamentoRepository.ObterPorIdAsync(id);
        if (pagamento == null)
            return NotFound("Pagamento não encontrado");

        var usuarioId = ObterUsuarioId();
        if (pagamento.Assinatura.UsuarioId != usuarioId)
            return Forbid("Acesso negado");

        if (pagamento.Status != StatusPagamento.Pendente)
            return BadRequest("Apenas pagamentos pendentes podem ser processados");

        // Simulação de processamento de pagamento
        var sucesso = SimularProcessamentoPagamento(request.MetodoPagamento);

        if (sucesso)
        {
            pagamento.Status = StatusPagamento.Aprovado;
            pagamento.DataProcessamento = DateTime.UtcNow;
            pagamento.MetodoPagamento = request.MetodoPagamento;
            pagamento.TransacaoId = Guid.NewGuid().ToString("N")[..16]; // ID simulado
            pagamento.DataAtualizacao = DateTime.UtcNow;

            await _pagamentoRepository.AtualizarAsync(pagamento);

            // Notificar sucesso
            var usuario = await _usuarioRepository.ObterPorIdAsync(pagamento.Assinatura.UsuarioId);
            if (usuario != null)
            {
                await _notificacaoService.NotificarPagamentoSucessoAsync(pagamento, usuario);
            }

            return Ok(new { 
                message = "Pagamento processado com sucesso",
                transacaoId = pagamento.TransacaoId,
                status = "Aprovado"
            });
        }
        else
        {
            pagamento.Status = StatusPagamento.Rejeitado;
            pagamento.MetodoPagamento = request.MetodoPagamento;
            pagamento.MensagemErro = "Pagamento rejeitado pela operadora";
            pagamento.TentativasCobranca++;
            pagamento.DataAtualizacao = DateTime.UtcNow;

            await _pagamentoRepository.AtualizarAsync(pagamento);

            // Notificar falha
            var usuario = await _usuarioRepository.ObterPorIdAsync(pagamento.Assinatura.UsuarioId);
            if (usuario != null)
            {
                await _notificacaoService.NotificarPagamentoFalhaAsync(pagamento, usuario);
            }

            return BadRequest(new {
                message = "Pagamento rejeitado",
                erro = pagamento.MensagemErro,
                tentativas = pagamento.TentativasCobranca
            });
        }
    }

    /// <summary>
    /// Reprocessar pagamento falhado
    /// </summary>
    [HttpPost("{id}/reprocessar")]
    public async Task<ActionResult> ReprocessarPagamento(Guid id)
    {
        var pagamento = await _pagamentoRepository.ObterPorIdAsync(id);
        if (pagamento == null)
            return NotFound("Pagamento não encontrado");

        var usuarioId = ObterUsuarioId();
        if (pagamento.Assinatura.UsuarioId != usuarioId)
            return Forbid("Acesso negado");

        if (pagamento.Status != StatusPagamento.Rejeitado)
            return BadRequest("Apenas pagamentos rejeitados podem ser reprocessados");

        if (pagamento.TentativasCobranca >= 3)
            return BadRequest("Número máximo de tentativas excedido");

        // Resetar status para reprocessamento
        pagamento.Status = StatusPagamento.Pendente;
        pagamento.MensagemErro = null;
        pagamento.DataAtualizacao = DateTime.UtcNow;

        await _pagamentoRepository.AtualizarAsync(pagamento);

        return Ok(new { message = "Pagamento marcado para reprocessamento" });
    }

    /// <summary>
    /// Listar todos os pagamentos (apenas Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<PagamentoAdminDto>>> ListarTodosPagamentos(
        [FromQuery] StatusPagamento? status = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        IEnumerable<Pagamento> pagamentos;

        if (status.HasValue)
        {
            pagamentos = await _pagamentoRepository.ObterPorStatusAsync(status.Value);
        }
        else if (dataInicio.HasValue && dataFim.HasValue)
        {
            pagamentos = await _pagamentoRepository.ObterPorPeriodoAsync(dataInicio.Value, dataFim.Value);
        }
        else
        {
            // Buscar pagamentos dos últimos 30 dias por padrão
            var inicio = DateTime.UtcNow.AddDays(-30);
            var fim = DateTime.UtcNow;
            pagamentos = await _pagamentoRepository.ObterPorPeriodoAsync(inicio, fim);
        }

        var response = pagamentos.Select(p => new PagamentoAdminDto
        {
            Id = p.Id,
            AssinaturaId = p.AssinaturaId,
            UsuarioNome = p.Assinatura?.Usuario?.Nome ?? "N/A",
            UsuarioEmail = p.Assinatura?.Usuario?.Email ?? "N/A",
            PlanoNome = p.Assinatura?.Plano?.Nome ?? "N/A",
            Valor = p.Valor,
            Status = p.Status.ToString(),
            DataVencimento = p.DataVencimento,
            DataProcessamento = p.DataProcessamento,
            MetodoPagamento = p.MetodoPagamento,
            TransacaoId = p.TransacaoId,
            MensagemErro = p.MensagemErro,
            TentativasCobranca = p.TentativasCobranca,
            DataCriacao = p.DataCriacao
        }).OrderByDescending(p => p.DataCriacao);

        return Ok(response);
    }

    /// <summary>
    /// Obter estatísticas de pagamentos (apenas Admin)
    /// </summary>
    [HttpGet("estatisticas")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EstatisticasPagamentoDto>> ObterEstatisticas()
    {
        var inicio = DateTime.UtcNow.AddDays(-30);
        var fim = DateTime.UtcNow;
        var pagamentos = await _pagamentoRepository.ObterPorPeriodoAsync(inicio, fim);

        var estatisticas = new EstatisticasPagamentoDto
        {
            TotalPagamentos = pagamentos.Count(),
            PagamentosAprovados = pagamentos.Count(p => p.Status == StatusPagamento.Aprovado),
            PagamentosRejeitados = pagamentos.Count(p => p.Status == StatusPagamento.Rejeitado),
            PagamentosPendentes = pagamentos.Count(p => p.Status == StatusPagamento.Pendente),
            ValorTotalAprovado = pagamentos.Where(p => p.Status == StatusPagamento.Aprovado).Sum(p => p.Valor),
            TaxaAprovacao = pagamentos.Any() 
                ? (decimal)pagamentos.Count(p => p.Status == StatusPagamento.Aprovado) / pagamentos.Count() * 100 
                : 0,
            Periodo = $"{inicio:dd/MM/yyyy} - {fim:dd/MM/yyyy}"
        };

        return Ok(estatisticas);
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

    /// <summary>
    /// Simula processamento de pagamento com base no método
    /// </summary>
    private static bool SimularProcessamentoPagamento(string metodoPagamento)
    {
        // Simulação simples: 80% de aprovação para cartão, 90% para PIX
        var random = new Random();
        var chance = metodoPagamento.ToLower() switch
        {
            "pix" => 0.9,
            "cartao" => 0.8,
            "boleto" => 0.85,
            _ => 0.7
        };

        return random.NextDouble() < chance;
    }
}

/// <summary>
/// DTO para resposta de pagamento
/// </summary>
public class PagamentoResponseDto
{
    public Guid Id { get; set; }
    public Guid AssinaturaId { get; set; }
    public string PlanoNome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public string? MetodoPagamento { get; set; }
    public string? TransacaoId { get; set; }
    public string? MensagemErro { get; set; }
}

/// <summary>
/// DTO para processar pagamento
/// </summary>
public class ProcessarPagamentoDto
{
    public string MetodoPagamento { get; set; } = string.Empty; // pix, cartao, boleto
}

/// <summary>
/// DTO para visualização admin de pagamentos
/// </summary>
public class PagamentoAdminDto
{
    public Guid Id { get; set; }
    public Guid AssinaturaId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string UsuarioEmail { get; set; } = string.Empty;
    public string PlanoNome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public string? MetodoPagamento { get; set; }
    public string? TransacaoId { get; set; }
    public string? MensagemErro { get; set; }
    public int TentativasCobranca { get; set; }
    public DateTime DataCriacao { get; set; }
}

/// <summary>
/// DTO para estatísticas de pagamentos
/// </summary>
public class EstatisticasPagamentoDto
{
    public int TotalPagamentos { get; set; }
    public int PagamentosAprovados { get; set; }
    public int PagamentosRejeitados { get; set; }
    public int PagamentosPendentes { get; set; }
    public decimal ValorTotalAprovado { get; set; }
    public decimal TaxaAprovacao { get; set; }
    public string Periodo { get; set; } = string.Empty;
}
