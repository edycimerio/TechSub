using TechSub.Dominio.Entidades;
using TechSub.Dominio.Enums;
using TechSub.Dominio.Interfaces;

namespace TechSub.Aplicacao.Services;

/// <summary>
/// Serviço para gerenciar lógica de negócio de assinaturas
/// </summary>
public class AssinaturaService
{
    private readonly IAssinaturaRepository _assinaturaRepository;
    private readonly IPagamentoRepository _pagamentoRepository;
    private readonly IPlanoRepository _planoRepository;

    public AssinaturaService(
        IAssinaturaRepository assinaturaRepository,
        IPagamentoRepository pagamentoRepository,
        IPlanoRepository planoRepository)
    {
        _assinaturaRepository = assinaturaRepository;
        _pagamentoRepository = pagamentoRepository;
        _planoRepository = planoRepository;
    }

    /// <summary>
    /// Processa trials expirados e faz downgrade para Free ou cobrança
    /// </summary>
    public async Task ProcessarTrialsExpiradosAsync()
    {
        var trialsExpirados = await _assinaturaRepository.ObterPorStatusAsync(StatusAssinatura.Trial);
        var hoje = DateTime.UtcNow;

        // Buscar plano Free
        var planoFree = await _planoRepository.ObterPorNomeAsync("Free");
        if (planoFree == null)
        {
            throw new InvalidOperationException("Plano Free não encontrado no sistema");
        }

        foreach (var assinatura in trialsExpirados)
        {
            if (assinatura.DataTerminoTrial <= hoje)
            {
                // Verificar se tem método de pagamento (simulado como verdadeiro por enquanto)
                var temMetodoPagamento = true; // TODO: Implementar verificação real

                if (temMetodoPagamento)
                {
                    // Converter para assinatura paga
                    await ConverterTrialParaPagaAsync(assinatura);
                }
                else
                {
                    // Downgrade para Free
                    await FazerDowngradeParaFreeAsync(assinatura, planoFree);
                }
            }
        }
    }

    /// <summary>
    /// Processa renovações automáticas
    /// </summary>
    public async Task ProcessarRenovacoesAutomaticasAsync()
    {
        var assinaturasAtivas = await _assinaturaRepository.ObterPorStatusAsync(StatusAssinatura.Ativa);
        var hoje = DateTime.UtcNow;

        foreach (var assinatura in assinaturasAtivas)
        {
            if (assinatura.DataProximaCobranca <= hoje)
            {
                await RenovarAssinaturaAutomaticamenteAsync(assinatura);
            }
        }
    }

    /// <summary>
    /// Processa cancelamentos no fim do ciclo
    /// </summary>
    public async Task ProcessarCancelamentosAsync()
    {
        var assinaturasCanceladas = await _assinaturaRepository.ObterPorStatusAsync(StatusAssinatura.Cancelada);
        var hoje = DateTime.UtcNow;

        foreach (var assinatura in assinaturasCanceladas)
        {
            // Se tem data de término e já passou, marcar como expirada
            if (assinatura.DataTermino.HasValue && assinatura.DataTermino <= hoje)
            {
                assinatura.Status = StatusAssinatura.Expirada;
                assinatura.DataAtualizacao = DateTime.UtcNow;
                await _assinaturaRepository.AtualizarAsync(assinatura);
            }
        }
    }

    private async Task ConverterTrialParaPagaAsync(Assinatura assinatura)
    {
        // Converter trial para assinatura ativa
        assinatura.Status = StatusAssinatura.Ativa;
        assinatura.EmTrial = false;
        assinatura.DataTerminoTrial = null;
        assinatura.DataProximaCobranca = assinatura.Periodicidade == PeriodicidadeCobranca.Mensal
            ? DateTime.UtcNow.AddMonths(1)
            : DateTime.UtcNow.AddYears(1);
        assinatura.DataAtualizacao = DateTime.UtcNow;

        await _assinaturaRepository.AtualizarAsync(assinatura);

        // Criar cobrança
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
    }

    private async Task FazerDowngradeParaFreeAsync(Assinatura assinatura, Plano planoFree)
    {
        // Fazer downgrade para Free
        assinatura.Status = StatusAssinatura.Ativa;
        assinatura.EmTrial = false;
        assinatura.DataTerminoTrial = null;
        assinatura.PlanoId = planoFree.Id;
        assinatura.Valor = planoFree.PrecoMensal; // Free = 0
        assinatura.DataProximaCobranca = null; // Free não tem cobrança
        assinatura.DataAtualizacao = DateTime.UtcNow;

        await _assinaturaRepository.AtualizarAsync(assinatura);
    }

    private async Task RenovarAssinaturaAutomaticamenteAsync(Assinatura assinatura)
    {
        // Calcular próxima cobrança
        var proximaCobranca = assinatura.Periodicidade == PeriodicidadeCobranca.Mensal
            ? assinatura.DataProximaCobranca?.AddMonths(1) ?? DateTime.UtcNow.AddMonths(1)
            : assinatura.DataProximaCobranca?.AddYears(1) ?? DateTime.UtcNow.AddYears(1);

        assinatura.DataProximaCobranca = proximaCobranca;
        assinatura.DataAtualizacao = DateTime.UtcNow;

        await _assinaturaRepository.AtualizarAsync(assinatura);

        // Criar nova cobrança
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
    }
}
