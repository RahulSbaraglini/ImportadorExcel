using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DesafioConcilig1.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaProdutoEContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contrato",
                table: "Contratos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Produto",
                table: "Contratos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contrato",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "Produto",
                table: "Contratos");
        }
    }
}
