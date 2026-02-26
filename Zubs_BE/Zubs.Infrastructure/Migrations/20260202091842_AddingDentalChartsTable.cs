using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zubs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingDentalChartsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Services_Code",
                table: "Services",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Services_Code",
                table: "Services");
        }
    }
}
