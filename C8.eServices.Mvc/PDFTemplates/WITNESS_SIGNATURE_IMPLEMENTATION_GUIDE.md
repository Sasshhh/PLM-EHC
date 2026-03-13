# WITNESS SIGNATURE IMPLEMENTATION - COMPLETE GUIDE

## 📋 OVERVIEW
This implementation adds Witness1 signature capture to the Lease Agreement Validation page with automatic PDF download when both tenant and witness signatures are captured.

---

## ✅ **STEP 1: Run Database Migration**

Execute this in Package Manager Console:
```powershell
Add-Migration AddWitness1SignatureToLeaseAgreementMaster
Update-Database
```

Or run this SQL script directly:
```sql
ALTER TABLE [dbo].[PropertyLeaseAgreementMaster]
ADD 
    [Witness1Signature] NVARCHAR(MAX) NULL,
    [Witness1Name] NVARCHAR(200) NULL,
    [Witness1SignatureDate] DATETIME NULL;
GO
```

---

## ✅ **STEP 2: Add Controller Method**

Add this method to `PropertyLeaseApplicationController.cs` right after `SaveTenantSignatureImage()`:

```csharp
/// <summary>
/// Saves Witness 1 signature image to the database
/// </summary>
[HttpPost]
public JsonResult SaveWitness1SignatureImage(int ApplicationId, string SignatureImageData, string WitnessName)
{
    try
    {
        var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == ApplicationId);
        if (application == null)
        {
            return Json(new { success = false, message = "Application not found." });
        }

        var lease = db.LeaseDetails.OrderByDescending(x => x.Id)
            .FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IsNew && x.IsActive && !x.IsDeleted);
        if (lease == null)
        {
            return Json(new { success = false, message = "Lease not found." });
        }

        var master = db.propertyLeaseAgreementMasters
            .FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && 
                                x.LeaseDetailsId == lease.Id && 
                                x.IsActive && !x.IsDeleted);
        if (master == null)
        {
            return Json(new { success = false, message = "Lease Agreement Master not found." });
        }

        // Save witness signature
        master.Witness1Signature = SignatureImageData;
        master.Witness1Name = WitnessName;
        master.Witness1SignatureDate = DateTime.Now;

        db.SaveChanges();

        // Check if both tenant and witness signatures are captured
        bool bothSignaturesCaptured = !string.IsNullOrEmpty(master.TenantSignature) && 
                                      !string.IsNullOrEmpty(master.Witness1Signature);

        return Json(new { 
            success = true, 
            message = "Witness signature saved successfully.",
            bothSignaturesCaptured = bothSignaturesCaptured 
        });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Error saving witness signature: " + ex.Message });
    }
}

/// <summary>
/// Checks if both tenant and witness signatures are captured
/// </summary>
[HttpGet]
public JsonResult CheckSignatureStatus(int ApplicationId)
{
    try
    {
        var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == ApplicationId);
        if (application == null)
        {
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        var lease = db.LeaseDetails.OrderByDescending(x => x.Id)
            .FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IsNew && x.IsActive && !x.IsDeleted);
        if (lease == null)
        {
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        var master = db.propertyLeaseAgreementMasters
            .FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && 
                                x.LeaseDetailsId == lease.Id && 
                                x.IsActive && !x.IsDeleted);
        if (master == null)
        {
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        bool hasTenantSignature = !string.IsNullOrEmpty(master.TenantSignature);
        bool hasWitnessSignature = !string.IsNullOrEmpty(master.Witness1Signature);

        return Json(new { 
            success = true,
            hasTenantSignature = hasTenantSignature,
            hasWitnessSignature = hasWitnessSignature,
            witnessName = master.Witness1Name ?? "",
            bothSignaturesCaptured = hasTenantSignature && hasWitnessSignature
        }, JsonRequestBehavior.AllowGet);
    }
    catch
    {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
    }
}
```

---

## ✅ **STEP 3: Update LeaseAgreementValidation.cshtml View**

Replace the signature section (around line 130-160) with this complete implementation:

