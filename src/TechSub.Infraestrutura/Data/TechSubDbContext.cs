using Microsoft.EntityFrameworkCore;
using TechSub.Dominio.Entidades;

namespace TechSub.Infraestrutura.Data;

/// <summary>
/// Contexto do banco de dados para o TechSub
/// </summary>
public class TechSubDbContext : DbContext
{
    public TechSubDbContext(DbContextOptions<TechSubDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Usuários do sistema
    /// </summary>
    public DbSet<Usuario> Usuarios { get; set; } = null!;

    /// <summary>
    /// Planos de assinatura disponíveis
    /// </summary>
    public DbSet<Plano> Planos { get; set; } = null!;

    /// <summary>
    /// Assinaturas dos usuários
    /// </summary>
    public DbSet<Assinatura> Assinaturas { get; set; } = null!;

    /// <summary>
    /// Pagamentos das assinaturas
    /// </summary>
    public DbSet<Pagamento> Pagamentos { get; set; } = null!;

    /// <summary>
    /// Métodos de pagamento dos usuários
    /// </summary>
    public DbSet<UsuarioMetodoPagamento> UsuarioMetodoPagamento { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Nome).IsRequired().HasMaxLength(200);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.SenhaHash).HasMaxLength(500);
            entity.Property(u => u.ProvedorId).HasMaxLength(255);
            entity.Property(u => u.Provedor).HasMaxLength(50);
            entity.Property(u => u.AvatarUrl).HasMaxLength(500);
            entity.Property(u => u.Role).HasMaxLength(50);
            entity.Property(u => u.DataCriacao).HasDefaultValueSql("NOW()");
            entity.Property(u => u.DataAtualizacao).HasDefaultValueSql("NOW()");
        });

        // Configuração da entidade Plano
        modelBuilder.Entity<Plano>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Nome).IsUnique();
            entity.Property(p => p.Nome).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Descricao).HasMaxLength(500);
            entity.Property(p => p.PrecoMensal).HasColumnType("decimal(10,2)");
            entity.Property(p => p.PrecoAnual).HasColumnType("decimal(10,2)");
            entity.Property(p => p.Recursos).HasMaxLength(2000);
            entity.Property(p => p.DataCriacao).HasDefaultValueSql("NOW()");
            entity.Property(p => p.DataAtualizacao).HasDefaultValueSql("NOW()");
        });

        // Configuração da entidade Assinatura
        modelBuilder.Entity<Assinatura>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Valor).HasColumnType("decimal(10,2)");
            entity.Property(a => a.DataCriacao).HasDefaultValueSql("NOW()");
            entity.Property(a => a.DataAtualizacao).HasDefaultValueSql("NOW()");

            // Relacionamento com Usuario
            entity.HasOne(a => a.Usuario)
                  .WithMany(u => u.Assinaturas)
                  .HasForeignKey(a => a.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relacionamento com Plano
            entity.HasOne(a => a.Plano)
                  .WithMany(p => p.Assinaturas)
                  .HasForeignKey(a => a.PlanoId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índice para busca rápida de assinatura ativa por usuário
            entity.HasIndex(a => new { a.UsuarioId, a.Status });
        });

        // Configuração da entidade Pagamento
        modelBuilder.Entity<Pagamento>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Valor).HasColumnType("decimal(10,2)");
            entity.Property(p => p.MetodoPagamento).HasMaxLength(50);
            entity.Property(p => p.TransacaoId).HasMaxLength(255);
            entity.Property(p => p.MensagemErro).HasMaxLength(500);
            entity.Property(p => p.DataCriacao).HasDefaultValueSql("NOW()");
            entity.Property(p => p.DataAtualizacao).HasDefaultValueSql("NOW()");

            // Relacionamento com Assinatura
            entity.HasOne(p => p.Assinatura)
                  .WithMany(a => a.Pagamentos)
                  .HasForeignKey(p => p.AssinaturaId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Índices para performance
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.DataVencimento);
        });

        // Configuração de relacionamento 1:1 para AssinaturaAtiva
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.AssinaturaAtiva)
            .WithOne()
            .HasForeignKey<Usuario>("AssinaturaAtivaId")
            .OnDelete(DeleteBehavior.SetNull);

        // Seed data para planos básicos
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Dados iniciais para o sistema
    /// </summary>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        var planoFree = new Plano
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Nome = "Free",
            Descricao = "Plano gratuito com recursos básicos",
            PrecoMensal = 0,
            PrecoAnual = 0,
            TemTrial = false,
            DiasTrialGratuito = 0,
            Recursos = "{\"usuarios\": 1, \"projetos\": 1, \"storage\": \"100MB\"}",
            Ordem = 1,
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        var planoBasic = new Plano
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Nome = "Basic",
            Descricao = "Plano básico para pequenas equipes",
            PrecoMensal = 29.90m,
            PrecoAnual = 299.00m,
            TemTrial = true,
            DiasTrialGratuito = 7,
            Recursos = "{\"usuarios\": 5, \"projetos\": 10, \"storage\": \"10GB\"}",
            Ordem = 2,
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        var planoPro = new Plano
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Nome = "Pro",
            Descricao = "Plano profissional para empresas",
            PrecoMensal = 99.90m,
            PrecoAnual = 999.00m,
            TemTrial = true,
            DiasTrialGratuito = 7,
            Recursos = "{\"usuarios\": -1, \"projetos\": -1, \"storage\": \"100GB\"}",
            Ordem = 3,
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        modelBuilder.Entity<Plano>().HasData(planoFree, planoBasic, planoPro);
    }
}
