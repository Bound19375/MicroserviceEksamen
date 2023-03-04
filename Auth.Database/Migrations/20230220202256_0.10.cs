using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Database.Migrations
{
    /// <inheritdoc />
    public partial class _010 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Order",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "NameEnum",
                table: "ActiveLicenses",
                newName: "ProductNameEnum");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ActiveLicenses",
                newName: "ProductName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "Order",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ProductNameEnum",
                table: "ActiveLicenses",
                newName: "NameEnum");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "ActiveLicenses",
                newName: "Name");
        }
    }
}
