using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCA.WorkSpace.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessAndExtensionRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtensionRequests_Users_UserId",
                table: "ExtensionRequests");

            migrationBuilder.DropIndex(
                name: "idx_extensionrequests_status",
                table: "ExtensionRequests");

            migrationBuilder.DropIndex(
                name: "idx_accessrequests_status",
                table: "AccessRequests");

            migrationBuilder.RenameIndex(
                name: "idx_extensionrequests_reservationid",
                table: "ExtensionRequests",
                newName: "IX_ExtensionRequests_ReservationId");

            migrationBuilder.RenameIndex(
                name: "idx_accessrequests_userid",
                table: "AccessRequests",
                newName: "IX_AccessRequests_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtensionRequests_Users_UserId",
                table: "ExtensionRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtensionRequests_Users_UserId",
                table: "ExtensionRequests");

            migrationBuilder.RenameIndex(
                name: "IX_ExtensionRequests_ReservationId",
                table: "ExtensionRequests",
                newName: "idx_extensionrequests_reservationid");

            migrationBuilder.RenameIndex(
                name: "IX_AccessRequests_UserId",
                table: "AccessRequests",
                newName: "idx_accessrequests_userid");

            migrationBuilder.CreateIndex(
                name: "idx_extensionrequests_status",
                table: "ExtensionRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_accessrequests_status",
                table: "AccessRequests",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtensionRequests_Users_UserId",
                table: "ExtensionRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
