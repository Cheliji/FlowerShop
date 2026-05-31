using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FlowerShop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFlowerFieldsAndSeedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Flowers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "Flowers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Flowers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SoldCount",
                table: "Flowers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Flowers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Subtitle",
                table: "Flowers",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SuitableFor",
                table: "Flowers",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Icon", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "🌹", true, "玫瑰", 1 },
                    { 2, "🌷", true, "百合", 2 },
                    { 3, "🌸", true, "康乃馨", 3 },
                    { 4, "🌻", true, "向日葵", 4 },
                    { 5, "💐", true, "混搭花束", 5 },
                    { 6, "🌺", true, "永生花", 6 },
                    { 7, "🌿", true, "绿植", 7 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flowers_Name",
                table: "Flowers",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Flowers_Name",
                table: "Flowers");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Flowers");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "Flowers");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Flowers");

            migrationBuilder.DropColumn(
                name: "SoldCount",
                table: "Flowers");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Flowers");

            migrationBuilder.DropColumn(
                name: "Subtitle",
                table: "Flowers");

            migrationBuilder.DropColumn(
                name: "SuitableFor",
                table: "Flowers");
        }
    }
}
