using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CursilloWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenameTextBoxTextToBackgroundColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mark the AddTextBoxTextColumn migration as applied if not already
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260402191146_AddTextBoxTextColumn')
                BEGIN
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ('20260402191146_AddTextBoxTextColumn', '10.0.5');
                END
            ");

            // Now rename TextBoxText to BackgroundColor
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tests]') AND name = 'TextBoxText')
                BEGIN
                    EXEC sp_rename N'[dbo].[Tests].[TextBoxText]', N'BackgroundColor', N'COLUMN';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tests]') AND name = 'BackgroundColor')
                BEGIN
                    EXEC sp_rename N'[dbo].[Tests].[BackgroundColor]', N'TextBoxText', N'COLUMN';
                END
            ");
        }
    }
}
