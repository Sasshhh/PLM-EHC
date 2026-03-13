# Implementation Plan: Revised Lease Agreement v1
**Status:** APPROVED - Ready for Implementation  
**Date:** March 2026  
**Template:** Revised Lease Agreement_v1.pdf  
**Total Fields:** 85  

---

## USER DECISIONS SUMMARY

### ✅ CONFIRMED REQUIREMENTS:

1. **Subsidies** → Set all subsidy fields to "0" or "N/A" for now
2. **DSTV** → Set all DSTV fields to 0 and fees to 0 (placeholder for future)
3. **Occupants** → Reduce from 6 to 3, capture more details (Name, ID, Relationship, Contact, Salary)
4. **Employer & Banking** → Use dummy/placeholder data for testing
5. **Key Deposit & Access Card** → Leave at 0 (access card = unit/parking access card)
6. **Undefined Fields (14)** → Populate with tenant initials (signature/initial fields for T&Cs)
7. **SignedAt Locations** → Auto-populate from complex location
8. **Witnesses** → Leave all witness fields blank for now

---

## PHASE 1: IMMEDIATE IMPLEMENTATION (33 Fields + 18 New Mappings)

### A. Category 1 Fields - Direct Mapping (33 fields)
**Action:** Map using existing `PropertyLeaseAgreementMaster` data

| Field Name | Source Model | Property | Code Example |
|------------|--------------|----------|--------------|
| `AgentName` | PropertyLeaseAgreementMaster | `RepresentedBy` | `pdfFormFields.SetField("AgentName", master.RepresentedBy ?? "")` |
| `FullNames` | PropertyLeaseAgreementMaster | `ApplicantFullName` | `pdfFormFields.SetField("FullNames", master.ApplicantFullName ?? "")` |
| `IdentityNumber` | PropertyLeaseAgreementMaster | `ApplicantIdentityNumber` | `pdfFormFields.SetField("IdentityNumber", master.ApplicantIdentityNumber ?? "")` |
| `UnitNumber` | PropertyLeaseAgreementMaster | `UnitNumber` | `pdfFormFields.SetField("UnitNumber", master.UnitNumber ?? "")` |
| `UnitBlock` | PropertyLeaseAgreementMaster | `BlockNumber` | `pdfFormFields.SetField("UnitBlock", master.BlockNumber ?? "")` |
| `Rent` | PropertyLeaseAgreementMaster | `MonthlyUnitRental` | `pdfFormFields.SetField("Rent", master.MonthlyUnitRental.ToString("F2"))` |
| `Deposit` | PropertyLeaseAgreementMaster | `InitialDepositPremises` | `pdfFormFields.SetField("Deposit", master.InitialDepositPremises.ToString("F2"))` |
| `CreditCheckFee` | PropertyLeaseAgreementMaster | `CreditCheckFee` | `pdfFormFields.SetField("CreditCheckFee", master.CreditCheckFee == 0 ? "N/A" : master.CreditCheckFee.ToString("F2"))` |
| `AmountRent` | PropertyLeaseAgreementMaster | `UnitRentalAmountPM` | `pdfFormFields.SetField("AmountRent", master.UnitRentalAmountPM.ToString("F2"))` |
| `AmountWater` | PropertyLeaseAgreementMaster | `Water` or `_water` | `pdfFormFields.SetField("AmountWater", master._water.ToString("F2"))` |
| `AmountElectricity` | PropertyLeaseAgreementMaster | `Electricity` | `pdfFormFields.SetField("AmountElectricity", master.ELEC == true ? master.Electricity.ToString("F2") : "Prepaid")` |
| `AmountRefuse` | PropertyLeaseAgreementMaster | `Refuse` or `_refuse` | `pdfFormFields.SetField("AmountRefuse", master._refuse.ToString("F2"))` |
| `AmountSewerage` | PropertyLeaseAgreementMaster | `Sewerage` or `_sewerage` | `pdfFormFields.SetField("AmountSewerage", master._sewerage.ToString("F2"))` |
| `ParkingBay` | PropertyLeaseAgreementMaster | `CarportParkingBayNumber` | `pdfFormFields.SetField("ParkingBay", master.CarportParkingBayNumber ?? "")` |
| `Storeroom` | PropertyLeaseAgreementMaster | `StoreRooms` | `pdfFormFields.SetField("Storeroom", master.STR == true ? master.StoreRooms.ToString("F2") : "N/A")` |
| `CommencementDate` | PropertyLeaseAgreementMaster | `CommencementDate` | `pdfFormFields.SetField("CommencementDate", master.CommencementDate ?? "")` |
| `SignedDay` | PropertyLeaseAgreementMaster | `TenantSignDay` | `pdfFormFields.SetField("SignedDay", master.TenantSignDay ?? "")` |
| `SignedMonth` | PropertyLeaseAgreementMaster | `TenantSignDate` | `pdfFormFields.SetField("SignedMonth", master.TenantSignDate ?? "")` |
| `SignedDay2` | PropertyLeaseAgreementMaster | `ManagersSignDay` | `pdfFormFields.SetField("SignedDay2", master.ManagersSignDay ?? "")` |
| `SignedMonth2` | PropertyLeaseAgreementMaster | `ManagersSignDate` | `pdfFormFields.SetField("SignedMonth2", master.ManagersSignDate ?? "")` |
| `TenantSignature` | PropertyLeaseAgreementMaster | `TenantSignature` (base64) | *Use existing image conversion logic* |
| `PropertyManagerSignature` | PropertyLeaseAgreementMaster | `PropertyManagersSignature` | *Use existing image conversion logic* |
| `RevenueManagerSignature` | PropertyLeaseAgreementMaster | `RevenueManagersSignature` | *Use existing image conversion logic* |
| `LeaseAdministrationFee` | PropertyLeaseAgreementMaster | `LeaseAdministrationFee` | `pdfFormFields.SetField("LeaseAdministrationFee", master.LeaseAdministrationFee.ToString("F2"))` |
| `Witness1` | PropertyLeaseAgreementMaster | `TenantWitnessONE` | **LEAVE BLANK** (per user request) |
| `Witness2` | PropertyLeaseAgreementMaster | `TenantWitnessTWO` | **LEAVE BLANK** (per user request) |
| `Witness3` | PropertyLeaseAgreementMaster | `ManagersWitnessONE` | **LEAVE BLANK** (per user request) |
| `Witness4` | PropertyLeaseAgreementMaster | `ManagersWitnessTWO` | **LEAVE BLANK** (per user request) |

