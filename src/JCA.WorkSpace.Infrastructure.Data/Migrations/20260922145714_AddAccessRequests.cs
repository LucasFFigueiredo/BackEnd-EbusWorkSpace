using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCA.WorkSpace.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:access_request_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:reservation_status", "pending,checked_in,canceled,no_show,completed,awaiting_approval")
                .Annotation("Npgsql:Enum:space_type", "desk,room")
                .Annotation("Npgsql:Enum:user_profile", "employee,manager,facilities,admin")
                .OldAnnotation("Npgsql:Enum:reservation_status", "pending,checked_in,canceled,no_show,completed,awaiting_approval")
                .OldAnnotation("Npgsql:Enum:space_type", "desk,room")
                .OldAnnotation("Npgsql:Enum:user_profile", "employee,manager,facilities,admin");

            migrationBuilder.CreateTable(
                name: "AccessRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedProfile = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_accessrequests_status",
                table: "AccessRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_accessrequests_userid",
                table: "AccessRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessRequests");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:reservation_status", "pending,checked_in,canceled,no_show,completed,awaiting_approval")
                .Annotation("Npgsql:Enum:space_type", "desk,room")
                .Annotation("Npgsql:Enum:user_profile", "employee,manager,facilities,admin")
                .OldAnnotation("Npgsql:Enum:access_request_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:reservation_status", "pending,checked_in,canceled,no_show,completed,awaiting_approval")
                .OldAnnotation("Npgsql:Enum:space_type", "desk,room")
                .OldAnnotation("Npgsql:Enum:user_profile", "employee,manager,facilities,admin");
        }
    }
}
