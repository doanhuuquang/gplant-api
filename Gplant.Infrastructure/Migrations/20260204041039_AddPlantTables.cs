using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gplant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CareInstructions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LightRequirement = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WateringFrequency = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Temperature = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Soil = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareInstructions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CareInstructionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plants_CareInstructions_CareInstructionId",
                        column: x => x.CareInstructionId,
                        principalTable: "CareInstructions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plants_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlantImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantImages_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Size = table.Column<float>(type: "real", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantVariants_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantImages_PlantId",
                table: "PlantImages",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantImages_PlantId_IsPrimary",
                table: "PlantImages",
                columns: new[] { "PlantId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_Plants_CareInstructionId",
                table: "Plants",
                column: "CareInstructionId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_CategoryId",
                table: "Plants",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_IsActive",
                table: "Plants",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_Slug",
                table: "Plants",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlantVariants_IsActive",
                table: "PlantVariants",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PlantVariants_PlantId",
                table: "PlantVariants",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantVariants_PlantId_IsActive",
                table: "PlantVariants",
                columns: new[] { "PlantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PlantVariants_SKU",
                table: "PlantVariants",
                column: "SKU",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantImages");

            migrationBuilder.DropTable(
                name: "PlantVariants");

            migrationBuilder.DropTable(
                name: "Plants");

            migrationBuilder.DropTable(
                name: "CareInstructions");
        }
    }
}
