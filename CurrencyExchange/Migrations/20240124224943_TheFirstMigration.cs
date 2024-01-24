using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CurrencyExchange.Migrations
{
    /// <inheritdoc />
    public partial class TheFirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseCurrency = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExchangeCurrency = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Rate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RateProvider = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ExchangeRateId = table.Column<int>(type: "int", nullable: false),
                    TimestampUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaseCurrencyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExchangedCurrencyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trades_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trades_ExchangeRates_ExchangeRateId",
                        column: x => x.ExchangeRateId,
                        principalTable: "ExchangeRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_BaseCurrency",
                table: "ExchangeRates",
                column: "BaseCurrency");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_ExchangeCurrency",
                table: "ExchangeRates",
                column: "ExchangeCurrency");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_LastUpdatedUTC",
                table: "ExchangeRates",
                column: "LastUpdatedUTC");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_ClientId",
                table: "Trades",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_ExchangeRateId",
                table: "Trades",
                column: "ExchangeRateId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_TimestampUTC",
                table: "Trades",
                column: "TimestampUTC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "ExchangeRates");
        }
    }
}
