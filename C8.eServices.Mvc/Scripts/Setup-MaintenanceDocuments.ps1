# =============================================
# RUN MAINTENANCE DOCUMENT CONFIGURATION SCRIPT
# This PowerShell script executes the SQL to set up maintenance document types
# =============================================

param(
    [string]$ServerInstance = "DESKTOP-JQ4FSGJ\SQLEXPRESS",
    [string]$Database = "CRMPLMDEV_2025"
)

Write-Host "=========================================="
Write-Host "Maintenance Document Configuration Setup"
Write-Host "=========================================="
Write-Host ""

$ScriptPath = "C8.eServices.Mvc\Scripts\add_maintenance_document_checklists.sql"

if (-not (Test-Path $ScriptPath)) {
    Write-Error "SQL script not found: $ScriptPath"
    exit 1
}

Write-Host "Server Instance: $ServerInstance" -ForegroundColor Green
Write-Host "Database: $Database" -ForegroundColor Green
Write-Host "Script: $ScriptPath" -ForegroundColor Green
Write-Host ""

try {
    Write-Host "Executing SQL script..." -ForegroundColor Yellow
    
    # Execute the SQL script
    Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -InputFile $ScriptPath -Verbose
    
    Write-Host ""
    Write-Host "✓ Script executed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "=========================================="
    Write-Host "Verification Query Results"
    Write-Host "=========================================="
    Write-Host ""
    
    # Run verification query
    $VerifyQuery = @"
    SELECT 
        'DocumentTypes' as TableName,
        dt.[Key] as [Key],
        dt.[Name] as [Name],
        dt.IsActive
    FROM DocumentTypes dt
    WHERE dt.[Key] IN ('dt_maintenance_task_document', 'dt_maintenance_before_image', 'dt_maintenance_after_image')
    
    UNION ALL
    
    SELECT 
        'DocumentCheckLists' as TableName,
        dt.[Key] as [Key],
        dt.[Name] as [Name],
        dcl.IsActive
    FROM DocumentCheckLists dcl
    INNER JOIN DocumentTypes dt ON dcl.DocumentTypeId = dt.Id
    WHERE dt.[Key] IN ('dt_maintenance_task_document', 'dt_maintenance_before_image', 'dt_maintenance_after_image')
    ORDER BY TableName, [Key]
"@
    
    $Results = Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -Query $VerifyQuery
    $Results | Format-Table -AutoSize
    
    Write-Host ""
    Write-Host "=========================================="
    Write-Host "✓ MAINTENANCE DOCUMENT CONFIGURATION COMPLETE"
    Write-Host "=========================================="
    
} catch {
    Write-Error "Failed to execute script: $($_.Exception.Message)"
    Write-Host ""
    Write-Host "Common issues:"
    Write-Host "1. SQL Server instance not running or accessible"
    Write-Host "2. Database name incorrect"
    Write-Host "3. Insufficient permissions"
    Write-Host ""
    Write-Host "Try running with different server instance:"
    Write-Host ".\Setup-MaintenanceDocuments.ps1 -ServerInstance 'localhost' -Database 'CRMPLMDEV_2025'"
    exit 1
}