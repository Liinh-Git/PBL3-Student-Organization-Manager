using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Backend.Migrations;

public partial class AddEventCategoryHierarchyPlaceholder : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // TODO(BE-DAY1): Create EventCategories table with self-reference ParentCategoryId.
        // TODO(BE-DAY1): Add Milestones table and FK from EventCategories -> Milestones.
        // TODO(BE-DAY1): Add Tasks table with FK to EventCategories.
        // TODO(BE-DAY1): Add indexes required for GET list endpoint performance.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // TODO(BE-DAY1): Drop Tasks, EventCategories, Milestones in reverse order.
    }
}
