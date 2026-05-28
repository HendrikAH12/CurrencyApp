using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CurrencyApp.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddExchangeRateCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExchangeRateCaches",
                columns: table => new
                {
                    FromCode = table.Column<string>(type: "TEXT", unicode: false, maxLength: 3, nullable: false),
                    ToCode = table.Column<string>(type: "TEXT", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 8, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRateCaches", x => new { x.FromCode, x.ToCode });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRateCaches");
        }
    }
}