### B. Category 2 Fields - New Mappings with Existing Data (18 fields)

| Field Name | Source Model | Property | Implementation |
|------------|--------------|----------|----------------|
| `Surname` | PropertyLeaseApplication | `LastName` | `pdfFormFields.SetField("Surname", application.LastName ?? "")` |
| `CellNumber` | PropertyLeaseApplication | `CellNo` | `pdfFormFields.SetField("CellNumber", application.CellNo ?? "")` |
| `WorkNumber` | PropertyLeaseApplication | `WorkNo` | `pdfFormFields.SetField("WorkNumber", application.WorkNo ?? "")` |
| `Salary` | PropertyLeaseApplication | `GrossIncome` | `pdfFormFields.SetField("Salary", application.GrossIncome?.ToString("F2") ?? "0.00")` |
| `TenantFullName` | Computed | `FirstName + " " + LastName` | `pdfFormFields.SetField("TenantFullName", $"{application.FirstName} {application.LastName}")` |
| `BuildingName` | ApplicationAllocatedProperty | `BuildingName` | Get via `matchedUnit.ApplicationAllocatedPropertyId` |
| `UnitAddress` | LeaseDetails | `LeaAddress` | `pdfFormFields.SetField("UnitAddress", lease.LeaAddress ?? "")` |
| `AmountTOTAL` | **CALCULATED** | Sum of charges | `double total = master._water + master._refuse + master._sewerage + master.UnitRentalAmountPM; pdfFormFields.SetField("AmountTOTAL", total.ToString("F2"))` |
| `SignedAt` | **AUTO-POPULATE** | Complex location | Get from `PreferredComplexArea.Name` via application |
| `SignedAt2` | **AUTO-POPULATE** | Complex location | Same as `SignedAt` |

### C. Subsidy Fields - Set to 0/N/A (9 fields)

