# ========================================
# CLEAR ASP.NET TEMP FILES & RESTART
# ========================================
# Run this if Razor views are showing code as text instead of rendering

Write-Host "?? Clearing ASP.NET temporary files..." -ForegroundColor Cyan

# 1. Stop IIS Express
Write-Host "Stopping IIS Express..." -ForegroundColor Yellow
taskkill /F /IM iisexpress.exe /T 2>$null
taskkill /F /IM w3wp.exe /T 2>$null

# 2. Clear user temp ASP.NET files
$userTemp = "C:\Users\$env:USERNAME\AppData\Local\Temp\Temporary ASP.NET Files"
if (Test-Path $userTemp) {
    Write-Host "Clearing user temp files..." -ForegroundColor Yellow
    Remove-Item $userTemp -Recurse -Force -ErrorAction SilentlyContinue
}

# 3. Clear system temp ASP.NET files (requires admin)
$systemTemp = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files"
if (Test-Path $systemTemp) {
    Write-Host "Clearing system temp files..." -ForegroundColor Yellow
    Remove-Item $systemTemp -Recurse -Force -ErrorAction SilentlyContinue
}

# 4. Clear bin and obj folders in project
$projectPath = "C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc"
if (Test-Path "$projectPath\bin") {
    Write-Host "Clearing bin folder..." -ForegroundColor Yellow
    Remove-Item "$projectPath\bin" -Recurse -Force -ErrorAction SilentlyContinue
}
if (Test-Path "$projectPath\obj") {
    Write-Host "Clearing obj folder..." -ForegroundColor Yellow
    Remove-Item "$projectPath\obj" -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "? Cleanup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "?? Next steps:" -ForegroundColor Cyan
Write-Host "1. Close Visual Studio" -ForegroundColor White
Write-Host "2. Reopen Visual Studio" -ForegroundColor White
Write-Host "3. Rebuild solution (Ctrl+Shift+B)" -ForegroundColor White
Write-Host "4. Start debugging (F5)" -ForegroundColor White
Write-Host "5. Clear browser cache (Ctrl+Shift+Delete)" -ForegroundColor White
Write-Host ""
pause
