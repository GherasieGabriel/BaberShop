using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersAndOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_APPOINTMENT_CLIENT_client_id",
                table: "APPOINTMENT");

            migrationBuilder.DropForeignKey(
                name: "FK_CLIENT_NOTIFICATION_SETTINGS_CLIENT_client_id",
                table: "CLIENT_NOTIFICATION_SETTINGS");

            migrationBuilder.DropForeignKey(
                name: "FK_REVIEW_CLIENT_client_id",
                table: "REVIEW");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CLIENT",
                table: "CLIENT");

            migrationBuilder.RenameTable(
                name: "CLIENT",
                newName: "Clients");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Clients",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Clients",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "member_since",
                table: "Clients",
                newName: "MemberSince");

            migrationBuilder.RenameColumn(
                name: "full_name",
                table: "Clients",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "client_id",
                table: "Clients",
                newName: "ClientId");

            migrationBuilder.AddColumn<string>(
                name: "photo_file_name",
                table: "BARBER",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "MemberSince",
                table: "Clients",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clients",
                table: "Clients",
                column: "ClientId");

            migrationBuilder.CreateTable(
                name: "ORDERS",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    application_user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    customer_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    customer_email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDERS", x => x.order_id);
                });

            migrationBuilder.CreateTable(
                name: "ORDER_ITEM",
                columns: table => new
                {
                    order_item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDER_ITEM", x => x.order_item_id);
                    table.ForeignKey(
                        name: "FK_ORDER_ITEM_ORDERS_order_id",
                        column: x => x.order_id,
                        principalTable: "ORDERS",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_ITEM_order_id",
                table: "ORDER_ITEM",
                column: "order_id");

            migrationBuilder.AddForeignKey(
                name: "FK_APPOINTMENT_Clients_client_id",
                table: "APPOINTMENT",
                column: "client_id",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLIENT_NOTIFICATION_SETTINGS_Clients_client_id",
                table: "CLIENT_NOTIFICATION_SETTINGS",
                column: "client_id",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_REVIEW_Clients_client_id",
                table: "REVIEW",
                column: "client_id",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_APPOINTMENT_Clients_client_id",
                table: "APPOINTMENT");

            migrationBuilder.DropForeignKey(
                name: "FK_CLIENT_NOTIFICATION_SETTINGS_Clients_client_id",
                table: "CLIENT_NOTIFICATION_SETTINGS");

            migrationBuilder.DropForeignKey(
                name: "FK_REVIEW_Clients_client_id",
                table: "REVIEW");

            migrationBuilder.DropTable(
                name: "ORDER_ITEM");

            migrationBuilder.DropTable(
                name: "ORDERS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clients",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "photo_file_name",
                table: "BARBER");

            migrationBuilder.RenameTable(
                name: "Clients",
                newName: "CLIENT");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "CLIENT",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "CLIENT",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "MemberSince",
                table: "CLIENT",
                newName: "member_since");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "CLIENT",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "CLIENT",
                newName: "client_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "member_since",
                table: "CLIENT",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CLIENT",
                table: "CLIENT",
                column: "client_id");

            migrationBuilder.AddForeignKey(
                name: "FK_APPOINTMENT_CLIENT_client_id",
                table: "APPOINTMENT",
                column: "client_id",
                principalTable: "CLIENT",
                principalColumn: "client_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CLIENT_NOTIFICATION_SETTINGS_CLIENT_client_id",
                table: "CLIENT_NOTIFICATION_SETTINGS",
                column: "client_id",
                principalTable: "CLIENT",
                principalColumn: "client_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_REVIEW_CLIENT_client_id",
                table: "REVIEW",
                column: "client_id",
                principalTable: "CLIENT",
                principalColumn: "client_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
