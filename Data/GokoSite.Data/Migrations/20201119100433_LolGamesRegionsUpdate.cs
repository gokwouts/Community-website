using Microsoft.EntityFrameworkCore.Migrations;

namespace GokoSite.Data.Migrations
{
    public partial class LolGamesRegionsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegionId",
                table: "Games",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    RegionId = table.Column<string>(nullable: false),
                    RegionName = table.Column<string>(nullable: true),
                    RiotRegionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.RegionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_RegionId",
                table: "Games",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Regions_RegionId",
                table: "Games",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "RegionId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Regions_RegionId",
                table: "Games");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_Games_RegionId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Games");
        }
    }
}
