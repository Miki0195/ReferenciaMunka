using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELTE.TravelAgency.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AccessToken",
                table: "AspNetUsers",
                column: "AccessToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AccessToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "AspNetUsers");
        }
    }
}