| Field Name | Value | Implementation |
|------------|-------|----------------|
| `RentSubsidy` | `"0.00"` | `pdfFormFields.SetField("RentSubsidy", "0.00")` |
| `DepositSubsidy` | `"0.00"` | `pdfFormFields.SetField("DepositSubsidy", "0.00")` |
| `keySubsidy` | `"0.00"` | `pdfFormFields.SetField("keySubsidy", "0.00")` |
| `AccessSubsidy` | `"0.00"` | `pdfFormFields.SetField("AccessSubsidy", "0.00")` |
| `LeaseAdministrationSubsidy` | `"0.00"` | `pdfFormFields.SetField("LeaseAdministrationSubsidy", "0.00")` |
| `KeyDeposit` | `"0.00"` | `pdfFormFields.SetField("KeyDeposit", "0.00")` |
| `AccessCard` | `"N/A"` | `pdfFormFields.SetField("AccessCard", "N/A")` |
| `DSTV` | `"NO"` | `pdfFormFields.SetField("DSTV", "NO")` |
| `DSTVFee` | `"0.00"` | `pdfFormFields.SetField("DSTVFee", "0.00")` |
| `AmountDSTV` | `"0.00"` | `pdfFormFields.SetField("AmountDSTV", "0.00")` |
| `DstvMonthlyFee` | `"0.00"` | `pdfFormFields.SetField("DstvMonthlyFee", "0.00")` |
| `DSTVActivationFee` | `"0.00"` | `pdfFormFields.SetField("DSTVActivationFee", "0.00")` |

### D. Employer & Banking - Dummy Data for Testing (2 fields)

```csharp
// Placeholder values for demo purposes
pdfFormFields.SetField("Employer", "To Be Captured");
pdfFormFields.SetField("BankingDetails", "To Be Provided");
```

### E. Occupants - Map from Existing Occupant Data (15 fields)

**IMPORTANT:** Reduce from 6 occupants to 3, but capture MORE details per occupant.

**Existing data sources:**
- `PropertyLeaseAgreementMaster.OccupantONE`, `OccupantONEIdentityNo`
- `PropertyLeaseAgreementMaster.OccupantTWO`, `OccupantTWOIdentityNo`
- `PropertyLeaseAgreementMaster.OccupantTHREE`, `OccupantTHREEIdentityNo`

**NEW fields to populate:**
- `Occupant1Relationship` → Get from spouse details or default to "Family Member"
- `Occupant1Contact` → Default to same as application `CellNo` or blank
- `Occupant1Salary` → Default to "0.00" (not captured yet)

```csharp
// Occupant 1
pdfFormFields.SetField("Occupant1Name", master.OccupantONE ?? "");
pdfFormFields.SetField("Occupant1ID", master.OccupantONEIdentityNo ?? "");
pdfFormFields.SetField("Occupant1Relationship", master.OccupantONE != null ? "Family Member" : "");
pdfFormFields.SetField("Occupant1Contact", master.OccupantONE != null ? application.CellNo ?? "" : "");
pdfFormFields.SetField("Occupant1Salary", master.OccupantONE != null ? "0.00" : "");

// Occupant 2
pdfFormFields.SetField("Occupant2Name", master.OccupantTWO ?? "");
pdfFormFields.SetField("Occupant2ID", master.OccupantTWOIdentityNo ?? "");
pdfFormFields.SetField("Occupant2Relationship", master.OccupantTWO != null ? "Family Member" : "");
pdfFormFields.SetField("Occupant2Contact", master.OccupantTWO != null ? application.CellNo ?? "" : "");
pdfFormFields.SetField("Occupant2Salary", master.OccupantTWO != null ? "0.00" : "");

// Occupant 3
pdfFormFields.SetField("Occupant3Name", master.OccupantTHREE ?? "");
pdfFormFields.SetField("Occupant3ID", master.OccupantTHREEIdentityNo ?? "");
pdfFormFields.SetField("Occupant3Relationship", master.OccupantTHREE != null ? "Family Member" : "");
pdfFormFields.SetField("Occupant3Contact", master.OccupantTHREE != null ? application.CellNo ?? "" : "");
pdfFormFields.SetField("Occupant3Salary", master.OccupantTHREE != null ? "0.00" : "");
```

### F. Undefined Fields (14) - Tenant Initials for T&Cs Agreement

**Purpose:** Signature/initial fields where tenant confirms they understand lease conditions.

