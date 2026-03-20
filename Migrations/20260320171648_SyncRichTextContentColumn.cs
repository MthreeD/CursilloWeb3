using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursilloWeb.Migrations
{
    /// <inheritdoc />
    public partial class SyncRichTextContentColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RichTextContent column already exists in database
            // This migration syncs the model with the existing schema
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RichTextContent",
                table: "ContentBlocks");
        }
    }
}
