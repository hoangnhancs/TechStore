using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserPhone",
                table: "Orders",
                newName: "RecipientPhone");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Orders",
                newName: "RecipientName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecipientPhone",
                table: "Orders",
                newName: "UserPhone");

            migrationBuilder.RenameColumn(
                name: "RecipientName",
                table: "Orders",
                newName: "UserName");
        }
    }
}
