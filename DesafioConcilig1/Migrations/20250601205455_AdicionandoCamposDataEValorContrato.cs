using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DesafioConcilig1.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoCamposDataEValorContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Valor",
                table: "Contratos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "Vencimento",
                table: "Contratos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Valor",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "Vencimento",
                table: "Contratos");
        }
    }
}
