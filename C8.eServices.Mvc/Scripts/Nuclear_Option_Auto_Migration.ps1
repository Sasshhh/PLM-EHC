# =====================================================================
# NUCLEAR OPTION - Enable Automatic Migration ONCE
# =====================================================================
# Run this ONLY if you're tired of troubleshooting and want it fixed NOW.
# =====================================================================

Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Red
Write-Host "║  ⚠️  NUCLEAR OPTION - Temporary Auto Migration            ║" -ForegroundColor Red
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Red
Write-Host ""
Write-Host "This will:" -ForegroundColor Yellow
Write-Host "  1. Temporarily enable AutomaticMigrationsEnabled = true" -ForegroundColor White
Write-Host "  2. Run Update-Database (EF will auto-fix everything)" -ForegroundColor White
Write-Host "  3. Disable AutomaticMigrationsEnabled = false again" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  WARNING: This is a one-time fix for solo developers only!" -ForegroundColor Red
Write-Host ""

$Response = Read-Host "Continue? (yes/no)"
if ($Response -ne "yes") {
    Write-Host "Cancelled." -ForegroundColor Yellow
    exit
}

$ConfigPath = "C8.eServices.Mvc\Migrations\Configuration.cs"

# Step 1: Enable auto migrations
Write-Host ""
Write-Host "Step 1: Enabling automatic migrations..." -ForegroundColor Cyan
$Content = Get-Content $ConfigPath -Raw
$Content = $Content -replace "AutomaticMigrationsEnabled = false;", "AutomaticMigrationsEnabled = true;"
$Content = $Content -replace "AutomaticMigrationDataLossAllowed = false;", "AutomaticMigrationDataLossAllowed = true;"
Set-Content $ConfigPath $Content
Write-Host "✅ Enabled" -ForegroundColor Green

# Step 2: Build
Write-Host ""
Write-Host "Step 2: Building project..." -ForegroundColor Cyan
Write-Host "Please run in Visual Studio: Build Solution" -ForegroundColor Yellow
Write-Host ""
Read-Host "Press Enter after build completes"

# Step 3: Update database
Write-Host ""
Write-Host "Step 3: Run Update-Database in Package Manager Console" -ForegroundColor Cyan
Write-Host ""
Read-Host "Press Enter after Update-Database completes"

# Step 4: Disable auto migrations again
Write-Host ""
Write-Host "Step 4: Disabling automatic migrations..." -ForegroundColor Cyan
$Content = Get-Content $ConfigPath -Raw
$Content = $Content -replace "AutomaticMigrationsEnabled = true;", "AutomaticMigrationsEnabled = false;"
$Content = $Content -replace "AutomaticMigrationDataLossAllowed = true;", "AutomaticMigrationDataLossAllowed = false;"
Set-Content $ConfigPath $Content
Write-Host "✅ Disabled" -ForegroundColor Green

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  ✅ COMPLETE! Everything should be synced now.             ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "Database is now in sync with your models." -ForegroundColor Green
Write-Host "Automatic migrations are DISABLED again." -ForegroundColor Green
