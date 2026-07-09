using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.Migrations
{
    /// <inheritdoc />
    public partial class MergedVersionFull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration is empty - all changes are handled by AddOrdersAndOrderItems and AddBarberPhoto
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration is empty - all changes are handled by AddOrdersAndOrderItems and AddBarberPhoto
        }
    }
}
