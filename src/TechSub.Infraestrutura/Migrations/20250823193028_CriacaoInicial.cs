using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechSub.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Planos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrecoMensal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PrecoAnual = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TemTrial = table.Column<bool>(type: "boolean", nullable: false),
                    DiasTrialGratuito = table.Column<int>(type: "integer", nullable: false),
                    Recursos = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assinaturas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Periodicidade = table.Column<int>(type: "integer", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataTermino = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataProximaCobranca = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmTrial = table.Column<bool>(type: "boolean", nullable: false),
                    DataTerminoTrial = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assinaturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assinaturas_Planos_PlanoId",
                        column: x => x.PlanoId,
                        principalTable: "Planos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssinaturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataProcessamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MetodoPagamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TransacaoId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MensagemErro = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TentativasCobranca = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagamentos_Assinaturas_AssinaturaId",
                        column: x => x.AssinaturaId,
                        principalTable: "Assinaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    HashSenha = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProvedorId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Provedor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    AssinaturaAtivaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Assinaturas_AssinaturaAtivaId",
                        column: x => x.AssinaturaAtivaId,
                        principalTable: "Assinaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Planos",
                columns: new[] { "Id", "Ativo", "DataAtualizacao", "DataCriacao", "Descricao", "DiasTrialGratuito", "Nome", "Ordem", "PrecoAnual", "PrecoMensal", "Recursos", "TemTrial" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), true, new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7290), new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7290), "Plano gratuito com recursos básicos", 0, "Free", 1, 0m, 0m, "{\"usuarios\": 1, \"projetos\": 1, \"storage\": \"100MB\"}", false },
                    { new Guid("22222222-2222-2222-2222-222222222222"), true, new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7297), new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7296), "Plano básico para pequenas equipes", 7, "Basic", 2, 299.00m, 29.90m, "{\"usuarios\": 5, \"projetos\": 10, \"storage\": \"10GB\"}", true },
                    { new Guid("33333333-3333-3333-3333-333333333333"), true, new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7300), new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7300), "Plano profissional para empresas", 7, "Pro", 3, 999.00m, 99.90m, "{\"usuarios\": -1, \"projetos\": -1, \"storage\": \"100GB\"}", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_PlanoId",
                table: "Assinaturas",
                column: "PlanoId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_UsuarioId_Status",
                table: "Assinaturas",
                columns: new[] { "UsuarioId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_AssinaturaId",
                table: "Pagamentos",
                column: "AssinaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_DataVencimento",
                table: "Pagamentos",
                column: "DataVencimento");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_Status",
                table: "Pagamentos",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Planos_Nome",
                table: "Planos",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AssinaturaAtivaId",
                table: "Usuarios",
                column: "AssinaturaAtivaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assinaturas_Usuarios_UsuarioId",
                table: "Assinaturas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assinaturas_Planos_PlanoId",
                table: "Assinaturas");

            migrationBuilder.DropForeignKey(
                name: "FK_Assinaturas_Usuarios_UsuarioId",
                table: "Assinaturas");

            migrationBuilder.DropTable(
                name: "Pagamentos");

            migrationBuilder.DropTable(
                name: "Planos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Assinaturas");
        }
    }
}
