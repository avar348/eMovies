using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMovies.Modules.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerOnboardingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                schema: "users",
                table: "user_profiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                schema: "users",
                table: "user_profiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "users",
                table: "user_profiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                schema: "users",
                table: "user_profiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationName",
                schema: "users",
                table: "user_profiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "users",
                table: "user_profiles",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "users",
                table: "user_profiles",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceAreaCoverage",
                schema: "users",
                table: "user_profiles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceAreaMiles",
                schema: "users",
                table: "user_profiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateRegion",
                schema: "users",
                table: "user_profiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine1",
                schema: "users",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                schema: "users",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "users",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "Country",
                schema: "users",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "OrganizationName",
                schema: "users",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "users",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "users",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "ServiceAreaCoverage",
                schema: "users",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "ServiceAreaMiles",
                schema: "users",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "StateRegion",
                schema: "users",
                table: "user_profiles");
        }
    }
}
