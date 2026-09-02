# =====================================================================
# Fix HasDSTV Migration Conflict - Automated Script
# =====================================================================
# This script automates the resolution steps
# Run from: Package Manager Console or PowerShell terminal
# =====================================================================

param(
    [string]$ConnectionString = "Server=.;Database=eServices_db;Integrated Security=true;",
    [switch]$WhatIf
)

Write-Host "=== HasDSTV Migration Conflict - Auto Fix ===" -ForegroundColor Cyan
Write-Host ""

# Check if in correct directory
if (-not (Test-Path "C8.eServices.Mvc\Migrations\Configuration.cs")) {
    Write-Host "❌ Error: Must run from solution root directory" -ForegroundColor Red
    Write-Host "   Current directory: $PWD" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Current directory: $PWD" -ForegroundColor Green
Write-Host ""

# Step 1: Verify Configuration.cs is updated
Write-Host "Step 1: Checking Configuration.cs..." -ForegroundColor Yellow
$configContent = Get-Content "C8.eServices.Mvc\Migrations\Configuration.cs" -Raw
if ($configContent -match "AutomaticMigrationsEnabled = false") {
    Write-Host "  ✅ AutomaticMigrationsEnabled = false" -ForegroundColor Green
} else {
    Write-Host "  ❌ AutomaticMigrationsEnabled is not false!" -ForegroundColor Red
    Write-Host "  Please set it manually in Configuration.cs" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Step 2: Check if SQL scripts exist
Write-Host "Step 2: Checking SQL fix scripts..." -ForegroundColor Yellow
$fixScript = "C8.eServices.Mvc\Scripts\Fix_HasDSTV_Migration_Conflict.sql"
$cleanupScript = "C8.eServices.Mvc\Scripts\Cleanup_MigrationHistory.sql"

if (Test-Path $fixScript) {
    Write-Host "  ✅ Fix_HasDSTV_Migration_Conflict.sql found" -ForegroundColor Green
} else {
    Write-Host "  ❌ Fix script not found: $fixScript" -ForegroundColor Red
    exit 1
}

if (Test-Path $cleanupScript) {
    Write-Host "  ✅ Cleanup_MigrationHistory.sql found" -ForegroundColor Green
} else {
    Write-Host "  ❌ Cleanup script not found: $cleanupScript" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 3: Execute SQL scripts
if ($WhatIf) {
    Write-Host "Step 3: [WHATIF] Would execute SQL scripts" -ForegroundColor Magenta
    Write-Host "  - Fix_HasDSTV_Migration_Conflict.sql" -ForegroundColor Gray
    Write-Host "  - Cleanup_MigrationHistory.sql" -ForegroundColor Gray
} else {
    Write-Host "Step 3: Executing SQL scripts..." -ForegroundColor Yellow
    Write-Host "  Connection: $ConnectionString" -ForegroundColor Gray
    
    try {
        # Try to execute using Invoke-Sqlcmd if available
        if (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue) {
            Write-Host "  Executing Fix_HasDSTV_Migration_Conflict.sql..." -ForegroundColor Gray
            Invoke-Sqlcmd -ConnectionString $ConnectionString -InputFile $fixScript -Verbose
            
            Write-Host "  Executing Cleanup_MigrationHistory.sql..." -ForegroundColor Gray
            Invoke-Sqlcmd -ConnectionString $ConnectionString -InputFile $cleanupScript -Verbose
            
            Write-Host "  ✅ SQL scripts executed successfully" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Invoke-Sqlcmd not available" -ForegroundColor Yellow
            Write-Host "  📋 Please run these scripts manually in SSMS:" -ForegroundColor Cyan
            Write-Host "     1. $fixScript" -ForegroundColor White
            Write-Host "     2. $cleanupScript" -ForegroundColor White
        }
    }
    catch {
        Write-Host "  ❌ SQL script execution failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "  📋 Please run scripts manually in SSMS" -ForegroundColor Yellow
    }
}
Write-Host ""

# Step 4: Run Update-Database
Write-Host "Step 4: Running Update-Database..." -ForegroundColor Yellow
if ($WhatIf) {
    Write-Host "  [WHATIF] Would run: Update-Database -Verbose" -ForegroundColor Magenta
} else {
    Write-Host "  Please run this command in Package Manager Console:" -ForegroundColor Cyan
    Write-Host "  Update-Database -Verbose" -ForegroundColor White
}
Write-Host ""

# Summary
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "✅ Configuration.cs updated (AutomaticMigrations disabled)" -ForegroundColor Green
Write-Host "✅ SQL fix scripts created and ready" -ForegroundColor Green
if ($WhatIf) {
    Write-Host "⏳ Run without -WhatIf to execute SQL scripts" -ForegroundColor Yellow
} else {
    Write-Host "⏳ Run Update-Database in Package Manager Console" -ForegroundColor Yellow
}
Write-Host ""
Write-Host "📖 For detailed instructions, see:" -ForegroundColor Cyan
Write-Host "   C8.eServices.Mvc\HASDSTV_MIGRATION_CONFLICT_RESOLUTION.md" -ForegroundColor White
Write-Host ""
