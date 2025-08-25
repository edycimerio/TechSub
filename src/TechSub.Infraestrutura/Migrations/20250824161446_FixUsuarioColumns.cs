using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechSub.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class FixUsuarioColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HashSenha",
                table: "Usuarios",
                newName: "SenhaHash");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Usuarios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataUltimoLogin",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Usuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotivoFalha",
                table: "Pagamentos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataUltimoLogin",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MotivoFalha",
                table: "Pagamentos");

            migrationBuilder.RenameColumn(
                name: "SenhaHash",
                table: "Usuarios",
                newName: "HashSenha");

            migrationBuilder.UpdateData(
                table: "Planos",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "DataAtualizacao", "DataCriacao" },
                values: new object[] { new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7290), new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7290) });

            migrationBuilder.UpdateData(
                table: "Planos",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "DataAtualizacao", "DataCriacao" },
                values: new object[] { new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7297), new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7296) });

            migrationBuilder.UpdateData(
                table: "Planos",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "DataAtualizacao", "DataCriacao" },
                values: new object[] { new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7300), new DateTime(2025, 8, 23, 19, 30, 27, 931, DateTimeKind.Utc).AddTicks(7300) });
        }
    }
}
