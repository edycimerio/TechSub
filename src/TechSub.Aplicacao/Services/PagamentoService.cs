using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Aplicacao.Interfaces;
using TechSub.Dominio.Enums;

namespace TechSub.Aplicacao.Services;

/// <summary>
/// Serviço para gerenciar pagamentos
/// </summary>
public class PagamentoService : IPagamentoService
{
    private readonly IPagamentoRepository _pagamentoRepository;
    private readonly IAssinaturaRepository _assinaturaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly INotificacaoService _notificacaoService;

    public PagamentoService(
        IPagamentoRepository pagamentoRepository,
        IAssinaturaRepository assinaturaRepository,
        IUsuarioRepository usuarioRepository,
        INotificacaoService notificacaoService)
    {
        _pagamentoRepository = pagamentoRepository;
        _assinaturaRepository = assinaturaRepository;
        _usuarioRepository = usuarioRepository;
        _notificacaoService = notificacaoService;
    }

    /// <summary>
    /// Obtém histórico de pagamentos do usuário
    /// </summary>
    public async Task<IEnumerable<PagamentoResponse>> ObterHistoricoUsuarioAsync(Guid usuarioId)
    {
        var pagamentos = await _pagamentoRepository.ObterPorUsuarioAsync(usuarioId);
        
        return pagamentos.Select(p => new PagamentoResponse
        {
            Id = p.Id,
            AssinaturaId = p.AssinaturaId,
            PlanoNome = p.Assinatura.Plano.Nome,
            Valor = p.Valor,
            Status = p.Status.ToString(),
            MetodoPagamento = p.MetodoPagamento,
            TransacaoId = p.TransacaoId,
            DataCriacao = p.DataCriacao,
            DataProcessamento = p.DataProcessamento,
            MotivoFalha = p.MotivoFalha
        });
    }

    /// <summary>
    /// Obtém todos os pagamentos
    /// </summary>
    public async Task<IEnumerable<PagamentoResponse>> ObterTodosAsync(string? status = null, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var pagamentos = await _pagamentoRepository.ObterTodosAsync();
        
        return pagamentos.Select(p => new PagamentoResponse
        {
            Id = p.Id,
            AssinaturaId = p.AssinaturaId,
            PlanoNome = p.Assinatura.Plano.Nome,
            UsuarioNome = p.Assinatura.Usuario.Nome,
            UsuarioEmail = p.Assinatura.Usuario.Email,
            Valor = p.Valor,
            Status = p.Status.ToString(),
            MetodoPagamento = p.MetodoPagamento,
            TransacaoId = p.TransacaoId,
            DataCriacao = p.DataCriacao,
            DataProcessamento = p.DataProcessamento,
            MotivoFalha = p.MotivoFalha
        });
    }

    /// <summary>
    /// Obtém detalhes de um pagamento
    /// </summary>
    public async Task<PagamentoResponse?> ObterPorIdAsync(Guid id, Guid usuarioId)
    {
        var pagamento = await _pagamentoRepository.ObterPorIdAsync(id);
        if (pagamento == null) return null;

        // Validar propriedade do pagamento
        if (pagamento.Assinatura.UsuarioId != usuarioId)
            return null;

        return new PagamentoResponse
        {
            Id = pagamento.Id,
            AssinaturaId = pagamento.AssinaturaId,
            PlanoNome = pagamento.Assinatura.Plano.Nome,
            UsuarioNome = pagamento.Assinatura.Usuario.Nome,
            UsuarioEmail = pagamento.Assinatura.Usuario.Email,
            Valor = pagamento.Valor,
            Status = pagamento.Status.ToString(),
            MetodoPagamento = pagamento.MetodoPagamento,
            TransacaoId = pagamento.TransacaoId,
            DataCriacao = pagamento.DataCriacao,
            DataProcessamento = pagamento.DataProcessamento,
            MotivoFalha = pagamento.MotivoFalha
        };
    }

