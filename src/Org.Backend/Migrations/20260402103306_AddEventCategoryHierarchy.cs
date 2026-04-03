using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEventCategoryHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Milestones_MilestoneId",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "MilestoneId",
                table: "Tasks",
                newName: "EventCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_MilestoneId",
                table: "Tasks",
                newName: "IX_Tasks_EventCategoryId");

            migrationBuilder.CreateTable(
                name: "EventCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OwnerDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventCategories_Departments_OwnerDepartmentId",
                        column: x => x.OwnerDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EventCategories_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventCategories_MilestoneId_CategoryName",
                table: "EventCategories",
                columns: new[] { "MilestoneId", "CategoryName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventCategories_MilestoneId_OrderIndex",
                table: "EventCategories",
                columns: new[] { "MilestoneId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_EventCategories_OwnerDepartmentId",
                table: "EventCategories",
                column: "OwnerDepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_EventCategories_EventCategoryId",
                table: "Tasks",
                column: "EventCategoryId",
                principalTable: "EventCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_EventCategories_EventCategoryId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "EventCategories");

            migrationBuilder.RenameColumn(
                name: "EventCategoryId",
                table: "Tasks",
                newName: "MilestoneId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_EventCategoryId",
                table: "Tasks",
                newName: "IX_Tasks_MilestoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Milestones_MilestoneId",
                table: "Tasks",
                column: "MilestoneId",
                principalTable: "Milestones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