**Implementation:** Extract tenant initials from `FirstName` and `LastName`.

```csharp
// Generate tenant initials
string tenantInitials = "";
if (!string.IsNullOrEmpty(application.FirstName) && !string.IsNullOrEmpty(application.LastName))
{
    tenantInitials = application.FirstName.Substring(0, 1).ToUpper() + application.LastName.Substring(0, 1).ToUpper();
}

// Populate all 14 undefined fields with initials
for (int i = 1; i <= 14; i++)
{
    string fieldName = i == 1 ? "undefined" : $"undefined_{i}";
    pdfFormFields.SetField(fieldName, tenantInitials);
}
```

### G. Heading Column Fields (4 fields)
**Status:** CLARIFIED - These are heading/column labels, leave blank

| Field Name | Purpose | Implementation |
|------------|---------|----------------|
| `Subject` | Heading column label | `pdfFormFields.SetField("Subject", "")` |
| `Description` | Heading column label | `pdfFormFields.SetField("Description", "")` |
| `Item` | Heading column label | `pdfFormFields.SetField("Item", "")` |
| `Item_2` | Heading column label | `pdfFormFields.SetField("Item_2", "")` |

**Note:** User confirmed these are just heading columns in the PDF template. Variables created but left blank for future use.

---

## PHASE 2: CODE IMPLEMENTATION STEPS

### Step 1: Update PDF Template Path
Modify `pdfDeneratePropertyLeaseAgreement()` method to use new template:

```csharp
// OLD:
pdfTemplate = IP == "::1"
    ? Server.MapPath("~/PDFTemplates/LA_Template.pdf")
    : Server.MapPath(template);

// NEW (add toggle for testing):
bool useNewTemplate = true; // TODO: Add app setting or user selection
pdfTemplate = IP == "::1"
    ? Server.MapPath(useNewTemplate ? "~/PDFTemplates/Revised Lease Agreement_v1.pdf" : "~/PDFTemplates/LA_Template.pdf")
    : Server.MapPath(template);
```

### Step 2: Add Helper Method for Initials Generation
Add to `PropertyLeaseApplicationController.cs`:

```csharp
private string GetTenantInitials(PropertyLeaseApplication application)
{
    if (string.IsNullOrEmpty(application.FirstName) || string.IsNullOrEmpty(application.LastName))
        return "";
    
    return application.FirstName.Substring(0, 1).ToUpper() + application.LastName.Substring(0, 1).ToUpper();
}
```

### Step 3: Add Helper Method for Complex Location
Add to `PropertyLeaseApplicationController.cs`:

```csharp
private string GetSignedAtLocation(PropertyLeaseApplication application)
{
    if (application.PreferredComplexAreaId.HasValue)
    {
        var complex = db.PreferredComplexAreas.Find(application.PreferredComplexAreaId.Value);
        return complex?.Name ?? "Ekurhuleni";
    }
    return "Ekurhuleni";
}
```

### Step 4: Add Helper Method for Building Name Lookup
Add to `PropertyLeaseApplicationController.cs`:

```csharp
private string GetBuildingName(int applicationId)
{
    // Find matched unit -> ApplicationAllocatedProperty -> BuildingName
    var matchedUnit = db.MatchedUnits
        .FirstOrDefault(x => x.PropertyLeaseApplicationId == applicationId && x.IsActive && !x.IsDeleted);
    
    if (matchedUnit?.ApplicationAllocatedPropertyId.HasValue == true)
    {
        var unit = db.ApplicationAllocatedProperty.Find(matchedUnit.ApplicationAllocatedPropertyId.Value);
        return unit?.BuildingName ?? "";
    }
    return "";
}
```

### Step 5: Calculate Total Monthly Charges

```csharp
private double CalculateTotalMonthlyCharges(PropertyLeaseAgreementMaster master)
{
    double total = 0;
    total += master.UnitRentalAmountPM;
    total += master._water;
    total += master._refuse;
    total += master._sewerage;
    
    // Add parking if applicable
    if (master.SPP == true) total += master.ShadePortParking;
    if (master.OPP == true) total += master.OpenParking;
    if (master.STR == true) total += master.StoreRooms;
    
    return total;
}
```

### Step 6: Main PDF Generation Code Update

