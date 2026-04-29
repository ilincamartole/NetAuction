using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Licitatii",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PretPornire = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PretCurent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    data_finalizare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    seller_id = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licitatii", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Bids",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    suma = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    licitatieId = table.Column<int>(type: "int", nullable: false),
                    userId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.id);
                    table.ForeignKey(
                        name: "FK_Bids_Licitatii_licitatieId",
                        column: x => x.licitatieId,
                        principalTable: "Licitatii",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bids_licitatieId",
                table: "Bids",
                column: "licitatieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bids");

            migrationBuilder.DropTable(
                name: "Licitatii");
        }
    }
}
