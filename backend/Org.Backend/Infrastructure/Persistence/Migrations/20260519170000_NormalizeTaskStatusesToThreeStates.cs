using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Org.Backend.Infrastructure.Persistence;

#nullable disable

namespace Org.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260519170000_NormalizeTaskStatusesToThreeStates")]
    public partial class NormalizeTaskStatusesToThreeStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "OrgTasks"
                SET "Status" = 'InProgress'
                WHERE "Status" = 'Blocked';

                UPDATE "OrgTasks"
                SET "Status" = 'Todo'
                WHERE "Status" = 'Cancelled';
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "OrgTasks"
                SET "Status" = 'Blocked'
                WHERE "Status" = 'InProgress'
                  AND "CompletedAt" IS NULL;

                UPDATE "OrgTasks"
                SET "Status" = 'Cancelled'
                WHERE "Status" = 'Todo'
                  AND "IsDeleted" = TRUE;
                """
            );
        }
    }
}
