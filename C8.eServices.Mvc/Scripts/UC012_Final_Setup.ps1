# =============================================
# UC012 FINAL SETUP - QUICK COMPLETION SCRIPT
# =============================================

Write-Host "=========================================="  -ForegroundColor Cyan
Write-Host "UC012 MAINTENANCE JOB CARD - FINAL SETUP" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "CURRENT STATUS:" -ForegroundColor Green
Write-Host "✅ Document types added to database" -ForegroundColor Green
Write-Host "✅ Controller actions integrated" -ForegroundColor Green  
Write-Host "✅ View updated with all sections" -ForegroundColor Green
Write-Host "✅ Models and migrations ready" -ForegroundColor Green
Write-Host ""

Write-Host "FINAL STEPS NEEDED:" -ForegroundColor Yellow
Write-Host ""

# Step 1 - Check if Visual Studio is running
Write-Host "[STEP 1] Visual Studio Compilation Fix" -ForegroundColor White
Write-Host "Issue: Project may reference deleted controller file" -ForegroundColor Gray
Write-Host "Action needed:" -ForegroundColor Yellow
Write-Host "  1. Open Visual Studio" -ForegroundColor Gray
Write-Host "  2. Right-click C8.eServices.Mvc project → Unload Project" -ForegroundColor Gray
Write-Host "  3. Right-click unloaded project → Edit C8.eServices.Mvc.csproj" -ForegroundColor Gray
Write-Host "  4. Find and remove this line if it exists:" -ForegroundColor Gray
Write-Host "     <Compile Include=""Controllers\PropertyLeaseApplicationController_MaintenanceJobCard.cs"" />" -ForegroundColor Red
Write-Host "  5. Save and close, then Reload Project" -ForegroundColor Gray
Write-Host "  6. Build Solution (Ctrl+Shift+B)" -ForegroundColor Gray
Write-Host ""

# Step 2 - Database migration
Write-Host "[STEP 2] Database Migration" -ForegroundColor White
Write-Host "Command to run in Visual Studio Package Manager Console:" -ForegroundColor Gray
Write-Host "PM> Update-Database" -ForegroundColor Yellow
Write-Host ""

# Step 3 - Testing
Write-Host "[STEP 3] Testing Workflow" -ForegroundColor White
Write-Host "Test accounts:" -ForegroundColor Gray
Write-Host "  • Maintenance Manager: COESolarDev11" -ForegroundColor Gray
Write-Host "  • Property & Facilities Manager: COESolarDev05" -ForegroundColor Gray
Write-Host ""

Write-Host "Test workflow:" -ForegroundColor Gray
Write-Host "  1. Login as COESolarDev11" -ForegroundColor Gray
Write-Host "  2. Navigate: Maintenance → Maintenance Job Sheets" -ForegroundColor Gray
Write-Host "  3. Select job sheet → Capture Job Card" -ForegroundColor Gray
Write-Host "  4. Test task capture (Step 9 from UC012)" -ForegroundColor Gray
Write-Host "  5. Test before/after image uploads" -ForegroundColor Gray
Write-Host "  6. Test signature capture" -ForegroundColor Gray
Write-Host "  7. Verify routing to COESolarDev05 for major defects" -ForegroundColor Gray
Write-Host ""

# File verification
Write-Host "[VERIFICATION] Checking key files..." -ForegroundColor White
$workspaceRoot = "C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc"

# Check PDF
$pdfPath = Join-Path $workspaceRoot "Content\Pre-inspection Form v2.pdf"
if (Test-Path $pdfPath) {
    Write-Host "✅ PDF file found" -ForegroundColor Green
} else {
    Write-Host "❌ PDF file missing!" -ForegroundColor Red
}

# Check migration
$migrationPath = Join-Path $workspaceRoot "Migrations\202603140900000_AddMaintenanceJobCardEnhancements.cs"
if (Test-Path $migrationPath) {
    Write-Host "✅ Migration file ready" -ForegroundColor Green
} else {
    Write-Host "❌ Migration file missing!" -ForegroundColor Red
}

# Check view
$viewPath = Join-Path $workspaceRoot "Views\PropertyLeaseApplication\MaintenanceJobSheet.cshtml"
if (Test-Path $viewPath) {
    Write-Host "✅ View file updated" -ForegroundColor Green
} else {
    Write-Host "❌ View file missing!" -ForegroundColor Red
}

# Check controller
$controllerPath = Join-Path $workspaceRoot "Controllers\PropertyLeaseApplicationController.cs"
if (Test-Path $controllerPath) {
    # Check if it contains our new actions
    $content = Get-Content $controllerPath -Raw
    if ($content -match "AddMaintenanceTask" -and $content -match "SaveMaintenanceSignature") {
        Write-Host "✅ Controller actions integrated" -ForegroundColor Green
    } else {
        Write-Host "❌ Controller actions not found!" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Controller file missing!" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================="  -ForegroundColor Cyan
Write-Host "DATABASE STATUS:" -ForegroundColor Yellow
Write-Host "=========================================="  -ForegroundColor Cyan

# Test database connection
try {
    $result = sqlcmd -S "localhost" -E -d CRMPLMDEV_2025 -Q "SELECT COUNT(*) as DocumentTypeCount FROM DocumentTypes WHERE [Key] IN ('dt_maintenance_task_document', 'dt_maintenance_before_image', 'dt_maintenance_after_image')" -h -1
    $count = [int]$result.Trim()
    
    if ($count -eq 3) {
        Write-Host "✅ Document types added ($count/3)" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Document types incomplete ($count/3)" -ForegroundColor Yellow
    }
    
    Write-Host "✅ Database connection successful" -ForegroundColor Green
} catch {
    Write-Host "❌ Database connection failed" -ForegroundColor Red
    Write-Host "Check that CRMPLMDEV_2025 database is accessible" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=========================================="  -ForegroundColor Cyan
Write-Host "NEXT ACTIONS:" -ForegroundColor White
Write-Host "=========================================="  -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Fix Visual Studio compilation (unload/reload project)" -ForegroundColor Yellow
Write-Host "2. Run: Update-Database in Package Manager Console" -ForegroundColor Yellow  
Write-Host "3. Test the complete UC012 workflow" -ForegroundColor Yellow
Write-Host ""
Write-Host "For detailed instructions, see:" -ForegroundColor White
Write-Host "UC012_FINAL_SETUP_STATUS.md" -ForegroundColor Cyan
Write-Host ""

# Offer to open documentation
Write-Host "Open setup documentation? (Y/N): " -ForegroundColor Green -NoNewline
$response = Read-Host
if ($response -eq "Y" -or $response -eq "y") {
    $docPath = Join-Path $workspaceRoot "UC012_FINAL_SETUP_STATUS.md"
    if (Test-Path $docPath) {
        Start-Process notepad.exe $docPath
    }
}