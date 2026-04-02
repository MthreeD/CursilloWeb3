using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursilloWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddTextBoxTextColumnFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, remove the incorrect migration record
            migrationBuilder.Sql("DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260402191146_AddTextBoxTextColumn';");

            // Check if column exists, if not add it
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tests]') AND name = 'TextBoxText')
                BEGIN
                    ALTER TABLE [Tests] ADD [TextBoxText] nvarchar(max) NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextBoxText",
                table: "Tests");
        }
    }
}
