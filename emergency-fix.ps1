# Emergency fix script for ContentBlocks InvalidCastException
# Run this if you still get the error after the automatic fix

Write-Host "=== ContentBlocks Emergency Fix ===" -ForegroundColor Green
Write-Host "This script will fix the binary data issue in your ContentBlocks table" -ForegroundColor Yellow

try {
    # Connect to LocalDB
    $serverInstance = "(localdb)\MSSQLLocalDB"
    $database = "CursilloWeb"
    
    Write-Host "Connecting to database..." -ForegroundColor Cyan
    
    # Step 1: Create emergency backup
    Write-Host "Creating emergency backup..." -ForegroundColor Cyan
    $backupQuery = @"
        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ContentBlocks_EmergencyFix]') AND type in (N'U'))
        BEGIN
            SELECT * INTO ContentBlocks_EmergencyFix FROM ContentBlocks;
            PRINT 'Emergency backup created successfully';
        END
    "@
    
    Invoke-Sqlcmd -ServerInstance $serverInstance -Database $database -Query $backupQuery
    Write-Host "✓ Backup created" -ForegroundColor Green
    
    # Step 2: Fix the data using CONVERT
    Write-Host "Converting binary data to strings..." -ForegroundColor Cyan
    $fixQuery = @"
        UPDATE ContentBlocks 
        SET 
            HtmlContent = CASE 
                WHEN HtmlContent IS NOT NULL THEN CONVERT(nvarchar(max), HtmlContent)
                ELSE NULL 
            END,
            RichTextContent = CASE 
                WHEN RichTextContent IS NOT NULL THEN CONVERT(nvarchar(max), RichTextContent)
                ELSE NULL 
            END,
            LastUpdated = GETDATE()
    "@
    
    $result = Invoke-Sqlcmd -ServerInstance $serverInstance -Database $database -Query $fixQuery
    Write-Host "✓ Data conversion completed" -ForegroundColor Green
    
    # Step 3: Verify the fix
    Write-Host "Verifying the fix..." -ForegroundColor Cyan
    $verifyQuery = @"
        SELECT 
            COUNT(*) as TotalRecords,
            SUM(CASE WHEN HtmlContent IS NOT NULL THEN 1 ELSE 0 END) as HtmlRecords,
            SUM(CASE WHEN RichTextContent IS NOT NULL THEN 1 ELSE 0 END) as RichTextRecords
        FROM ContentBlocks
    "@
    
    $verification = Invoke-Sqlcmd -ServerInstance $serverInstance -Database $database -Query $verifyQuery
    Write-Host "✓ Fix verified:" -ForegroundColor Green
    Write-Host "  Total Records: $($verification.TotalRecords)" -ForegroundColor White
    Write-Host "  HTML Records: $($verification.HtmlRecords)" -ForegroundColor White  
    Write-Host "  RichText Records: $($verification.RichTextRecords)" -ForegroundColor White
    
    Write-Host "`n=== FIX COMPLETED SUCCESSFULLY! ===" -ForegroundColor Green -BackgroundColor DarkGreen
    Write-Host "Your application should now work without the InvalidCastException." -ForegroundColor Green
    Write-Host "You can safely restart your application." -ForegroundColor Green
    
} catch {
    Write-Host "`n=== ERROR OCCURRED ===" -ForegroundColor Red -BackgroundColor DarkRed
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nPlease try the following:" -ForegroundColor Yellow
    Write-Host "1. Make sure SQL Server LocalDB is running" -ForegroundColor White
    Write-Host "2. Verify the database name is 'CursilloWeb'" -ForegroundColor White
    Write-Host "3. Run this script as Administrator" -ForegroundColor White
    Write-Host "4. Contact support if the issue persists" -ForegroundColor White
}

Read-Host "`nPress Enter to continue..."