using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCarManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AccountIdentityAndSupportRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverLicenseNumber",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "NationalIdNumber",
                table: "Users",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupportRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_NationalIdNumber",
                table: "Users",
                column: "NationalIdNumber",
                unique: true,
                filter: "[NationalIdNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportRequests");

            migrationBuilder.DropIndex(
                name: "IX_Users_NationalIdNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NationalIdNumber",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "DriverLicenseNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
