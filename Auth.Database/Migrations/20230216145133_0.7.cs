using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Database.Migrations
{
    /// <inheritdoc />
    public partial class _07 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NameEnum",
                table: "ActiveLicenses",
                newName: "NameEnum");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ActiveLicenses",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NameEnum",
                table: "ActiveLicenses",
                newName: "NameEnum");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ActiveLicenses",
                newName: "Name");
        }
    }
}
