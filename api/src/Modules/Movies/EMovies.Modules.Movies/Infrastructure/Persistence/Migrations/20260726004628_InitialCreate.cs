using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMovies.Modules.Movies.Infrastructure.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "movies");

        migrationBuilder.CreateTable(
            name: "movies",
            schema: "movies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false),
                Description = table.Column<string>(
                    type: "character varying(2000)",
                    maxLength: 2000,
                    nullable: true),
                ReleaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                Genre = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),
                CreatedAtUtc = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_movies", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_movies_Title",
            schema: "movies",
            table: "movies",
            column: "Title");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "movies",
            schema: "movies");
    }
}
