using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursilloWeb.Migrations
{
    /// <inheritdoc />
    public partial class ClearRTFContentForProperRTFFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear existing RTFContent since we're changing from JSON to actual RTF format
            migrationBuilder.Sql("UPDATE ContentBlocks SET RTFContent = NULL WHERE RTFContent IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cannot restore the previous JSON data
        }
    }
}
