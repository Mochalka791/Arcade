using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arcade.Data.Migrations
{
    public partial class AddTetrisStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TetrisStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),

                    UserId = table.Column<int>(type: "int", nullable: false),

                    HighScore = table.Column<int>(type: "int", nullable: false, defaultValue: 0),

                    GamesPlayed = table.Column<int>(type: "int", nullable: false, defaultValue: 0),

                    MaxLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TetrisStats", x => x.Id);

                    table.ForeignKey(
                        name: "FK_TetrisStats_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TetrisStats_UserId",
                table: "TetrisStats",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TetrisStats");
        }
    }
}
