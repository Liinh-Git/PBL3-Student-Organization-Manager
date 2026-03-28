using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddConstraintsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roles_OrgId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_EventMembers_EventId",
                table: "EventMembers");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_OrgId_RoleName",
                table: "Roles",
                columns: new[] { "OrgId", "RoleName" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CHK_Resource_Quantity",
                table: "Resources",
                sql: "\"Quantity\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_EventMembers_EventId_MemberId",
                table: "EventMembers",
                columns: new[] { "EventId", "MemberId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roles_OrgId_RoleName",
                table: "Roles");

            migrationBuilder.DropCheckConstraint(
                name: "CHK_Resource_Quantity",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_EventMembers_EventId_MemberId",
                table: "EventMembers");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_OrgId",
                table: "Roles",
                column: "OrgId");

            migrationBuilder.CreateIndex(
                name: "IX_EventMembers_EventId",
                table: "EventMembers",
                column: "EventId");
        }
    }
}
