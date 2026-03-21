using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Convidado_Nome",
                table: "Convidado",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_Convidado_PresencaConfirmada",
                table: "Convidado",
                column: "PresencaConfirmada");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Convidado_Nome",
                table: "Convidado");

            migrationBuilder.DropIndex(
                name: "IX_Convidado_PresencaConfirmada",
                table: "Convidado");
        }
    }
}
