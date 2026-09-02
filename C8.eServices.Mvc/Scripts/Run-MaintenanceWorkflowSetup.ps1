# =============================================
# PowerShell Script to Execute Maintenance Workflow Setup
# This script runs the master SQL setup script
# =============================================

$scriptPath = Join-Path $PSScriptRoot "MASTER_SETUP_MAINTENANCE_WORKFLOW.sql"

Write-Host "=========================================="  -ForegroundColor Cyan
Write-Host "Maintenance Workflow Database Setup" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if SQL file exists
if (-not (Test-Path $scriptPath)) {
    Write-Host "ERROR: SQL script not found at: $scriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "SQL Script Location: $scriptPath" -ForegroundColor Green
Write-Host ""

# Prompt for database connection details
Write-Host "Enter Database Connection Details:" -ForegroundColor Yellow
Write-Host ""

$server = Read-Host "SQL Server instance (e.g., localhost, .\SQLEXPRESS, server\instance)"
if ([string]::IsNullOrWhiteSpace($server)) {
    $server = "localhost"
    Write-Host "Using default: localhost" -ForegroundColor Gray
}

$database = Read-Host "Database name (default: eServicesDb)"
if ([string]::IsNullOrWhiteSpace($database)) {
    $database = "eServicesDb"
    Write-Host "Using default: eServicesDb" -ForegroundColor Gray
}

Write-Host ""
$authChoice = Read-Host "Use Windows Authentication? (Y/N)"

if ($authChoice -eq 'Y' -or $authChoice -eq 'y') {
    $connectionString = "-S $server -d $database -E"
    Write-Host "Using Windows Authentication" -ForegroundColor Green
} else {
    $username = Read-Host "SQL Server username"
    $password = Read-Host "SQL Server password" -AsSecureString
    $passwordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))
    $connectionString = "-S $server -d $database -U $username -P $passwordPlain"
    Write-Host "Using SQL Server Authentication" -ForegroundColor Green
}

Write-Host ""
Write-Host "=========================================="  -ForegroundColor Cyan
Write-Host "Executing SQL Script..." -ForegroundColor Cyan
Write-Host "=========================================="  -ForegroundColor Cyan
Write-Host ""

# Execute the SQL script
try {
    $command = "sqlcmd $connectionString -i `"$scriptPath`" -I"
    Invoke-Expression $command
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "=========================================="  -ForegroundColor Green
        Write-Host "SQL Script Executed Successfully!" -ForegroundColor Green
        Write-Host "=========================================="  -ForegroundColor Green
        Write-Host ""
        Write-Host "Next Steps:" -ForegroundColor Yellow
        Write-Host "1. Create user accounts for Maintenance Manager and Property & Facilities Manager" -ForegroundColor White
        Write-Host "2. Assign users to the new roles in the application" -ForegroundColor White
        Write-Host "3. Update AppSettings with actual Customer IDs" -ForegroundColor White
        Write-Host "4. Assign Maintenance Managers to complexes in PreferredComplexAreas table" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "ERROR: SQL script execution failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: Failed to execute SQL script" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
