using TechSub.Aplicacao.Requests;
using TechSub.Aplicacao.Responses;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces.Repositories;
using TechSub.Aplicacao.Interfaces;
using TechSub.Dominio.Enums;

namespace TechSub.Aplicacao.Services;

/// <summary>
/// Serviço para gerenciar assinaturas e lógica de negócio SaaS
/// </summary>
public class AssinaturaService : IAssinaturaService
{
    private readonly IAssinaturaRepository _assinaturaRepository;
    private readonly IPagamentoRepository _pagamentoRepository;
    private readonly IPlanoRepository _planoRepository;
    private readonly INotificacaoService _notificacaoService;
    private readonly IUsuarioMetodoPagamentoRepository _usuarioMetodoPagamentoRepository;

    public AssinaturaService(
        IAssinaturaRepository assinaturaRepository,
        IPagamentoRepository pagamentoRepository,
        IPlanoRepository planoRepository,
        INotificacaoService notificacaoService,
        IUsuarioMetodoPagamentoRepository usuarioMetodoPagamentoRepository)
    {
        _assinaturaRepository = assinaturaRepository;
        _pagamentoRepository = pagamentoRepository;
        _planoRepository = planoRepository;
        _notificacaoService = notificacaoService;
        _usuarioMetodoPagamentoRepository = usuarioMetodoPagamentoRepository;
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
                // Verificar se tem método de pagamento (consulta dinâmica no banco)
                var temMetodoPagamento = await _usuarioMetodoPagamentoRepository.TemMetodoPagamentoAsync(assinatura.UsuarioId);

                if (temMetodoPagamento)
                {
                    // Converter para assinatura paga
                    await ConverterTrialParaPagaAsync(assinatura, assinatura.UsuarioId);
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
    /// Calcula MRR (Monthly Recurring Revenue)
    /// </summary>
    public async Task<decimal> CalcularMRRAsync()
    {
        return await _assinaturaRepository.CalcularMRRAsync();
    }

    /// <summary>
    /// Obtém assinatura ativa do usuário
    /// </summary>
    public async Task<AssinaturaResponse?> ObterAtivaAsync(Guid usuarioId, Guid usuarioLogadoId)
    {

        var assinaturas = await _assinaturaRepository.ObterPorUsuarioAsync(usuarioId);
        var assinaturaAtiva = assinaturas.FirstOrDefault(a => a.Status == StatusAssinatura.Ativa);
        
        if (assinaturaAtiva == null) return null;

        return new AssinaturaResponse
        {
            Id = assinaturaAtiva.Id,
            UsuarioId = assinaturaAtiva.UsuarioId,
            PlanoId = assinaturaAtiva.PlanoId,
            PlanoNome = assinaturaAtiva.Plano.Nome,
            Status = assinaturaAtiva.Status.ToString(),
            EmTrial = assinaturaAtiva.EmTrial,
            DataInicio = assinaturaAtiva.DataInicio,
            DataTermino = assinaturaAtiva.DataTermino,
            DataTerminoTrial = assinaturaAtiva.DataTerminoTrial,
            DataProximaCobranca = assinaturaAtiva.DataProximaCobranca,
            ValorMensal = assinaturaAtiva.Plano.PrecoMensal,
            ValorAnual = assinaturaAtiva.Plano.PrecoAnual
        };
    }

    /// <summary>
    /// Obtém assinaturas do usuário
    /// </summary>
    public async Task<IEnumerable<AssinaturaResponse>> ObterPorUsuarioAsync(Guid usuarioId, Guid usuarioLogadoId)
    {

        var assinaturas = await _assinaturaRepository.ObterPorUsuarioAsync(usuarioId);
        
        return assinaturas.Select(a => new AssinaturaResponse
        {
            Id = a.Id,
            UsuarioId = a.UsuarioId,
            PlanoId = a.PlanoId,
            PlanoNome = a.Plano.Nome,
            Status = a.Status.ToString(),
            EmTrial = a.EmTrial,
            DataInicio = a.DataInicio,
            DataTermino = a.DataTermino,
            DataTerminoTrial = a.DataTerminoTrial,
            DataProximaCobranca = a.DataProximaCobranca,
            ValorMensal = a.Plano.PrecoMensal,
            ValorAnual = a.Plano.PrecoAnual
        });
    }

    /// <summary>
    /// Cria nova assinatura
    /// </summary>
    public async Task<AssinaturaResponse> CriarAsync(CriarAssinaturaRequest dto, Guid usuarioId)
    {
        // Verificar se usuário já tem assinatura ativa (REQUISITO: apenas UMA assinatura ativa por vez)
        var assinaturaExistente = await _assinaturaRepository.ObterAtivaPorUsuarioAsync(usuarioId);
        if (assinaturaExistente != null)
        {
            throw new InvalidOperationException("Usuário já possui uma assinatura ativa. Cancele a assinatura atual antes de criar uma nova.");
        }

        var plano = await _planoRepository.ObterPorIdAsync(dto.PlanoId);
        if (plano == null)
        {
            throw new ArgumentException("Plano não encontrado");
        }

        var novaAssinatura = new Assinatura
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            PlanoId = dto.PlanoId,
            Status = plano.TemTrial ? StatusAssinatura.Trial : StatusAssinatura.Ativa,
            EmTrial = plano.TemTrial,
            DataInicio = DateTime.UtcNow,
            DataTerminoTrial = plano.TemTrial ? DateTime.UtcNow.AddDays(plano.DiasTrialGratuito) : null,
            DataProximaCobranca = plano.TemTrial ? DateTime.UtcNow.AddDays(plano.DiasTrialGratuito) : DateTime.UtcNow.AddMonths(1)
        };

        await _assinaturaRepository.AdicionarAsync(novaAssinatura);

        return new AssinaturaResponse
        {
            Id = novaAssinatura.Id,
            UsuarioId = novaAssinatura.UsuarioId,
            PlanoId = novaAssinatura.PlanoId,
            PlanoNome = plano.Nome,
            Status = novaAssinatura.Status.ToString(),
            EmTrial = novaAssinatura.EmTrial,
            DataInicio = novaAssinatura.DataInicio,
            DataTermino = novaAssinatura.DataTermino,
            DataTerminoTrial = novaAssinatura.DataTerminoTrial,
            DataProximaCobranca = novaAssinatura.DataProximaCobranca,
            ValorMensal = plano.PrecoMensal,
            ValorAnual = plano.PrecoAnual
        };
    }

    /// <summary>
    /// Cancela assinatura
    /// </summary>
    public async Task<bool> CancelarAsync(Guid assinaturaId, Guid usuarioId)
    {
        var assinatura = await _assinaturaRepository.ObterPorIdAsync(assinaturaId);
        if (assinatura == null) return false;

        // Validar autorização - usuário só pode cancelar suas próprias assinaturas
        if (assinatura.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acesso negado. Você só pode cancelar suas próprias assinaturas.");
        }

        assinatura.Status = StatusAssinatura.Cancelada;
        assinatura.DataTermino = assinatura.DataProximaCobranca; // Mantém acesso até fim do ciclo

        await _assinaturaRepository.AtualizarAsync(assinatura);
        return true;
    }

    /// <summary>
    /// Obtém assinatura por ID
    /// </summary>
    public async Task<AssinaturaResponse?> ObterPorIdAsync(Guid id, Guid usuarioId)
    {
        var assinatura = await _assinaturaRepository.ObterPorIdAsync(id);
        if (assinatura == null) return null;

        // Usuário só pode ver suas próprias assinaturas
        if (assinatura.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acesso negado. Você só pode acessar suas próprias assinaturas.");
        }

        return new AssinaturaResponse
        {
            Id = assinatura.Id,
            UsuarioId = assinatura.UsuarioId,
            PlanoId = assinatura.PlanoId,
            PlanoNome = assinatura.Plano.Nome,
            Status = assinatura.Status.ToString(),
            Periodicidade = assinatura.Periodicidade.ToString(),
            EmTrial = assinatura.EmTrial,
            DataInicio = assinatura.DataInicio,
            DataTermino = assinatura.DataTermino,
            DataTerminoTrial = assinatura.DataTerminoTrial,
            DataProximaCobranca = assinatura.DataProximaCobranca,
            ValorMensal = assinatura.Plano.PrecoMensal,
            ValorAnual = assinatura.Plano.PrecoAnual
        };
    }

    /// <summary>
    /// Atualiza assinatura
    /// </summary>
    public async Task<AssinaturaResponse?> AtualizarAsync(Guid id, AtualizarAssinaturaRequest request, Guid usuarioId)
    {
        var assinatura = await _assinaturaRepository.ObterPorIdAsync(id);
        if (assinatura == null) return null;

        // Usuário só pode atualizar suas próprias assinaturas
        if (assinatura.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acesso negado. Você só pode atualizar suas próprias assinaturas.");
        }

        var novoPlano = await _planoRepository.ObterPorIdAsync(request.PlanoId);
        if (novoPlano == null)
            throw new InvalidOperationException("Plano não encontrado");

        assinatura.PlanoId = request.PlanoId;
        assinatura.Periodicidade = Enum.Parse<PeriodicidadeCobranca>(request.TipoCobranca, true);
        assinatura.DataAtualizacao = DateTime.UtcNow;

        await _assinaturaRepository.AtualizarAsync(assinatura);

        return new AssinaturaResponse
        {
            Id = assinatura.Id,
            UsuarioId = assinatura.UsuarioId,
            PlanoId = assinatura.PlanoId,
            PlanoNome = novoPlano.Nome,
            Status = assinatura.Status.ToString(),
            Periodicidade = assinatura.Periodicidade.ToString(),
            EmTrial = assinatura.EmTrial,
            DataInicio = assinatura.DataInicio,
            DataTermino = assinatura.DataTermino,
            DataTerminoTrial = assinatura.DataTerminoTrial,
            DataProximaCobranca = assinatura.DataProximaCobranca,
            ValorMensal = novoPlano.PrecoMensal,
            ValorAnual = novoPlano.PrecoAnual
        };
    }

    /// <summary>
    /// Remove assinatura
    /// </summary>
    public async Task<bool> RemoverAsync(Guid id, Guid usuarioId)
    {
        var assinatura = await _assinaturaRepository.ObterPorIdAsync(id);
        if (assinatura == null) return false;

        // Usuário só pode remover suas próprias assinaturas
        if (assinatura.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acesso negado. Você só pode remover suas próprias assinaturas.");
        }

        await _assinaturaRepository.RemoverAsync(id);
        return true;
    }

    /// <summary>
    /// Renova assinatura
    /// </summary>
    public async Task<bool> RenovarAsync(Guid assinaturaId, Guid usuarioId)
    {
        var assinatura = await _assinaturaRepository.ObterPorIdAsync(assinaturaId);
        if (assinatura == null) return false;

        // Validar autorização - usuário só pode renovar suas próprias assinaturas
        if (assinatura.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acesso negado. Você só pode renovar suas próprias assinaturas.");
        }

        assinatura.Status = StatusAssinatura.Ativa;
        assinatura.DataTermino = null; // Remove data de término
        assinatura.DataProximaCobranca = DateTime.UtcNow.AddMonths(1);

        await _assinaturaRepository.AtualizarAsync(assinatura);
        return true;
    }

    /// <summary>
    /// Obtém todas as assinaturas (admin)
    /// </summary>
    public async Task<IEnumerable<AssinaturaResponse>> ObterTodasAsync()
    {

        var assinaturas = await _assinaturaRepository.ObterTodasAsync();
        
        return assinaturas.Select(a => new AssinaturaResponse
        {
            Id = a.Id,
            UsuarioId = a.UsuarioId,
            PlanoId = a.PlanoId,
            PlanoNome = a.Plano.Nome,
            Status = a.Status.ToString(),
            EmTrial = a.EmTrial,
            DataInicio = a.DataInicio,
            DataTermino = a.DataTermino,
            DataTerminoTrial = a.DataTerminoTrial,
            DataProximaCobranca = a.DataProximaCobranca,
            ValorMensal = a.Plano.PrecoMensal,
            ValorAnual = a.Plano.PrecoAnual
        });
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
                await _assinaturaRepository.AtualizarAsync(assinatura);
            }
        }
    }

    private async Task ConverterTrialParaPagaAsync(Assinatura assinatura, Guid usuarioId, Guid? usuarioLogadoId = null)
    {

        // Verificar se já existe assinatura ativa
        var assinaturaExistente = await _assinaturaRepository.ObterPorUsuarioAsync(usuarioId);
        if (assinaturaExistente.Any(a => a.Status == StatusAssinatura.Ativa))
        {
            throw new InvalidOperationException("Já existe uma assinatura ativa para este usuário.");
        }

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
        await _notificacaoService.EnviarNotificacaoAssinaturaRenovadaAsync(assinatura.UsuarioId, assinatura.Plano.Nome, null);
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

    /// <summary>
    /// TESTE: Simula trial expirado modificando data para ontem
    /// </summary>
    public async Task SimularTrialExpiradoAsync(Guid assinaturaId, Guid usuarioId)
    {
        var assinatura = await _assinaturaRepository.ObterPorIdAsync(assinaturaId);
        if (assinatura == null || assinatura.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Assinatura não encontrada ou acesso negado");
        }

        if (assinatura.Status != StatusAssinatura.Trial)
        {
            throw new InvalidOperationException("Apenas assinaturas em trial podem ser simuladas como expiradas");
        }

        // Modificar data para ontem
        assinatura.DataTerminoTrial = DateTime.UtcNow.AddDays(-1);
        await _assinaturaRepository.AtualizarAsync(assinatura);
    }
}
