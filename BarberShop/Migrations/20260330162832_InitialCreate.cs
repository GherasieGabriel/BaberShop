using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BARBER",
                columns: table => new
                {
                    barber_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    first_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    hire_date = table.Column<DateTime>(type: "date", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BARBER", x => x.barber_id);
                });

            migrationBuilder.CreateTable(
                name: "CLIENT",
                columns: table => new
                {
                    client_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    member_since = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLIENT", x => x.client_id);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    stock_quantity = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCT", x => x.product_id);
                });

            migrationBuilder.CreateTable(
                name: "SERVICE",
                columns: table => new
                {
                    service_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    base_duration = table.Column<int>(type: "int", nullable: false),
                    base_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SERVICE", x => x.service_id);
                });

            migrationBuilder.CreateTable(
                name: "CLIENT_NOTIFICATION_SETTINGS",
                columns: table => new
                {
                    client_id = table.Column<int>(type: "int", nullable: false),
                    notify_email = table.Column<bool>(type: "bit", nullable: false),
                    notify_sms = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLIENT_NOTIFICATION_SETTINGS", x => x.client_id);
                    table.ForeignKey(
                        name: "FK_CLIENT_NOTIFICATION_SETTINGS_CLIENT_client_id",
                        column: x => x.client_id,
                        principalTable: "CLIENT",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "APPOINTMENT",
                columns: table => new
                {
                    appointment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    client_id = table.Column<int>(type: "int", nullable: false),
                    barber_id = table.Column<int>(type: "int", nullable: false),
                    service_id = table.Column<int>(type: "int", nullable: false),
                    start_datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APPOINTMENT", x => x.appointment_id);
                    table.ForeignKey(
                        name: "FK_APPOINTMENT_BARBER_barber_id",
                        column: x => x.barber_id,
                        principalTable: "BARBER",
                        principalColumn: "barber_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APPOINTMENT_CLIENT_client_id",
                        column: x => x.client_id,
                        principalTable: "CLIENT",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_APPOINTMENT_SERVICE_service_id",
                        column: x => x.service_id,
                        principalTable: "SERVICE",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PAYMENT",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    appointment_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    transaction_ref = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENT", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_PAYMENT_APPOINTMENT_appointment_id",
                        column: x => x.appointment_id,
                        principalTable: "APPOINTMENT",
                        principalColumn: "appointment_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REVIEW",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    appointment_id = table.Column<int>(type: "int", nullable: false),
                    client_id = table.Column<int>(type: "int", nullable: false),
                    barber_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_public = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REVIEW", x => x.review_id);
                    table.ForeignKey(
                        name: "FK_REVIEW_APPOINTMENT_appointment_id",
                        column: x => x.appointment_id,
                        principalTable: "APPOINTMENT",
                        principalColumn: "appointment_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REVIEW_BARBER_barber_id",
                        column: x => x.barber_id,
                        principalTable: "BARBER",
                        principalColumn: "barber_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REVIEW_CLIENT_client_id",
                        column: x => x.client_id,
                        principalTable: "CLIENT",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APPOINTMENT_barber_id",
                table: "APPOINTMENT",
                column: "barber_id");

            migrationBuilder.CreateIndex(
                name: "IX_APPOINTMENT_client_id",
                table: "APPOINTMENT",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_APPOINTMENT_service_id",
                table: "APPOINTMENT",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_appointment_id",
                table: "PAYMENT",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_REVIEW_appointment_id",
                table: "REVIEW",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_REVIEW_barber_id",
                table: "REVIEW",
                column: "barber_id");

            migrationBuilder.CreateIndex(
                name: "IX_REVIEW_client_id",
                table: "REVIEW",
                column: "client_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CLIENT_NOTIFICATION_SETTINGS");

            migrationBuilder.DropTable(
                name: "PAYMENT");

            migrationBuilder.DropTable(
                name: "PRODUCT");

            migrationBuilder.DropTable(
                name: "REVIEW");

            migrationBuilder.DropTable(
                name: "APPOINTMENT");

            migrationBuilder.DropTable(
                name: "BARBER");

            migrationBuilder.DropTable(
                name: "CLIENT");

            migrationBuilder.DropTable(
                name: "SERVICE");
        }
    }
}
