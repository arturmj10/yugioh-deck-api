using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YugiohDeck.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDonoDoDeck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Decks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Decks");
        }
    }
}
