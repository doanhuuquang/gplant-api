using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gplant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLightningSaleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LightningSales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LightningSales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LightningSaleItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LightningSaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlantVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    QuantityLimit = table.Column<int>(type: "int", nullable: false),
                    QuantitySold = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LightningSaleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LightningSaleItems_LightningSales_LightningSaleId",
                        column: x => x.LightningSaleId,
                        principalTable: "LightningSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LightningSaleItems_PlantVariants_PlantVariantId",
                        column: x => x.PlantVariantId,
                        principalTable: "PlantVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LightningSaleItems_IsActive_QuantitySold_QuantityLimit",
                table: "LightningSaleItems",
                columns: new[] { "IsActive", "QuantitySold", "QuantityLimit" });

            migrationBuilder.CreateIndex(
                name: "IX_LightningSaleItems_LightningSaleId",
                table: "LightningSaleItems",
                column: "LightningSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_LightningSaleItems_LightningSaleId_PlantVariantId",
                table: "LightningSaleItems",
                columns: new[] { "LightningSaleId", "PlantVariantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LightningSaleItems_PlantVariantId",
                table: "LightningSaleItems",
                column: "PlantVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_LightningSales_EndDateUtc",
                table: "LightningSales",
                column: "EndDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_LightningSales_IsActive",
                table: "LightningSales",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LightningSales_IsActive_StartDateUtc_EndDateUtc",
                table: "LightningSales",
                columns: new[] { "IsActive", "StartDateUtc", "EndDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LightningSales_StartDateUtc",
                table: "LightningSales",
                column: "StartDateUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LightningSaleItems");

            migrationBuilder.DropTable(
                name: "LightningSales");
        }
    }
}
