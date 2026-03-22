using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursilloWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddHtmlTestColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HtmlTest",
                table: "Tests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HtmlTest",
                table: "Tests");
        }
    }
}