```html
<div class="panel-heading">Generated Lease Agreement</div>
<div style="width: 90%;margin: 0 auto;">
    <br />
    <table id="FailureTable" class="table table-bordered table-hover table-striped panel panel-default display ">
        <thead>
            <tr>
                <th>Document Type</th>
                <th>Document</th>
                <th>Tenant Signature</th>
                <th>Witness 1 Signature</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>@Model.DocName</td>
                <td>
                    <input type="button" id="btnExternal" class="button1" value="View" 
                           onclick="location.href='@Url.Action("pdfDeneratePropertyLeaseAgreement", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("ApplicationId=" + ViewBag.PropertyLeaseAppliactionId.ToString()) })'" />
                </td>
                <td>
                    <div class="form-group">
                        @Html.Label("Tenant Signature", htmlAttributes: new { @class = "control-label" })
                        <div>
                            <input type="hidden" id="capturedTenantSignatureData" name="capturedTenantSignatureData" value="" />
                            <div id="tenantSignatureStatus" style="margin-bottom:5px;"></div>
                            <button type="button" class="button1" onclick="openTenantSignaturePad()">Draw Tenant Signature</button>
                        </div>
                    </div>
                </td>
                <td>
                    <div class="form-group">
                        @Html.Label("Witness 1 Signature", htmlAttributes: new { @class = "control-label" })
                        <div>
                            <input type="text" id="witnessName" placeholder="Witness Full Name" class="form-control" style="margin-bottom:5px;" maxlength="200" />
                            <input type="hidden" id="capturedWitnessSignatureData" name="capturedWitnessSignatureData" value="" />
                            <div id="witnessSignatureStatus" style="margin-bottom:5px;"></div>
                            <button type="button" class="button1" onclick="openWitnessSignaturePad()">Draw Witness Signature</button>
                        </div>
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</div>
```

---

## ✅ **STEP 4: Update JavaScript (Replace existing modal and scripts)**

Replace the entire signature modal section and JavaScript with this:

