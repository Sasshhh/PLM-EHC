# PowerShell script to update all pdfFormFields.SetField calls to use SetFieldWithFontSize
# This script updates the pdfDeneratePropertyLeaseAgreement method to use 9pt font

$filePath = "C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\Controllers\PropertyLeaseApplicationController.cs"
$content = Get-Content $filePath -Raw

# Replace all pdfFormFields.SetField with SetFieldWithFontSize(pdfFormFields, ..., 9.0f)
# Pattern: pdfFormFields.SetField("FieldName", value)
# Replace with: SetFieldWithFontSize(pdfFormFields, "FieldName", value, 9.0f)

$pattern = 'pdfFormFields\.SetField\("([^"]+)",\s*([^)]+)\)'
$replacement = 'SetFieldWithFontSize(pdfFormFields, "$1", $2, 9.0f)'

$updatedContent = $content -replace $pattern, $replacement

# Write back to file
$updatedContent | Set-Content $filePath -NoNewline

Write-Host "✅ Updated all SetField calls to use SetFieldWithFontSize with 9pt font" -ForegroundColor Green
Write-Host "Total replacements made: $(([regex]::Matches($content, $pattern)).Count)" -ForegroundColor Cyan
