using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DesafioConcilig1.Migrations
{
    /// <inheritdoc />
    public partial class PopulandoTabelaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "NomeUsuario", "Senha" },
                values: new object[] { 1, "admin", "123456" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
