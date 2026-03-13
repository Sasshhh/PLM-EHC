# Dump all AcroForm field names from the PDF template using iTextSharp
$templatePath = "C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\Revised Lease Agreement_v1.pdf"
$itextDll    = (Get-ChildItem "C:\REPO\PLM V1\PLM-EHC" -Recurse -Filter "itextsharp.dll" | Select-Object -First 1).FullName

if (-not $itextDll) { Write-Host "itextsharp.dll not found"; exit }
Write-Host "Using: $itextDll"
Add-Type -Path $itextDll

$reader = New-Object iTextSharp.text.pdf.PdfReader($templatePath)
$fields = $reader.AcroFields.Fields
Write-Host "`n=== PDF FIELD NAMES ($($fields.Count) total) ==="
$fields.Keys | Sort-Object | ForEach-Object { Write-Host "  [$_]" }
$reader.Close()
