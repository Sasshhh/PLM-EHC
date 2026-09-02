# Template Download Fix for Pre-inspection Form v2.pdf

## Problem
Download button for unit inspection template has two critical issues:
1. **Performance**: Takes 4 minutes to download (database query slowness)
2. **Wrong File**: Downloads incorrect document from database instead of static "Pre-inspection Form v2.pdf"

## Root Cause
`DocumentGetConductUnitInspectionTemplate` method queries database with:
- PropertyLeaseApplicationId = 1
- ReferenceId = 1256
- Retrieves from Documents table via Entity Framework
- Causes slow database query (4 minutes)
- Returns wrong document

## Solution
Add new controller action to serve static PDF file directly using `Server.MapPath`:

### Step 1: Add Controller Action
Add to `PropertyLeaseApplicationController.cs`:

```csharp
/// <summary>
/// Downloads the Pre-inspection Form v2.pdf template directly from static files
/// </summary>
[HttpGet]
public ActionResult DownloadPreInspectionTemplate()
{
    try
    {
        // Use Server.MapPath for dynamic URL support (localhost, production, etc.)
        string filePath = Server.MapPath("~/Content/Pre-inspection Form v2.pdf");
        
        if (!System.IO.File.Exists(filePath))
        {
            TempData["ErrorMessage"] = "Pre-inspection Form template not found. Please contact support.";
            return RedirectToAction("Index", "Home");
        }
        
        // Serve file with correct MIME type and filename
        return File(filePath, "application/pdf", "Pre-inspection Form v2.pdf");
    }
    catch (Exception ex)
    {
        EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
        TempData["ErrorMessage"] = "Error downloading template: " + ex.Message;
        return RedirectToAction("Index", "Home");
    }
}
```

### Step 2: Update View
Modify `ConductUnitInspection.cshtml` line 115-118 to replace partial with direct download link:

**Replace:**
```razor
<div class="panel-heading">Conduct Unit Inspection Template</div>
<div style="width: 90%;margin: 0 auto;">
    @Html.Partial("_DocumentsPartialDisplayPLMTemplates", Model.DocumentsViewModelTemplate)
</div>
```

**With:**
```razor
<div class="panel-heading">Conduct Unit Inspection Template</div>
<div style="width: 90%;margin: 0 auto;">
    <div class="form-group">
        <div class="col-md-10">
            <p>Download the pre-inspection form template to fill out during the unit inspection:</p>
            <a href="@Url.Action("DownloadPreInspectionTemplate", "PropertyLeaseApplication")" 
               class="btn btn-primary" 
               target="_blank">
                <i class="glyphicon glyphicon-download-alt"></i> Download Pre-Inspection Form Template
            </a>
            <br /><br />
        </div>
    </div>
</div>
```

## Benefits
1. **Instant Download**: Static file serves immediately (no database query)
2. **Correct File**: Serves "Pre-inspection Form v2.pdf" from Content folder
3. **Dynamic URL Support**: `Server.MapPath` works with any hosting URL:
   - Localhost: `http://localhost:3450/...`
   - Production: `http://productionserver.com/...`
   - Resolves to physical file path regardless of domain
4. **Proper MIME Type**: Returns `application/pdf` for correct browser handling
5. **Error Handling**: Graceful failure if file not found

## Testing Checklist
- [ ] Download completes instantly (< 1 second)
- [ ] Correct file downloaded (Pre-inspection Form v2.pdf)
- [ ] Opens correctly in PDF viewer
- [ ] Works with localhost URL
- [ ] Works with production/hosted URL
- [ ] Error message shown if file missing

## Files Modified
1. `C8.eServices.Mvc\Controllers\PropertyLeaseApplicationController.cs` - Add DownloadPreInspectionTemplate action
2. `C8.eServices.Mvc\Views\PropertyLeaseApplication\ConductUnitInspection.cshtml` - Replace partial with direct link

## Alternative Solution (If Partial Must Be Reused)
If the partial view must remain for other pages, create simplified model:

```csharp
// In ConductUnitInspection action
var templateViewModel = new DocumentsViewModel
{
    DocumentCheckLists = new List<DocumentCheckList>
    {
        new DocumentCheckList
        {
            Id = 0,
            DocumentType = new DocumentType 
            { 
                Name = "Pre-Inspection Form",
                Description = "Download template to fill out during inspection"
            }
        }
    },
    Documents = new List<Document>
    {
        new Document
        {
            Id = 0,
            DocumentName = "Pre-inspection Form v2.pdf",
            DocumentCheckListId = 0,
            File = new Models.File 
            { 
                Data = "STATIC_TEMPLATE" // Special marker
            }
        }
    },
    IsUploadView = false
};
```

Then modify partial to check for "STATIC_TEMPLATE" marker and route to new action instead of File/GetDocument.

## Recommendation
**Use the direct link approach (Step 1 & 2)** - it's cleaner, faster, and doesn't require partial modifications.