```html
<!-- Tenant Signature Modal -->
<div id="tenantSignaturePadModal" class="modal fade" role="dialog">
    <div class="modal-dialog" style="width:520px;">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal">&times;</button>
                <h4 class="modal-title">Draw Tenant Signature</h4>
            </div>
            <div class="modal-body" style="text-align:center;">
                <canvas id="tenantSignaturePadCanvas" width="460" height="160"></canvas>
                <br /><br />
                <button type="button" class="btn btn-default" onclick="clearTenantSignaturePad()">Clear</button>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                <button type="button" class="button1" onclick="confirmTenantSignature()">Confirm Signature</button>
            </div>
        </div>
    </div>
</div>

<!-- Witness Signature Modal -->
<div id="witnessSignaturePadModal" class="modal fade" role="dialog">
    <div class="modal-dialog" style="width:520px;">
        <div class="modal-content">
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal">&times;</button>
                <h4 class="modal-title">Draw Witness Signature</h4>
            </div>
            <div class="modal-body" style="text-align:center;">
                <canvas id="witnessSignaturePadCanvas" width="460" height="160"></canvas>
                <br /><br />
                <button type="button" class="btn btn-default" onclick="clearWitnessSignaturePad()">Clear</button>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                <button type="button" class="button1" onclick="confirmWitnessSignature()">Confirm Signature</button>
            </div>
        </div>
    </div>
</div>

@section Scripts{
    <script src="@Url.Content( "~/scripts/Datatables/jquery.dataTables.min.js" )"></script>
    <script src="@Url.Content( "~/scripts/Datatables/dataTables.bootstrap.js" )"></script>
    <script>
        var _tenantCanvas, _tenantCtx, _tenantDrawing = false, _tenantHasMark = false;
        var _witnessCanvas, _witnessCtx, _witnessDrawing = false, _witnessHasMark = false;

        // Check signature status on page load
        $(document).ready(function() {
            checkSignatureStatus();
        });

        function checkSignatureStatus() {
            $.ajax({
                url: '@Url.Action("CheckSignatureStatus", "PropertyLeaseApplication")',
                type: 'GET',
                data: { ApplicationId: @ViewBag.PropertyLeaseAppliactionId },
                success: function(result) {
                    if (result && result.success) {
                        if (result.hasTenantSignature) {
                            $('#tenantSignatureStatus').html('<span style="color:green;"><b>✓ Tenant signed</b></span>');
                        }
                        if (result.hasWitnessSignature) {
                            $('#witnessSignatureStatus').html('<span style="color:green;"><b>✓ Witness signed: ' + result.witnessName + '</b></span>');
                            $('#witnessName').val(result.witnessName);
                        }
                        
                        // Auto-download if both signatures exist
                        if (result.bothSignaturesCaptured) {
                            showAutoDownloadMessage();
                        }
                    }
                }
            });
        }

        function showAutoDownloadMessage() {
            swal({
                title: 'Both Signatures Captured!',
                text: 'Lease Agreement PDF will download automatically.',
                icon: 'success',
                timer: 3000,
                button: false
            }).then(function() {
                triggerPdfDownload();
            });
        }

        function triggerPdfDownload() {
            window.location.href = '@Url.Action("pdfDeneratePropertyLeaseAgreement", "PropertyLeaseApplication", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("ApplicationId=" + ViewBag.PropertyLeaseAppliactionId.ToString()) })';
        }

        // ========== TENANT SIGNATURE FUNCTIONS ==========
        function openTenantSignaturePad() {
            $('#tenantSignaturePadModal').modal({ backdrop: 'static', keyboard: false, show: true });
            _tenantCanvas = document.getElementById('tenantSignaturePadCanvas');
            _tenantCtx = _tenantCanvas.getContext('2d');
            _tenantCtx.clearRect(0, 0, _tenantCanvas.width, _tenantCanvas.height);
            _tenantHasMark = false;
            _tenantCtx.strokeStyle = '#000';
            _tenantCtx.lineWidth = 2;
            _tenantCtx.lineCap = 'round';

            _tenantCanvas.onmousedown = function(e) { _tenantDrawing = true; _tenantCtx.beginPath(); var r = _tenantCanvas.getBoundingClientRect(); _tenantCtx.moveTo(e.clientX - r.left, e.clientY - r.top); };
            _tenantCanvas.onmousemove = function(e) { if (!_tenantDrawing) return; _tenantHasMark = true; var r = _tenantCanvas.getBoundingClientRect(); _tenantCtx.lineTo(e.clientX - r.left, e.clientY - r.top); _tenantCtx.stroke(); };
            _tenantCanvas.onmouseup = function() { _tenantDrawing = false; };
            _tenantCanvas.onmouseleave = function() { _tenantDrawing = false; };

            _tenantCanvas.ontouchstart = function(e) { e.preventDefault(); _tenantDrawing = true; _tenantCtx.beginPath(); var r = _tenantCanvas.getBoundingClientRect(); _tenantCtx.moveTo(e.touches[0].clientX - r.left, e.touches[0].clientY - r.top); };
            _tenantCanvas.ontouchmove = function(e) { e.preventDefault(); if (!_tenantDrawing) return; _tenantHasMark = true; var r = _tenantCanvas.getBoundingClientRect(); _tenantCtx.lineTo(e.touches[0].clientX - r.left, e.touches[0].clientY - r.top); _tenantCtx.stroke(); };
            _tenantCanvas.ontouchend = function() { _tenantDrawing = false; };
        }

        function clearTenantSignaturePad() {
            if (_tenantCtx) _tenantCtx.clearRect(0, 0, _tenantCanvas.width, _tenantCanvas.height);
            _tenantHasMark = false;
        }

        function confirmTenantSignature() {
            if (!_tenantHasMark) {
                swal('Error', 'Please draw tenant signature before confirming.', 'warning');
                return;
            }
            var dataUrl = _tenantCanvas.toDataURL('image/png');
            $.ajax({
                url: '@Url.Action("SaveTenantSignatureImage", "PropertyLeaseApplication")',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({ ApplicationId: @ViewBag.PropertyLeaseAppliactionId, SignatureImageData: dataUrl }),
                success: function(result) {
                    if (result && result.success) {
                        document.getElementById('capturedTenantSignatureData').value = dataUrl;
                        document.getElementById('tenantSignatureStatus').innerHTML = '<span style="color:green;"><b>✓ Tenant signed</b></span>';
                        $('#tenantSignaturePadModal').modal('hide');
                        
                        // Check if both signatures are now captured
                        checkAndAutoDownload();
                    } else {
                        swal('Error', (result && result.message) ? result.message : 'Failed to save tenant signature.', 'error');
                    }
                },
                error: function() {
                    swal('Error', 'An error occurred while saving tenant signature.', 'error');
                }
            });
        }

        // ========== WITNESS SIGNATURE FUNCTIONS ==========
        function openWitnessSignaturePad() {
            var witnessName = $('#witnessName').val().trim();
            if (!witnessName) {
                swal('Error', 'Please enter witness full name first.', 'warning');
                $('#witnessName').focus();
                return;
            }

            $('#witnessSignaturePadModal').modal({ backdrop: 'static', keyboard: false, show: true });
            _witnessCanvas = document.getElementById('witnessSignaturePadCanvas');
            _witnessCtx = _witnessCanvas.getContext('2d');
            _witnessCtx.clearRect(0, 0, _witnessCanvas.width, _witnessCanvas.height);
            _witnessHasMark = false;
            _witnessCtx.strokeStyle = '#000';
            _witnessCtx.lineWidth = 2;
            _witnessCtx.lineCap = 'round';

            _witnessCanvas.onmousedown = function(e) { _witnessDrawing = true; _witnessCtx.beginPath(); var r = _witnessCanvas.getBoundingClientRect(); _witnessCtx.moveTo(e.clientX - r.left, e.clientY - r.top); };
            _witnessCanvas.onmousemove = function(e) { if (!_witnessDrawing) return; _witnessHasMark = true; var r = _witnessCanvas.getBoundingClientRect(); _witnessCtx.lineTo(e.clientX - r.left, e.clientY - r.top); _witnessCtx.stroke(); };
            _witnessCanvas.onmouseup = function() { _witnessDrawing = false; };
            _witnessCanvas.onmouseleave = function() { _witnessDrawing = false; };

            _witnessCanvas.ontouchstart = function(e) { e.preventDefault(); _witnessDrawing = true; _witnessCtx.beginPath(); var r = _witnessCanvas.getBoundingClientRect(); _witnessCtx.moveTo(e.touches[0].clientX - r.left, e.touches[0].clientY - r.top); };
            _witnessCanvas.ontouchmove = function(e) { e.preventDefault(); if (!_witnessDrawing) return; _witnessHasMark = true; var r = _witnessCanvas.getBoundingClientRect(); _witnessCtx.lineTo(e.touches[0].clientX - r.left, e.touches[0].clientY - r.top); _witnessCtx.stroke(); };
            _witnessCanvas.ontouchend = function() { _witnessDrawing = false; };
        }

        function clearWitnessSignaturePad() {
            if (_witnessCtx) _witnessCtx.clearRect(0, 0, _witnessCanvas.width, _witnessCanvas.height);
            _witnessHasMark = false;
        }

        function confirmWitnessSignature() {
            if (!_witnessHasMark) {
                swal('Error', 'Please draw witness signature before confirming.', 'warning');
                return;
            }
            var witnessName = $('#witnessName').val().trim();
            var dataUrl = _witnessCanvas.toDataURL('image/png');
            $.ajax({
                url: '@Url.Action("SaveWitness1SignatureImage", "PropertyLeaseApplication")',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({ 
                    ApplicationId: @ViewBag.PropertyLeaseAppliactionId, 
                    SignatureImageData: dataUrl,
                    WitnessName: witnessName
                }),
                success: function(result) {
                    if (result && result.success) {
                        document.getElementById('capturedWitnessSignatureData').value = dataUrl;
                        document.getElementById('witnessSignatureStatus').innerHTML = '<span style="color:green;"><b>✓ Witness signed: ' + witnessName + '</b></span>';
                        $('#witnessSignaturePadModal').modal('hide');
                        
                        // Check if both signatures are now captured
                        checkAndAutoDownload();
                    } else {
                        swal('Error', (result && result.message) ? result.message : 'Failed to save witness signature.', 'error');
                    }
                },
                error: function() {
                    swal('Error', 'An error occurred while saving witness signature.', 'error');
                }
            });
        }

        // Check if both signatures captured and auto-download
        function checkAndAutoDownload() {
            $.ajax({
                url: '@Url.Action("CheckSignatureStatus", "PropertyLeaseApplication")',
                type: 'GET',
                data: { ApplicationId: @ViewBag.PropertyLeaseAppliactionId },
                success: function(result) {
                    if (result && result.success && result.bothSignaturesCaptured) {
                        setTimeout(function() {
                            showAutoDownloadMessage();
                        }, 500);
                    }
                }
            });
        }

        // Existing functions (keep these)
        function ChangeStatus() {
            // Your existing code
        }

        function clickSubmit() {
            // Your existing code
        }
    </script>
}
```

---

## ✅ **TESTING CHECKLIST**

1. ☐ Run database migration
2. ☐ Build solution (Ctrl+Shift+B)
3. ☐ Navigate to Lease Agreement Validation page
4. ☐ Click "Draw Tenant Signature" → Draw → Confirm
5. ☐ Enter witness name
6. ☐ Click "Draw Witness Signature" → Draw → Confirm
7. ☐ PDF should auto-download after both signatures captured
8. ☐ Refresh page → Both signatures should show as captured
9. ☐ PDF should auto-download on page load if both exist

---

## 📝 **KEY FEATURES**

✅ Dual signature capture (Tenant + Witness 1)  
✅ Witness name capture with validation  
✅ Auto-download PDF when both signatures captured  
✅ Persistent signature status (survives page refresh)  
✅ Touch-screen compatible  
✅ Separate canvases for each signature  
✅ Visual confirmation (✓ green checkmarks)  

---

## 🚀 **NEXT STEPS**

After testing, you can:
- Add more witnesses (Witness2, Witness3) using same pattern
- Add ID number capture for witnesses
- Store signature timestamps (already included in database)
- Add audit trail for signature captures

---

**Created:** March 10, 2026  
**Author:** GitHub Copilot  
**Status:** Ready for Implementation
