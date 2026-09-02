$word = New-Object -ComObject Word.Application
$word.Visible = $false
try {
    $doc = $word.Documents.Open('C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\WordTemplates\Cancellation of Lease agreement template.docx')
    $text = $doc.Content.Text
    $doc.Close([ref][Microsoft.Office.Interop.Word.WdSaveOptions]::wdDoNotSaveChanges)
    Write-Host '--- Cancellation of Lease Text ---'
    Write-Host $text.Substring(0, [Math]::Min(2500, $text.Length))

    $doc = $word.Documents.Open('C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\WordTemplates\Notice to Vacate.docx')
    $text2 = $doc.Content.Text
    $doc.Close([ref][Microsoft.Office.Interop.Word.WdSaveOptions]::wdDoNotSaveChanges)
    Write-Host '--- Notice to Vacate Text ---'
    Write-Host $text2.Substring(0, [Math]::Min(2500, $text2.Length))
} catch {
    Write-Error $_.Exception.Message
} finally {
    $word.Quit()
}
