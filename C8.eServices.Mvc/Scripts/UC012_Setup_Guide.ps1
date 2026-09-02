# =============================================
# UC012 MAINTENANCE JOB CARD - QUICK SETUP GUIDE
# =============================================

Write-Host "=========================================="  -ForegroundColor Cyan
Write-Host "UC012 MAINTENANCE JOB CARD SETUP" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$workspaceRoot = "C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc"

Write-Host "SETUP CHECKLIST:" -ForegroundColor Yellow
Write-Host ""

# 1. Check if PDF exists
Write-Host "[1] Checking Pre-Inspection Form PDF..." -ForegroundColor White
$pdfPath = Join-Path $workspaceRoot "Content\Pre-inspection Form v2.pdf"
if (Test-Path $pdfPath) {
    Write-Host "    ✓ PDF file found at: $pdfPath" -ForegroundColor Green
} else {
    Write-Host "    ✗ PDF file NOT found!" -ForegroundColor Red
    Write-Host "    Expected: $pdfPath" -ForegroundColor Yellow
}
Write-Host ""

# 2. Check controller actions file
Write-Host "[2] Checking Controller Actions File..." -ForegroundColor White
$controllerActionsPath = Join-Path $workspaceRoot "Controllers\PropertyLeaseApplicationController_MaintenanceJobCard.cs"
if (Test-Path $controllerActionsPath) {
    Write-Host "    ✓ Controller actions file ready for integration" -ForegroundColor Green
    Write-Host "    Location: $controllerActionsPath" -ForegroundColor Gray
} else {
    Write-Host "    ✗ Controller actions file NOT found!" -ForegroundColor Red
}
Write-Host ""

# 3. Check migration file
Write-Host "[3] Checking Migration File..." -ForegroundColor White
$migrationPath = Join-Path $workspaceRoot "Migrations\202603140900000_AddMaintenanceJobCardEnhancements.cs"
if (Test-Path $migrationPath) {
    Write-Host "    ✓ Migration file ready" -ForegroundColor Green
    Write-Host "    Location: $migrationPath" -ForegroundColor Gray
} else {
    Write-Host "    ✗ Migration file NOT found!" -ForegroundColor Red
}
Write-Host ""

# 4. Check SQL script
Write-Host "[4] Checking SQL Script..." -ForegroundColor White
$sqlScriptPath = Join-Path $workspaceRoot "Scripts\add_maintenance_job_card_document_types.sql"
if (Test-Path $sqlScriptPath) {
    Write-Host "    ✓ SQL script ready" -ForegroundColor Green
    Write-Host "    Location: $sqlScriptPath" -ForegroundColor Gray
} else {
    Write-Host "    ✗ SQL script NOT found!" -ForegroundColor Red
}
Write-Host ""

# 5. Check view file
Write-Host "[5] Checking Updated View..." -ForegroundColor White
$viewPath = Join-Path $workspaceRoot "Views\PropertyLeaseApplication\MaintenanceJobSheet.cshtml"
if (Test-Path $viewPath) {
    Write-Host "    ✓ View file updated" -ForegroundColor Green
    Write-Host "    Location: $viewPath" -ForegroundColor Gray
} else {
    Write-Host "    ✗ View file NOT found!" -ForegroundColor Red
}
Write-Host ""

Write-Host "=========================================="  -ForegroundColor Cyan
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "STEP 1: INTEGRATE CONTROLLER ACTIONS" -ForegroundColor White
Write-Host "  • Open: PropertyLeaseApplicationController.cs" -ForegroundColor Gray
Write-Host "  • Copy methods from: PropertyLeaseApplicationController_MaintenanceJobCard.cs" -ForegroundColor Gray
Write-Host "  • Paste at end of PropertyLeaseApplicationController class" -ForegroundColor Gray
Write-Host ""

Write-Host "STEP 2: RUN DATABASE MIGRATION" -ForegroundColor White
Write-Host "  • Open Package Manager Console in Visual Studio" -ForegroundColor Gray
Write-Host "  • Run: Update-Database" -ForegroundColor Yellow
Write-Host ""

Write-Host "STEP 3: ADD DOCUMENT TYPES TO DATABASE" -ForegroundColor White
Write-Host "  • Open SQL Server Management Studio" -ForegroundColor Gray
Write-Host "  • Connect to: CRMPLMDEV_2025" -ForegroundColor Gray
Write-Host "  • Run script: $sqlScriptPath" -ForegroundColor Yellow
Write-Host ""

Write-Host "STEP 4: BUILD SOLUTION" -ForegroundColor White
Write-Host "  • In Visual Studio: Build > Build Solution" -ForegroundColor Gray
Write-Host "  • Fix any compilation errors" -ForegroundColor Gray
Write-Host ""

Write-Host "STEP 5: TEST WORKFLOW" -ForegroundColor White
Write-Host "  • Login as: COESolarDev11 (Maintenance Manager)" -ForegroundColor Gray
Write-Host "  • Navigate to: Maintenance > Maintenance Job Sheets" -ForegroundColor Gray
Write-Host "  • Select job sheet > Capture Job Card" -ForegroundColor Gray
Write-Host "  • Test all features: Tasks, Images, Signature" -ForegroundColor Gray
Write-Host ""

Write-Host "=========================================="  -ForegroundColor Cyan
Write-Host "For detailed instructions, see:" -ForegroundColor White
Write-Host "UC012_IMPLEMENTATION_COMPLETE.md" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Offer to open files
Write-Host "Would you like to open the implementation guide? (Y/N): " -ForegroundColor Green -NoNewline
$response = Read-Host
if ($response -eq "Y" -or $response -eq "y") {
    $guidePath = Join-Path $workspaceRoot "UC012_IMPLEMENTATION_COMPLETE.md"
    if (Test-Path $guidePath) {
        Start-Process notepad.exe $guidePath
    }
}
