using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zubs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PatientBirthDateFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DentalChart_Patients_PatientId",
                table: "DentalChart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DentalChart",
                table: "DentalChart");

            migrationBuilder.RenameTable(
                name: "DentalChart",
                newName: "DentalCharts");

            migrationBuilder.RenameIndex(
                name: "IX_DentalChart_PatientId",
                table: "DentalCharts",
                newName: "IX_DentalCharts_PatientId");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "BirthDate",
                table: "Patients",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DentalCharts",
                table: "DentalCharts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DentalCharts_Patients_PatientId",
                table: "DentalCharts",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DentalCharts_Patients_PatientId",
                table: "DentalCharts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DentalCharts",
                table: "DentalCharts");

            migrationBuilder.RenameTable(
                name: "DentalCharts",
                newName: "DentalChart");

            migrationBuilder.RenameIndex(
                name: "IX_DentalCharts_PatientId",
                table: "DentalChart",
                newName: "IX_DentalChart_PatientId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Patients",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DentalChart",
                table: "DentalChart",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DentalChart_Patients_PatientId",
                table: "DentalChart",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
