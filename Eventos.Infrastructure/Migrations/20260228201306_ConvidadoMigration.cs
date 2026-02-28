using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvidadoMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Convidado",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PresencaConfirmada = table.Column<bool>(type: "bit", nullable: false),
                    Participacao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuantidadeAcompanhantes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Convidado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Acompanhante",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConvidadoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acompanhante", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Acompanhante_Convidado_ConvidadoId",
                        column: x => x.ConvidadoId,
                        principalTable: "Convidado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acompanhante_ConvidadoId",
                table: "Acompanhante",
                column: "ConvidadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Acompanhante");

            migrationBuilder.DropTable(
                name: "Convidado");
        }
    }
}
