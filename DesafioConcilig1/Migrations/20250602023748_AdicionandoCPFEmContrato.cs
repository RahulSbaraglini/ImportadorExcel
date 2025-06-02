using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DesafioConcilig1.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoCPFEmContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CPF",
                table: "Contratos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CPF",
                table: "Contratos");
        }
    }
}
