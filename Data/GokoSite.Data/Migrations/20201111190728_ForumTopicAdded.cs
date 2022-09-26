using Microsoft.EntityFrameworkCore.Migrations;

namespace GokoSite.Data.Migrations
{
    public partial class ForumTopicAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ForumTopic",
                table: "Forums",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForumTopic",
                table: "Forums");
        }
    }
}