```csharp
[DecryptParameter]
public void pdfDeneratePropertyLeaseAgreement(int? ApplicationId)
{
    if (ApplicationId == null) throw new Exception("Invalid Application.");

    var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == ApplicationId);
    if (application == null) throw new Exception("Application not found.");

    var lease = db.LeaseDetails.OrderByDescending(x => x.Id)
        .FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IsNew && x.IsActive && !x.IsDeleted);
    if (lease == null) throw new Exception("Invalid Property Lease.");

    var master = db.propertyLeaseAgreementMasters
        .FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted);
    if (master == null) throw new Exception("Invalid Lease Agreement.");

    // Use new template
    string pdfTemplate = Server.MapPath("~/PDFTemplates/Revised Lease Agreement_v1.pdf");

    var timestamp2 = DateTime.Now.ToString("ddMMyyyyHHmmss");
    string folderName = Server.MapPath("~/Templates");
    string pathString = System.IO.Path.Combine(folderName, timestamp2);
    System.IO.Directory.CreateDirectory(pathString);

    string newFile = folderName + "\\" + timestamp2 + "_" + (application?.IDNo ?? "") + "_PLMLeaseAgreement_v1.pdf";
    var filename = (application?.ApplicationReferenceNumber ?? "Lease") + ".LEASEAGREEMENT_v1.pdf";

    PdfReader pdfReader = new PdfReader(pdfTemplate);
    PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
    AcroFields pdfFormFields = pdfStamper.AcroFields;

    // CATEGORY 1: Existing fields
    pdfFormFields.SetField("AgentName", master.RepresentedBy ?? "");
    pdfFormFields.SetField("FullNames", master.ApplicantFullName ?? "");
    pdfFormFields.SetField("IdentityNumber", master.ApplicantIdentityNumber ?? application.IDNo ?? "");
    pdfFormFields.SetField("UnitNumber", master.UnitNumber ?? "");
    pdfFormFields.SetField("UnitBlock", master.BlockNumber ?? "");
    pdfFormFields.SetField("Rent", master.MonthlyUnitRental.ToString("F2"));
    pdfFormFields.SetField("Deposit", master.InitialDepositPremises.ToString("F2"));
    pdfFormFields.SetField("CreditCheckFee", master.CreditCheckFee == 0 ? "N/A" : master.CreditCheckFee.ToString("F2"));
    pdfFormFields.SetField("AmountRent", master.UnitRentalAmountPM.ToString("F2"));
    pdfFormFields.SetField("AmountWater", master._water.ToString("F2"));
    pdfFormFields.SetField("AmountElectricity", master.ELEC == true ? master.Electricity.ToString("F2") : "Prepaid");
    pdfFormFields.SetField("AmountRefuse", master._refuse.ToString("F2"));
    pdfFormFields.SetField("AmountSewerage", master._sewerage.ToString("F2"));
    pdfFormFields.SetField("ParkingBay", master.CarportParkingBayNumber ?? "");
    pdfFormFields.SetField("Storeroom", master.STR == true ? master.StoreRooms.ToString("F2") : "N/A");
    pdfFormFields.SetField("CommencementDate", master.CommencementDate ?? "");
    pdfFormFields.SetField("SignedDay", master.TenantSignDay ?? "");
    pdfFormFields.SetField("SignedMonth", master.TenantSignDate ?? "");
    pdfFormFields.SetField("SignedDay2", master.ManagersSignDay ?? "");
    pdfFormFields.SetField("SignedMonth2", master.ManagersSignDate ?? "");
    pdfFormFields.SetField("LeaseAdministrationFee", master.LeaseAdministrationFee.ToString("F2"));

    // CATEGORY 2: New mappings with existing data
    pdfFormFields.SetField("Surname", application.LastName ?? "");
    pdfFormFields.SetField("CellNumber", application.CellNo ?? "");
    pdfFormFields.SetField("WorkNumber", application.WorkNo ?? "");
    pdfFormFields.SetField("Salary", application.GrossIncome?.ToString("F2") ?? "0.00");
    pdfFormFields.SetField("TenantFullName", $"{application.FirstName} {application.LastName}");
    pdfFormFields.SetField("BuildingName", GetBuildingName(application.Id));
    pdfFormFields.SetField("UnitAddress", lease.LeaAddress ?? "");
    pdfFormFields.SetField("AmountTOTAL", CalculateTotalMonthlyCharges(master).ToString("F2"));
    
    string signedLocation = GetSignedAtLocation(application);
    pdfFormFields.SetField("SignedAt", signedLocation);
    pdfFormFields.SetField("SignedAt2", signedLocation);

    // SUBSIDIES: Set to 0
    pdfFormFields.SetField("RentSubsidy", "0.00");
    pdfFormFields.SetField("DepositSubsidy", "0.00");
    pdfFormFields.SetField("keySubsidy", "0.00");
    pdfFormFields.SetField("AccessSubsidy", "0.00");
    pdfFormFields.SetField("LeaseAdministrationSubsidy", "0.00");
    pdfFormFields.SetField("KeyDeposit", "0.00");
    pdfFormFields.SetField("AccessCard", "N/A");

    // DSTV: Set to 0
    pdfFormFields.SetField("DSTV", "NO");
    pdfFormFields.SetField("DSTVFee", "0.00");
    pdfFormFields.SetField("AmountDSTV", "0.00");
    pdfFormFields.SetField("DstvMonthlyFee", "0.00");
    pdfFormFields.SetField("DSTVActivationFee", "0.00");

    // EMPLOYER & BANKING: Placeholder
    pdfFormFields.SetField("Employer", "To Be Captured");
    pdfFormFields.SetField("BankingDetails", "To Be Provided");

    // OCCUPANTS: 3 occupants with expanded details
    pdfFormFields.SetField("Occupant1Name", master.OccupantONE ?? "");
    pdfFormFields.SetField("Occupant1ID", master.OccupantONEIdentityNo ?? "");
    pdfFormFields.SetField("Occupant1Relationship", master.OccupantONE != null ? "Family Member" : "");
    pdfFormFields.SetField("Occupant1Contact", master.OccupantONE != null ? application.CellNo ?? "" : "");
    pdfFormFields.SetField("Occupant1Salary", master.OccupantONE != null ? "0.00" : "");

    pdfFormFields.SetField("Occupant2Name", master.OccupantTWO ?? "");
    pdfFormFields.SetField("Occupant2ID", master.OccupantTWOIdentityNo ?? "");
    pdfFormFields.SetField("Occupant2Relationship", master.OccupantTWO != null ? "Family Member" : "");
    pdfFormFields.SetField("Occupant2Contact", master.OccupantTWO != null ? application.CellNo ?? "" : "");
    pdfFormFields.SetField("Occupant2Salary", master.OccupantTWO != null ? "0.00" : "");

    pdfFormFields.SetField("Occupant3Name", master.OccupantTHREE ?? "");
    pdfFormFields.SetField("Occupant3ID", master.OccupantTHREEIdentityNo ?? "");
    pdfFormFields.SetField("Occupant3Relationship", master.OccupantTHREE != null ? "Family Member" : "");
    pdfFormFields.SetField("Occupant3Contact", master.OccupantTHREE != null ? application.CellNo ?? "" : "");
    pdfFormFields.SetField("Occupant3Salary", master.OccupantTHREE != null ? "0.00" : "");

    // UNDEFINED FIELDS: Tenant initials for T&C agreement
    string tenantInitials = GetTenantInitials(application);
    pdfFormFields.SetField("undefined", tenantInitials);
    for (int i = 2; i <= 14; i++)
    {
        pdfFormFields.SetField($"undefined_{i}", tenantInitials);
    }

    // WITNESSES: Leave blank
    pdfFormFields.SetField("Witness1", "");
    pdfFormFields.SetField("Witness2", "");
    pdfFormFields.SetField("Witness3", "");
    pdfFormFields.SetField("Witness4", "");

    // UNKNOWN FIELDS: Leave blank for now
    pdfFormFields.SetField("Subject", "");
    pdfFormFields.SetField("Description", "");
    pdfFormFields.SetField("Item", "");
    pdfFormFields.SetField("Item_2", "");

    // SIGNATURES: Use existing image conversion logic
    if (!string.IsNullOrEmpty(master.TenantSignature) && master.TenantSignature.Contains(","))
    {
        try
        {
            string base64Data = master.TenantSignature.Substring(master.TenantSignature.IndexOf(',') + 1);
            byte[] sigBytes = Convert.FromBase64String(base64Data);
            iTextSharp.text.Image sigImage = iTextSharp.text.Image.GetInstance(sigBytes);

            AcroFields.FieldPosition sigPos = null;
            var positions = pdfFormFields.GetFieldPositions("TenantSignature");
            if (positions != null && positions.Count > 0)
                sigPos = positions[0];

            if (sigPos != null)
            {
                iTextSharp.text.Rectangle rect = sigPos.position;
                sigImage.ScaleToFit(rect.Width, rect.Height);
                sigImage.SetAbsolutePosition(rect.Left, rect.Bottom);
                PdfContentByte cb = pdfStamper.GetOverContent(sigPos.page);
                cb.AddImage(sigImage);
            }
        }
        catch { }
    }

    // Property Manager & Revenue Manager signatures (similar logic)
    // ... (add similar code for PropertyManagerSignature and RevenueManagerSignature)

    pdfStamper.FormFlattening = true;
    pdfStamper.Close();

    string ReportURL = newFile;
    byte[] temp = System.IO.File.ReadAllBytes(ReportURL);

    Response.Clear();
    MemoryStream ms = new MemoryStream(temp);
    Response.ContentType = "application/pdf";
    Response.AddHeader("content-disposition", "attachment;filename=" + filename);
    Response.Buffer = true;
    ms.WriteTo(Response.OutputStream);
    Response.End();
}
```