    /// <summary>
    /// Processa pagamento (simulação)
    /// </summary>
    public async Task<object> ProcessarPagamentoAsync(Guid pagamentoId, ProcessarPagamentoRequest dto, Guid usuarioId)
    {
        var pagamento = await _pagamentoRepository.ObterPorIdAsync(pagamentoId);
        if (pagamento == null)
            throw new ArgumentException("Pagamento não encontrado");

        // Validar propriedade do pagamento
        if (pagamento.Assinatura.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("Acesso negado ao pagamento");

        if (pagamento.Status != StatusPagamento.Pendente)
            throw new InvalidOperationException("Pagamento já foi processado");

        // Simulação de processamento (80-90% de aprovação)
        var random = new Random();
        var aprovado = random.Next(1, 101) <= 85; // 85% de aprovação

        pagamento.Status = aprovado ? StatusPagamento.Aprovado : StatusPagamento.Rejeitado;
        pagamento.DataProcessamento = DateTime.UtcNow;
        pagamento.TransacaoId = Guid.NewGuid().ToString();

        if (!aprovado)
        {
            var motivos = new[] { "Cartão recusado", "Saldo insuficiente", "Cartão expirado", "Dados inválidos" };
            pagamento.MotivoFalha = motivos[random.Next(motivos.Length)];
        }

        await _pagamentoRepository.AtualizarAsync(pagamento);

        // Buscar usuário para notificação
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        
        if (aprovado)
        {
            // Atualizar assinatura se aprovado
            var assinatura = await _assinaturaRepository.ObterPorIdAsync(pagamento.AssinaturaId);
            if (assinatura != null)
            {
                assinatura.Status = StatusAssinatura.Ativa;
                if (assinatura.EmTrial)
                {
                    assinatura.EmTrial = false;
                    assinatura.DataProximaCobranca = assinatura.Periodicidade == PeriodicidadeCobranca.Mensal 
                        ? DateTime.UtcNow.AddMonths(1) 
                        : DateTime.UtcNow.AddYears(1);
                }
                await _assinaturaRepository.AtualizarAsync(assinatura);
            }

            await _notificacaoService.EnviarNotificacaoPagamentoAprovadoAsync(usuario!.Id, pagamento.Valor);
        }
        else
        {
            await _notificacaoService.EnviarNotificacaoPagamentoRejeitadoAsync(usuario!.Id, pagamento.Valor, pagamento.MotivoFalha!);
        }

        return new PagamentoResponse
        {
            Id = pagamento.Id,
            AssinaturaId = pagamento.AssinaturaId,
            PlanoNome = pagamento.Assinatura.Plano.Nome,
            UsuarioNome = pagamento.Assinatura.Usuario.Nome,
            UsuarioEmail = pagamento.Assinatura.Usuario.Email,
            Valor = pagamento.Valor,
            Status = pagamento.Status.ToString(),
            MetodoPagamento = pagamento.MetodoPagamento,
            TransacaoId = pagamento.TransacaoId,
            DataCriacao = pagamento.DataCriacao,
            DataProcessamento = pagamento.DataProcessamento,
            MotivoFalha = pagamento.MotivoFalha
        };
    }

    /// <summary>
    /// Reprocessa pagamento falhado
    /// </summary>
    public async Task<object> ReprocessarPagamentoAsync(Guid pagamentoId, Guid usuarioId)
    {
        var pagamento = await _pagamentoRepository.ObterPorIdAsync(pagamentoId);
        if (pagamento == null)
            throw new ArgumentException("Pagamento não encontrado");

        // Validar propriedade
        if (pagamento.Assinatura.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("Acesso negado ao pagamento");

        if (pagamento.Status != StatusPagamento.Rejeitado)
            throw new InvalidOperationException("Apenas pagamentos rejeitados podem ser reprocessados");

        // Reset para reprocessamento
        pagamento.Status = StatusPagamento.Pendente;
        pagamento.MotivoFalha = null;
        pagamento.TransacaoId = null;
        pagamento.DataProcessamento = null;

        await _pagamentoRepository.AtualizarAsync(pagamento);

        // Processar novamente
        return await ProcessarPagamentoAsync(pagamentoId, new ProcessarPagamentoRequest { PagamentoId = pagamentoId }, usuarioId);
    }

    /// <summary>
    /// Obtém estatísticas de pagamentos
    /// </summary>
    public async Task<object> ObterEstatisticasAsync()
    {
        var pagamentos = await _pagamentoRepository.ObterTodosAsync();
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

        var totalPagamentos = pagamentos.Count();
        var totalPagamentosAprovados = pagamentos.Count(p => p.Status == StatusPagamento.Aprovado);

        return new EstatisticasPagamentoResponse
        {
            TotalPagamentos = totalPagamentos,
            PagamentosAprovados = totalPagamentosAprovados,
            PagamentosRejeitados = pagamentos.Count(p => p.Status == StatusPagamento.Rejeitado),
            PagamentosPendentes = pagamentos.Count(p => p.Status == StatusPagamento.Pendente),
            ReceitaTotal = pagamentos.Where(p => p.Status == StatusPagamento.Aprovado).Sum(p => p.Valor),
            ReceitaMesAtual = pagamentos
                .Where(p => p.Status == StatusPagamento.Aprovado && p.DataProcessamento >= inicioMes)
                .Sum(p => p.Valor),
            TaxaAprovacao = totalPagamentos > 0 
                ? Math.Round((double)(totalPagamentosAprovados * 100.0 / totalPagamentos), 2) 
                : 0
        };
    }
}
