using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AliadaLegal.Web.Migrations
{
    /// <inheritdoc />
    public partial class IndiceUnicoCorreoClientesAliados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Correo",
                table: "Clientes",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Aliados_Correo",
                table: "Aliados",
                column: "Correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_Correo",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Aliados_Correo",
                table: "Aliados");
        }
    }
}
