using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mastermindAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SecretCode = table.Column<string>(type: "TEXT", nullable: false),
                    Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameGuesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuessCode = table.Column<string>(type: "TEXT", nullable: false),
                    ExactMatches = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionMatches = table.Column<int>(type: "INTEGER", nullable: false),
                    NoMatches = table.Column<int>(type: "INTEGER", nullable: false),
                    AttemptNumber = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameGuesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameGuesses_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameGuesses_GameId",
                table: "GameGuesses",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameGuesses");

            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
