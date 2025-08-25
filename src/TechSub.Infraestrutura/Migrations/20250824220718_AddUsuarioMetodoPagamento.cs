using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechSub.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioMetodoPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuarioMetodoPagamento",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemMetodoPagamento = table.Column<bool>(type: "boolean", nullable: false),
                    UsuarioId1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioMetodoPagamento", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_UsuarioMetodoPagamento_Usuarios_UsuarioId1",
                        column: x => x.UsuarioId1,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Planos",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "DataAtualizacao", "DataCriacao" },
                values: new object[] { new DateTime(2025, 8, 24, 22, 7, 17, 214, DateTimeKind.Utc).AddTicks(5870), new DateTime(2025, 8, 24, 22, 7, 17, 214, DateTimeKind.Utc).AddTicks(5870) });

            migrationBuilder.UpdateData(
                table: "Planos",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "DataAtualizacao", "DataCriacao" },
                values: new object[] { new DateTime(2025, 8, 24, 22, 7, 17, 214, DateTimeKind.Utc).AddTicks(5877), new DateTime(2025, 8, 24, 22, 7, 17, 214, DateTimeKind.Utc).AddTicks(5877) });

            migrationBuilder.UpdateData(
                table: "Planos",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "DataAtualizacao", "DataCriacao" },
                values: new object[] { new DateTime(2025, 8, 24, 22, 7, 17, 214, DateTimeKind.Utc).AddTicks(5881), new DateTime(2025, 8, 24, 22, 7, 17, 214, DateTimeKind.Utc).AddTicks(5880) });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioMetodoPagamento_UsuarioId1",
                table: "UsuarioMetodoPagamento",
                column: "UsuarioId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuarioMetodoPagamento");

            migrationBuilder.UpdateData(
                table: "Planos",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "DataAtualizacao", "DataCriacao" },
                values: new object[] { new DateTime(2025, 8, 24, 16, 14, 45, 453, DateTimeKind.Utc).AddTicks(9754), new DateTime(2025, 8, 24, 16, 14, 45, 453, DateTimeKind.Utc).AddTicks(9754) });

            migrationBuilder.UpdateData(
                table: "Planos",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "DataAtualizacao", "DataCriacao" },
                values: new object[] { new DateTime(2025, 8, 24, 16, 14, 45, 453, DateTimeKind.Utc).AddTicks(9763), new DateTime(2025, 8, 24, 16, 14, 45, 453, DateTimeKind.Utc).AddTicks(9762) });

            migrationBuilder.UpdateData(
                table: "Planos",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "DataAtualizacao", "DataCriacao" },
                values: new object[] { new DateTime(2025, 8, 24, 16, 14, 45, 453, DateTimeKind.Utc).AddTicks(9767), new DateTime(2025, 8, 24, 16, 14, 45, 453, DateTimeKind.Utc).AddTicks(9767) });
        }
    }
}
