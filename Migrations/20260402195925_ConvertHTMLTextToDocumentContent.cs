using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursilloWeb.Migrations
{
    /// <inheritdoc />
    public partial class ConvertHTMLTextToDocumentContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new column first
            migrationBuilder.AddColumn<byte[]>(
                name: "DocumentContent",
                table: "Tests",
                type: "varbinary(max)",
                nullable: true);

            // Convert existing HTML to RTF (basic conversion)
            // Note: This creates a simple RTF document with the HTML as plain text
            migrationBuilder.Sql(@"
                UPDATE Tests 
                SET DocumentContent = CASE 
                    WHEN HTMLText IS NOT NULL THEN 
                        CAST(N'{\rtf1\ansi\deff0 {\fonttbl {\f0 Segoe UI Light;}}' + CHAR(10) + 
                             N'\f0\fs24 ' + REPLACE(REPLACE(HTMLText, '<', ''), '>', '') + N'}' AS varbinary(max))
                    ELSE NULL
                END
                WHERE HTMLText IS NOT NULL;
            ");

            // Now drop the old column
            migrationBuilder.DropColumn(
                name: "HTMLText",
                table: "Tests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentContent",
                table: "Tests");

            migrationBuilder.AddColumn<string>(
                name: "HTMLText",
                table: "Tests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
