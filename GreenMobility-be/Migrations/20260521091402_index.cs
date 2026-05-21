using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenMobility_be.Migrations
{
    /// <inheritdoc />
    public partial class index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_HubId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_RentalCode",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_UserId",
                table: "Rentals");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Hub_Filter",
                table: "Vehicles",
                columns: new[] { "HubId", "IsDeleted", "VehicleStatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_UIC",
                table: "Vehicles",
                column: "UIC",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Vehicles_ApiKey",
                table: "Vehicles",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Active_Rentals",
                table: "Rentals",
                column: "UserId",
                filter: "[EndDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_History_Desc",
                table: "Rentals",
                column: "EndDate",
                descending: new bool[0],
                filter: "[EndDate] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_RentalCode",
                table: "Rentals",
                column: "RentalCode",
                unique: true,
                filter: "[RentalCode] IS NOT NULL AND [EndDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Hubs_Active",
                table: "Hubs",
                column: "Name",
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Hub_Filter",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_UIC",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "UX_Vehicles_ApiKey",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "UX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Active_Rentals",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_History_Desc",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "UX_RentalCode",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Hubs_Active",
                table: "Hubs");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_HubId",
                table: "Vehicles",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_RentalCode",
                table: "Rentals",
                column: "RentalCode",
                unique: true,
                filter: "[RentalCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_UserId",
                table: "Rentals",
                column: "UserId");
        }
    }
}
