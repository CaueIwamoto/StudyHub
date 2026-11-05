using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StudyHub.Infrastructure.Persistence;

#nullable disable

namespace StudyHub.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(StudyHubDbContext))]
    [Migration("20261105150000_AdicionarPrazoEntregaProposta")]
    public class AdicionarPrazoEntregaProposta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PrazoEntrega",
                table: "Propostas",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PrazoEntrega", table: "Propostas");
        }
    }
}
