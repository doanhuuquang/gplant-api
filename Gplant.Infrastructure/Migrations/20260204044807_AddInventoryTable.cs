using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gplant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlantVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    QuantityReserved = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventories_PlantVariants_PlantVariantId",
                        column: x => x.PlantVariantId,
                        principalTable: "PlantVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_PlantVariantId",
                table: "Inventories",
                column: "PlantVariantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_QuantityAvailable",
                table: "Inventories",
                column: "QuantityAvailable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventories");
        }
    }
}
