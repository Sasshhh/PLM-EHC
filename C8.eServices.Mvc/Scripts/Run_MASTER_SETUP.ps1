# =============================================
# RUN MASTER SETUP FOR UC012 → UC013 WORKFLOW
# =============================================
# This script executes MASTER_SETUP_UC012_UC013.sql

param(
    [string]$ServerInstance = "localhost",
    [string]$Database = "eServicesDb",
    [switch]$UseWindowsAuth = $true,
    [string]$Username = "",
    [string]$Password = ""
)

$scriptPath = Join-Path $PSScriptRoot "MASTER_SETUP_UC012_UC013.sql"

Write-Host "=========================================="
Write-Host "UC012 → UC013 MASTER SETUP RUNNER"
Write-Host "=========================================="
Write-Host ""
Write-Host "Script: $scriptPath"
Write-Host "Server: $ServerInstance"
Write-Host "Database: $Database"
Write-Host ""

if (-not (Test-Path $scriptPath)) {
    Write-Host "❌ ERROR: Script file not found!" -ForegroundColor Red
    Write-Host "   Expected: $scriptPath"
    exit 1
}

# Try using Invoke-Sqlcmd (SQL Server PowerShell module)
if (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue) {
    Write-Host "✅ Found Invoke-Sqlcmd, executing script..." -ForegroundColor Green
    Write-Host ""
    
    try {
        if ($UseWindowsAuth) {
            Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -InputFile $scriptPath -Verbose
        } else {
            Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -InputFile $scriptPath -Username $Username -Password $Password -Verbose
        }
        
        Write-Host ""
        Write-Host "✅ Script executed successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:"
        Write-Host "1. Check the output above for any errors"
        Write-Host "2. Navigate to MaintenanceJobSheet page"
        Write-Host "3. Sign the job card to test workflow"
        Write-Host ""
    }
    catch {
        Write-Host "❌ ERROR executing script:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host ""
        Write-Host "Try running manually in SSMS instead."
        exit 1
    }
}
# Try using sqlcmd
elseif (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
    Write-Host "✅ Found sqlcmd, executing script..." -ForegroundColor Green
    Write-Host ""
    
    try {
        if ($UseWindowsAuth) {
            sqlcmd -S $ServerInstance -d $Database -i $scriptPath -E
        } else {
            sqlcmd -S $ServerInstance -d $Database -i $scriptPath -U $Username -P $Password
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "✅ Script executed successfully!" -ForegroundColor Green
        } else {
            Write-Host ""
            Write-Host "⚠️  Script completed with errors (exit code: $LASTEXITCODE)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "❌ ERROR executing script:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "❌ Neither Invoke-Sqlcmd nor sqlcmd found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "MANUAL EXECUTION REQUIRED:"
    Write-Host ""
    Write-Host "Option 1: SQL Server Management Studio (SSMS)"
    Write-Host "  1. Open SSMS"
    Write-Host "  2. Connect to: $ServerInstance"
    Write-Host "  3. Open file: $scriptPath"
    Write-Host "  4. Press F5 to execute"
    Write-Host ""
    Write-Host "Option 2: Install SQL Server PowerShell module"
    Write-Host "  Install-Module -Name SqlServer -Scope CurrentUser"
    Write-Host "  Then run this script again"
    Write-Host ""
    exit 1
}
