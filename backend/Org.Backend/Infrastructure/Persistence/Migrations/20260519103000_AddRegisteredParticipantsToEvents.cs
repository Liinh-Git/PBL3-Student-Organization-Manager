using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Org.Backend.Infrastructure.Persistence;

#nullable disable

namespace Org.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260519103000_AddRegisteredParticipantsToEvents")]
    public partial class AddRegisteredParticipantsToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegisteredParticipants",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE "Events" e
                SET "RegisteredParticipants" = COALESCE(a."ParticipantCount", 0)
                FROM (
                    SELECT "EventId", COUNT(*)::int AS "ParticipantCount"
                    FROM "Attendees"
                    WHERE "Status" <> 'Cancelled'
                    GROUP BY "EventId"
                ) a
                WHERE e."Id" = a."EventId";
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegisteredParticipants",
                table: "Events");
        }
    }
}
