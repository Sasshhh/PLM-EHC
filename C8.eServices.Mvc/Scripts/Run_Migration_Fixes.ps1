# =====================================================================
# Auto-Fix Migration Issues - Run Both SQL Scripts
# =====================================================================
# This script automatically runs:
#   1. Fix_ContextKey_Mismatch.sql
#   2. Fix_Missing_Migrations_FINAL.sql
# =====================================================================

param(
    [string]$Server = "localhost",
    [string]$Database = "CRMPLMDEV_2025",
    [switch]$UseWindowsAuth = $true,
    [string]$Username,
    [string]$Password
)

$ErrorActionPreference = "Stop"

Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Auto-Fix Migration Issues - Automated Execution          ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Host "📁 Script Directory: $ScriptDir" -ForegroundColor Gray
Write-Host "📁 Project Root: $ProjectRoot" -ForegroundColor Gray
Write-Host ""

# Define SQL scripts to run (in order)
$SqlScripts = @(
    @{
        Name = "Fix ContextKey Mismatch"
        File = "Fix_ContextKey_Mismatch.sql"
        Description = "Corrects wrong ContextKey in 2 existing migrations"
    },
    @{
        Name = "Fix Missing Migrations"
        File = "Fix_Missing_Migrations_FINAL.sql"
        Description = "Inserts missing migration records into __MigrationHistory"
    }
)

# Build connection string
if ($UseWindowsAuth) {
    $ConnectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    Write-Host "🔐 Using Windows Authentication" -ForegroundColor Green
} else {
    if (-not $Username -or -not $Password) {
        Write-Host "❌ Error: Username and Password required for SQL Authentication" -ForegroundColor Red
        Write-Host "   Usage: -UseWindowsAuth:`$false -Username 'sa' -Password 'yourpass'" -ForegroundColor Yellow
        exit 1
    }
    $ConnectionString = "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
    Write-Host "🔐 Using SQL Authentication (User: $Username)" -ForegroundColor Green
}

Write-Host "🗄️  Server: $Server" -ForegroundColor Cyan
Write-Host "🗄️  Database: $Database" -ForegroundColor Cyan
Write-Host ""

# Check if SQL cmdlets are available
$HasSqlModule = Get-Module -ListAvailable -Name SqlServer -ErrorAction SilentlyContinue

if ($HasSqlModule) {
    Write-Host "✅ SqlServer PowerShell module detected" -ForegroundColor Green
    Import-Module SqlServer -ErrorAction SilentlyContinue
    $UseSqlModule = $true
} else {
    Write-Host "⚠️  SqlServer module not found, checking for sqlcmd.exe..." -ForegroundColor Yellow
    $SqlCmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
    
    if ($SqlCmd) {
        Write-Host "✅ sqlcmd.exe detected" -ForegroundColor Green
        $UseSqlModule = $false
    } else {
        Write-Host "❌ ERROR: Neither SqlServer module nor sqlcmd.exe found!" -ForegroundColor Red
        Write-Host "" 
        Write-Host "Please install one of the following:" -ForegroundColor Yellow
        Write-Host "  1. SqlServer PowerShell Module:" -ForegroundColor White
        Write-Host "     Install-Module -Name SqlServer -Scope CurrentUser" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  2. Or run scripts manually in SSMS" -ForegroundColor White
        Write-Host ""
        exit 1
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Executing SQL Scripts" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$AllSuccessful = $true
$ExecutedScripts = 0

foreach ($Script in $SqlScripts) {
    $ScriptPath = Join-Path $ScriptDir $Script.File
    
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host "📜 [$($ExecutedScripts + 1)/$($SqlScripts.Count)] $($Script.Name)" -ForegroundColor Yellow
    Write-Host "   $($Script.Description)" -ForegroundColor Gray
    Write-Host ""
    
    if (-not (Test-Path $ScriptPath)) {
        Write-Host "❌ ERROR: Script file not found: $ScriptPath" -ForegroundColor Red
        $AllSuccessful = $false
        continue
    }
    
    Write-Host "   File: $($Script.File)" -ForegroundColor Gray
    Write-Host "   Executing..." -ForegroundColor Cyan
    Write-Host ""
    
    try {
        if ($UseSqlModule) {
            # Use Invoke-Sqlcmd
            $Result = Invoke-Sqlcmd -ConnectionString $ConnectionString -InputFile $ScriptPath -Verbose -ErrorAction Stop
            
            # Display output (if any)
            if ($Result) {
                $Result | Format-Table -AutoSize | Out-String | Write-Host
            }
        } else {
            # Use sqlcmd.exe
            if ($UseWindowsAuth) {
                $Output = & sqlcmd -S $Server -d $Database -E -i $ScriptPath -I 2>&1
            } else {
                $Output = & sqlcmd -S $Server -d $Database -U $Username -P $Password -i $ScriptPath -I 2>&1
            }
            
            # Display output
            $Output | Write-Host
            
            # Check for errors
            if ($LASTEXITCODE -ne 0) {
                throw "sqlcmd exited with code $LASTEXITCODE"
            }
        }
        
        Write-Host ""
        Write-Host "   ✅ SUCCESS: $($Script.Name) completed" -ForegroundColor Green
        Write-Host ""
        $ExecutedScripts++
        
    } catch {
        Write-Host ""
        Write-Host "   ❌ ERROR executing $($Script.Name):" -ForegroundColor Red
        Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        $AllSuccessful = $false
        
        # Ask user if they want to continue
        Write-Host "   Continue with next script? (Y/N): " -ForegroundColor Yellow -NoNewline
        $Response = Read-Host
        
        if ($Response -notlike "Y*") {
            Write-Host ""
            Write-Host "⏸️  Execution stopped by user" -ForegroundColor Yellow
            break
        }
        Write-Host ""
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Execution Summary" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($AllSuccessful -and $ExecutedScripts -eq $SqlScripts.Count) {
    Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║  ✅ ALL SCRIPTS EXECUTED SUCCESSFULLY!                     ║" -ForegroundColor Green
    Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "✅ Executed $ExecutedScripts/$($SqlScripts.Count) scripts successfully" -ForegroundColor Green
    Write-Host "✅ ContextKey mismatches corrected" -ForegroundColor Green
    Write-Host "✅ Missing migrations inserted into __MigrationHistory" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 NEXT STEP:" -ForegroundColor Yellow
    Write-Host "   Open Package Manager Console in Visual Studio and run:" -ForegroundColor White
    Write-Host "   Update-Database -Verbose" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   Expected output: 'No pending explicit migrations'" -ForegroundColor Gray
    Write-Host ""
    
} else {
    Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║  ⚠️  EXECUTION COMPLETED WITH ERRORS                       ║" -ForegroundColor Red
    Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "⚠️  Executed $ExecutedScripts/$($SqlScripts.Count) scripts" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "📋 MANUAL STEPS:" -ForegroundColor Yellow
    Write-Host "   1. Review the errors above" -ForegroundColor White
    Write-Host "   2. Open SQL Server Management Studio" -ForegroundColor White
    Write-Host "   3. Manually run the scripts that failed" -ForegroundColor White
    Write-Host ""
}

Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
