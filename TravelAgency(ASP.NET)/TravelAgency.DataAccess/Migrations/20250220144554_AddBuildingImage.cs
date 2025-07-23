using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELTE.TravelAgency.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    ImageSmall = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ImageLarge = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingImages_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingImages_BuildingId",
                table: "BuildingImages",
                column: "BuildingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingImages");
        }
    }
}
