using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Halls",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Projections",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Projections",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Halls",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Halls",
                keyColumn: "Id",
                keyValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Halls",
                columns: new[] { "Id", "Name", "SeatCount" },
                values: new object[,]
                {
                    { 1, "Main Hall", 150 },
                    { 2, "Big Hall", 200 },
                    { 3, "Small Hall", 80 }
                });

            migrationBuilder.InsertData(
                table: "Projections",
                columns: new[] { "Id", "HallId", "MovieId", "ProjectionTime" },
                values: new object[,]
                {
                    { 1, 1, 3, new DateTime(2025, 7, 17, 18, 0, 0, 0, DateTimeKind.Local) },
                    { 2, 2, 4, new DateTime(2025, 7, 17, 20, 0, 0, 0, DateTimeKind.Local) }
                });
        }
    }
}
