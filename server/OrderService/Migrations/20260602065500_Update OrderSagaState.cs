using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderSagaState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentExpiryTokenId",
                table: "OrderSagaStates",
                newName: "OnlinePaymentExpiryTokenId");

            migrationBuilder.AddColumn<Guid>(
                name: "CodPaymentExpiryTokenId",
                table: "OrderSagaStates",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodPaymentExpiryTokenId",
                table: "OrderSagaStates");

            migrationBuilder.RenameColumn(
                name: "OnlinePaymentExpiryTokenId",
                table: "OrderSagaStates",
                newName: "PaymentExpiryTokenId");
        }
    }
}
