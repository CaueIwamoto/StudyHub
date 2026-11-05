using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StudyHub.Infrastructure.Persistence;

#nullable disable

namespace StudyHub.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(StudyHubDbContext))]
    [Migration("20261105170000_AdicionarStatusCanceladaProposta")]
    public class AdicionarStatusCanceladaProposta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Propostas\" DROP CONSTRAINT \"CK_Propostas_Status\";");
            migrationBuilder.Sql("ALTER TABLE \"Propostas\" ADD CONSTRAINT \"CK_Propostas_Status\" CHECK (\"Status\" IN (1, 2, 3, 4, 5));");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback, Cancelada é convertida para NaoSelecionada, que também representa um estado inativo.
            migrationBuilder.Sql("UPDATE \"Propostas\" SET \"Status\" = 4 WHERE \"Status\" = 5;");
            migrationBuilder.Sql("ALTER TABLE \"Propostas\" DROP CONSTRAINT \"CK_Propostas_Status\";");
            migrationBuilder.Sql("ALTER TABLE \"Propostas\" ADD CONSTRAINT \"CK_Propostas_Status\" CHECK (\"Status\" IN (1, 2, 3, 4));");
        }
    }
}
