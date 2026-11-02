using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarManagement.Api.Data.Migrations
{
    public partial class AssessDamageByAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AssessedAt",
                table: "DamageReports",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AssessedAt", table: "DamageReports");
        }
    }
}
