# ============================================================================
# ROLLBACK UNIT ACCEPTANCE - QUICK SCRIPT
# ============================================================================
# This script makes it easy to rollback a unit acceptance for re-testing
# ============================================================================

param(
    [Parameter(Mandatory=$true)]
    [int]$ApplicationId,
    
    [string]$ServerInstance = "localhost",
    [string]$Database = "CRMPLMDEV_2025"
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "ROLLBACK UNIT ACCEPTANCE" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Application ID: $ApplicationId" -ForegroundColor Yellow
Write-Host "Database: $Database" -ForegroundColor Yellow
Write-Host ""

$scriptPath = Join-Path $PSScriptRoot "rollback_unit_acceptance.sql"

if (-not (Test-Path $scriptPath)) {
    Write-Host "ERROR: Script not found at $scriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "Reading SQL script..." -ForegroundColor Gray
$sqlContent = Get-Content $scriptPath -Raw

# Replace the @AppId parameter
$sqlContent = $sqlContent -replace 'DECLARE @AppId INT = \d+;', "DECLARE @AppId INT = $ApplicationId;"

Write-Host "Executing rollback..." -ForegroundColor Yellow
Write-Host ""

try {
    # Execute using Invoke-Sqlcmd (requires SqlServer module)
    if (-not (Get-Module -ListAvailable -Name SqlServer)) {
        Write-Host "WARNING: SqlServer module not installed. Installing..." -ForegroundColor Yellow
        Install-Module -Name SqlServer -Force -AllowClobber
    }

    Import-Module SqlServer

    $result = Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -Query $sqlContent -Verbose

    Write-Host ""
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "ROLLBACK COMPLETE!" -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Application $ApplicationId has been reset." -ForegroundColor Green
    Write-Host ""
    Write-Host "You can now:" -ForegroundColor Cyan
    Write-Host "  1. Rebuild solution (Ctrl+Shift+B)" -ForegroundColor White
    Write-Host "  2. Start debugging (F5)" -ForegroundColor White
    Write-Host "  3. Login as applicant" -ForegroundColor White
    Write-Host "  4. Accept unit offer again" -ForegroundColor White
    Write-Host ""
    
    # Display result summary
    if ($result) {
        Write-Host "Result Summary:" -ForegroundColor Cyan
        $result | Format-Table -AutoSize
    }
}
catch {
    Write-Host ""
    Write-Host "ERROR: Failed to execute rollback" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "MANUAL FALLBACK:" -ForegroundColor Yellow
    Write-Host "1. Open SQL Server Management Studio" -ForegroundColor White
    Write-Host "2. Connect to: $ServerInstance" -ForegroundColor White
    Write-Host "3. Open: $scriptPath" -ForegroundColor White
    Write-Host "4. Change @AppId to: $ApplicationId" -ForegroundColor White
    Write-Host "5. Execute manually (F5)" -ForegroundColor White
    exit 1
}

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# ============================================================================
# USAGE EXAMPLES:
# ============================================================================
# 
# Reset application 5216:
#   .\Rollback-UnitAcceptance.ps1 -ApplicationId 5216
#
# Reset with custom database:
#   .\Rollback-UnitAcceptance.ps1 -ApplicationId 5216 -Database "PROD_DB"
#
# Reset with remote server:
#   .\Rollback-UnitAcceptance.ps1 -ApplicationId 5216 -ServerInstance "SERVER\INSTANCE"
#
# ============================================================================