---

## PHASE 3: TESTING CHECKLIST

### Test Case 1: Basic Field Population
- [ ] Generate lease for existing application with complete data
- [ ] Verify all Category 1 fields populate correctly
- [ ] Verify new Category 2 fields (Surname, CellNumber, WorkNumber, Salary, etc.)
- [ ] Verify calculated `AmountTOTAL` is correct

### Test Case 2: Subsidy & DSTV Fields
- [ ] Verify all subsidy fields show "0.00"
- [ ] Verify DSTV shows "NO"
- [ ] Verify all DSTV fees show "0.00"

### Test Case 3: Occupants
- [ ] Verify 3 occupants display correctly (not 6)
- [ ] Verify new fields: Relationship, Contact, Salary
- [ ] Verify empty occupants leave fields blank

### Test Case 4: Initials & Witnesses
- [ ] Verify all 14 `undefined` fields show tenant initials
- [ ] Verify all 4 Witness fields are blank

### Test Case 5: Signatures
- [ ] Verify tenant signature renders as image
- [ ] Verify property manager signature renders
- [ ] Verify revenue manager signature renders

### Test Case 6: Edge Cases
- [ ] Application with null/missing data
- [ ] Application with no occupants
- [ ] Application with no matched unit (BuildingName empty)

---

## ROLLOUT STRATEGY

