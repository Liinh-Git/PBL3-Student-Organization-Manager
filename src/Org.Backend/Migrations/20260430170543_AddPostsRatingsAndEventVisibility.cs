using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPostsRatingsAndEventVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 1); // 1 = MembersOnly

            migrationBuilder.CreateTable(
                name: "EventRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Aspect = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRatings_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationPosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    PostType = table.Column<int>(type: "integer", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    TargetDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationPosts_Departments_TargetDepartmentId",
                        column: x => x.TargetDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrganizationPosts_Events_RelatedEventId",
                        column: x => x.RelatedEventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrganizationPosts_Members_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationPosts_Organizations_OrgId",
                        column: x => x.OrgId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRatings_EventId_UserId_Aspect",
                table: "EventRatings",
                columns: new[] { "EventId", "UserId", "Aspect" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventRatings_UserId",
                table: "EventRatings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPosts_CreatedBy",
                table: "OrganizationPosts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPosts_OrgId_CreatedAt",
                table: "OrganizationPosts",
                columns: new[] { "OrgId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPosts_RelatedEventId",
                table: "OrganizationPosts",
                column: "RelatedEventId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPosts_TargetDepartmentId",
                table: "OrganizationPosts",
                column: "TargetDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPosts_Visibility_CreatedAt",
                table: "OrganizationPosts",
                columns: new[] { "Visibility", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRatings");

            migrationBuilder.DropTable(
                name: "OrganizationPosts");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Events");
        }
    }
}
