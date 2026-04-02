using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursilloWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenameHtmlTestColumnToHTMLText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FontName",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "FontSize",
                table: "Tests");

            // Check if HtmlTest column exists before renaming
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tests]') AND name = 'HtmlTest')
                BEGIN
                    EXEC sp_rename N'[dbo].[Tests].[HtmlTest]', N'HTMLText', N'COLUMN';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("EXEC sp_rename N'[dbo].[Tests].[HTMLText]', N'HtmlTest', N'COLUMN';");

            migrationBuilder.AddColumn<string>(
                name: "FontName",
                table: "Tests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FontSize",
                table: "Tests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
