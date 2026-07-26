using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMovies.Modules.Reviews.Infrastructure.Persistence.Migrations;

public partial class InitialReviews : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "reviews");

        migrationBuilder.CreateTable(
            name: "reviews",
            schema: "reviews",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false),
                UserDisplayName = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false),
                Rating = table.Column<int>(type: "integer", nullable: false),
                Content = table.Column<string>(
                    type: "character varying(4000)",
                    maxLength: 4000,
                    nullable: true),
                Status = table.Column<string>(
                    type: "character varying(20)",
                    maxLength: 20,
                    nullable: false),
                ModerationReason = table.Column<string>(
                    type: "character varying(1000)",
                    maxLength: 1000,
                    nullable: true),
                CreatedAtUtc = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false),
                ModeratedAtUtc = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_reviews", x => x.Id);
                table.CheckConstraint(
                    "CK_reviews_rating",
                    "\"Rating\" BETWEEN 1 AND 5");
            });

        migrationBuilder.CreateIndex(
            name: "IX_reviews_MovieId_Status",
            schema: "reviews",
            table: "reviews",
            columns: ["MovieId", "Status"]);

        migrationBuilder.CreateIndex(
            name: "IX_reviews_MovieId_UserId",
            schema: "reviews",
            table: "reviews",
            columns: ["MovieId", "UserId"],
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "reviews",
            schema: "reviews");
    }
}
