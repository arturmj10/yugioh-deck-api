using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YugiohDeck.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoBanlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BanlistGoat",
                table: "Cards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BanlistOcg",
                table: "Cards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BanlistTcg",
                table: "Cards",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BanlistGoat",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "BanlistOcg",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "BanlistTcg",
                table: "Cards");
        }
    }
}
