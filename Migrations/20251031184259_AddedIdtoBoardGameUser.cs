using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBB.Migrations
{
    /// <inheritdoc />
    public partial class AddedIdtoBoardGameUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BoardGameUsers",
                table: "BoardGameUsers");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "BoardGameUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BoardGameUsers",
                table: "BoardGameUsers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BoardGameUsers_BoardGameId",
                table: "BoardGameUsers",
                column: "BoardGameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BoardGameUsers",
                table: "BoardGameUsers");

            migrationBuilder.DropIndex(
                name: "IX_BoardGameUsers_BoardGameId",
                table: "BoardGameUsers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "BoardGameUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BoardGameUsers",
                table: "BoardGameUsers",
                columns: new[] { "BoardGameId", "UserId" });
        }
    }
}
