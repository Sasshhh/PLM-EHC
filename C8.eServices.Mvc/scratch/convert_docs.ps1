$word = New-Object -ComObject Word.Application
$word.Visible = $false
try {
    $doc = $word.Documents.Open('C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\WordTemplates\Cancellation of Lease agreement template.docx')
    $doc.SaveAs('C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\Cancellation_Lease_Template_Draft.pdf', 17) # 17 is wdFormatPDF
    $doc.Close([ref][Microsoft.Office.Interop.Word.WdSaveOptions]::wdDoNotSaveChanges)
    Write-Host 'Cancellation of Lease converted successfully.'

    $doc = $word.Documents.Open('C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\WordTemplates\Notice to Vacate.docx')
    $doc.SaveAs('C:\REPO\PLM V1\PLM-EHC\C8.eServices.Mvc\PDFTemplates\Notice_To_Vacate_Template_Draft.pdf', 17)
    $doc.Close([ref][Microsoft.Office.Interop.Word.WdSaveOptions]::wdDoNotSaveChanges)
    Write-Host 'Notice to Vacate converted successfully.'
} catch {
    Write-Error $_.Exception.Message
} finally {
    $word.Quit()
}
