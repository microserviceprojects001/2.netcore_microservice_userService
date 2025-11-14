using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectMoney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinMoney",
                table: "Projects",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinMoney",
                table: "Projects");
        }
    }
}