### Option A: Parallel Testing (RECOMMENDED)
Keep both templates active with a toggle:

```csharp
// Add to AppSettings table
Key: "USE_NEW_LEASE_TEMPLATE"
Value: "false" (default to old template)

// In code:
var useNewTemplate = db.AppSettings.FirstOrDefault(x => x.Key == "USE_NEW_LEASE_TEMPLATE")?.Value == "true";
string templateFile = useNewTemplate ? "Revised Lease Agreement_v1.pdf" : "LA_Template.pdf";
```

### Option B: Immediate Switchover
Replace old template entirely after Phase 3 testing passes.

---

## SUCCESS CRITERIA

✅ All 85 PDF fields are populated (no errors)  
✅ Subsidies default to 0 as requested  
✅ DSTV fields default to 0/NO as requested  
✅ Occupants reduced to 3 with expanded details  
✅ Tenant initials populate all 14 undefined fields  
✅ Witnesses remain blank  
✅ Signatures render as images correctly  
✅ Generated PDF downloads successfully  

---

## NEXT ACTIONS

1. **USER:** Clarify `Subject`, `Description`, `Item`, `Item_2` fields
2. **DEV:** Implement Phase 2 code changes in `PropertyLeaseApplicationController.cs`
3. **DEV:** Add helper methods (initials, location, building name, total calculation)
4. **DEV:** Test with real application data
5. **USER:** Review generated PDF and provide feedback

---

**Document Status:** READY FOR IMPLEMENTATION  
**Prepared By:** GitHub Copilot  
**Date:** March 2026
