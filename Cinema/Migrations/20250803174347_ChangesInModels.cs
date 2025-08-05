using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSeats",
                table: "Projections");

            migrationBuilder.RenameColumn(
                name: "SeatNumber",
                table: "Tickets",
                newName: "SeatRow");

            migrationBuilder.RenameColumn(
                name: "SeatCount",
                table: "Halls",
                newName: "Rows");

            migrationBuilder.AddColumn<int>(
                name: "SeatColumn",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Columns",
                table: "Halls",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeatColumn",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Columns",
                table: "Halls");

            migrationBuilder.RenameColumn(
                name: "SeatRow",
                table: "Tickets",
                newName: "SeatNumber");

            migrationBuilder.RenameColumn(
                name: "Rows",
                table: "Halls",
                newName: "SeatCount");

            migrationBuilder.AddColumn<int>(
                name: "AvailableSeats",
                table: "Projections",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
