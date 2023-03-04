using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Database.Migrations
{
    /// <inheritdoc />
    public partial class _02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "ActiveLicenses",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveLicenses_OrderId",
                table: "ActiveLicenses",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveLicenses_Order_OrderId",
                table: "ActiveLicenses",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActiveLicenses_Order_OrderId",
                table: "ActiveLicenses");

            migrationBuilder.DropIndex(
                name: "IX_ActiveLicenses_OrderId",
                table: "ActiveLicenses");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "ActiveLicenses");
        }
    }
}
