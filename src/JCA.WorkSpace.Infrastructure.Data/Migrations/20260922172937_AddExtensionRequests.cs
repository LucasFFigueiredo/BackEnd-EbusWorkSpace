using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCA.WorkSpace.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExtensionRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:access_request_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:extension_request_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:reservation_status", "pending,checked_in,canceled,no_show,completed,awaiting_approval")
                .Annotation("Npgsql:Enum:space_type", "desk,room")
                .Annotation("Npgsql:Enum:user_profile", "employee,manager,facilities,admin")
                .OldAnnotation("Npgsql:Enum:access_request_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:reservation_status", "pending,checked_in,canceled,no_show,completed,awaiting_approval")
                .OldAnnotation("Npgsql:Enum:space_type", "desk,room")
                .OldAnnotation("Npgsql:Enum:user_profile", "employee,manager,facilities,admin");

            migrationBuilder.CreateTable(
                name: "ExtensionRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedMinutes = table.Column<int>(type: "integer", nullable: false),
                    Justification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtensionRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtensionRequests_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtensionRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_extensionrequests_reservationid",
                table: "ExtensionRequests",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "idx_extensionrequests_status",
                table: "ExtensionRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExtensionRequests_UserId",
                table: "ExtensionRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtensionRequests");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:access_request_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:reservation_status", "pending,checked_in,canceled,no_show,completed,awaiting_approval")
                .Annotation("Npgsql:Enum:space_type", "desk,room")
                .Annotation("Npgsql:Enum:user_profile", "employee,manager,facilities,admin")
                .OldAnnotation("Npgsql:Enum:access_request_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:extension_request_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:reservation_status", "pending,checked_in,canceled,no_show,completed,awaiting_approval")
                .OldAnnotation("Npgsql:Enum:space_type", "desk,room")
                .OldAnnotation("Npgsql:Enum:user_profile", "employee,manager,facilities,admin");
        }
    }
}
