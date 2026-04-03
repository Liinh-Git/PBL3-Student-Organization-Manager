using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEventCategoryParentHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentCategoryId",
                table: "EventCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventCategories_ParentCategoryId",
                table: "EventCategories",
                column: "ParentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventCategories_EventCategories_ParentCategoryId",
                table: "EventCategories",
                column: "ParentCategoryId",
                principalTable: "EventCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventCategories_EventCategories_ParentCategoryId",
                table: "EventCategories");

            migrationBuilder.DropIndex(
                name: "IX_EventCategories_ParentCategoryId",
                table: "EventCategories");

            migrationBuilder.DropColumn(
                name: "ParentCategoryId",
                table: "EventCategories");
        }
    }
}
