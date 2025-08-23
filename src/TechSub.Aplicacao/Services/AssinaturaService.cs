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
                    await ConverterTrialParaPagaAsync(assinatura, assinatura.UsuarioId, null, null);
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
    public async Task<object> CalcularMRRAsync(string? userRole)
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

        return new object();
    }

    /// <summary>
    /// Obtém assinatura ativa do usuário
    /// </summary>
    public async Task<AssinaturaResponse?> ObterAtivaAsync(Guid usuarioId, Guid usuarioLogadoId, string? userRole)
    {
        // Validar autorização - usuário só pode ver suas próprias assinaturas, exceto Admin
        if (userRole != "Admin" && usuarioLogadoId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acesso negado. Você só pode acessar suas próprias assinaturas.");
        }

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
    public async Task<IEnumerable<AssinaturaResponse>> ObterPorUsuarioAsync(Guid usuarioId, Guid usuarioLogadoId, string? userRole)
    {
        // Validar autorização - usuário só pode ver suas próprias assinaturas, exceto Admin
        if (userRole != "Admin" && usuarioLogadoId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acesso negado. Você só pode acessar suas próprias assinaturas.");
        }

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
        // Verificar se usuário já tem assinatura ativa
        var assinaturaExistente = await _assinaturaRepository.ObterAtivaPorUsuarioAsync(usuarioId);
        if (assinaturaExistente != null)
        {
            throw new InvalidOperationException("Usuário já possui uma assinatura ativa");
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
    public async Task<bool> CancelarAsync(Guid assinaturaId, Guid usuarioId, string? userRole)
    {
        var assinatura = await _assinaturaRepository.ObterPorIdAsync(assinaturaId);
        if (assinatura == null) return false;

        // Validar autorização - usuário só pode cancelar suas próprias assinaturas, exceto Admin
        if (userRole != "Admin" && assinatura.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acesso negado. Você só pode cancelar suas próprias assinaturas.");
        }

        assinatura.Status = StatusAssinatura.Cancelada;
        assinatura.DataTermino = assinatura.DataProximaCobranca; // Mantém acesso até fim do ciclo

        await _assinaturaRepository.AtualizarAsync(assinatura);
        return true;
    }

    /// <summary>
    /// Renova assinatura
    /// </summary>
    public async Task<bool> RenovarAsync(Guid assinaturaId, Guid usuarioId, string? userRole)
    {
        var assinatura = await _assinaturaRepository.ObterPorIdAsync(assinaturaId);
        if (assinatura == null) return false;

        // Validar autorização - usuário só pode renovar suas próprias assinaturas, exceto Admin
        if (userRole != "Admin" && assinatura.UsuarioId != usuarioId)
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
    public async Task<IEnumerable<AssinaturaResponse>> ObterTodasAsync(string? userRole)
    {
        // Validar autorização - apenas Admin pode ver todas as assinaturas
        if (userRole != "Admin")
        {
            throw new UnauthorizedAccessException("Acesso negado. Apenas administradores podem listar todas as assinaturas.");
        }

        var assinaturas = await _assinaturaRepository.ObterTodosAsync();
        
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

    private async Task ConverterTrialParaPagaAsync(Assinatura assinatura, Guid usuarioId, Guid? usuarioLogadoId = null, string? userRole = null)
    {
        // Validar autorização - usuário só pode criar assinatura para si mesmo, exceto Admin
        if (userRole != "Admin" && usuarioLogadoId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acesso negado. Você só pode criar assinatura para si mesmo.");
        }

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
