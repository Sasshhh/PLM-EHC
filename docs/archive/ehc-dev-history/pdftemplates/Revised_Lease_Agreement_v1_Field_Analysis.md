# Revised Lease Agreement v1 - PDF Field Analysis
**Document Date:** March 2026  
**Total Fields:** 85  
**Old Template:** LA_Template.pdf  
**New Template:** Revised Lease Agreement_v1.pdf  

---

## CATEGORY 1: Fields with Existing Values (Similar to Old Template)
**Count: 33 fields**  
✅ These fields exist in the old template and have data sources in `PropertyLeaseAgreementMaster` model or related models.

| Field Name | Old Template Field | Data Source | Model/Property | Notes |
|------------|-------------------|-------------|----------------|-------|
| `AgentName` | ✅ `AgentName` | PropertyLeaseAgreementMaster | `RepresentedBy` | Property manager or agent name |
| `FullNames` | ✅ `FullNames` | PropertyLeaseAgreementMaster | `ApplicantFullName` | Tenant full name |
| `IdentityNumber` | ✅ `IdentityNumber` | PropertyLeaseAgreementMaster | `ApplicantIdentityNumber` | Tenant ID number |
| `UnitNumber` | ✅ `UnitNumber` | PropertyLeaseAgreementMaster | `UnitNumber` | Unit/apartment number |
| `BuildingName` | ❓ NEW | ApplicationAllocatedProperty | `BuildingName` | Building name available |
| `UnitBlock` | ✅ `Block` | PropertyLeaseAgreementMaster | `BlockNumber` | Block/section identifier |
| `Rent` | ✅ `RentalUnit` | PropertyLeaseAgreementMaster | `MonthlyUnitRental` | Monthly rental amount |
| `Deposit` | ✅ `InitialDepositPremises` | PropertyLeaseAgreementMaster | `InitialDepositPremises` | Initial deposit |
| `CreditCheckFee` | ✅ `CreditCheckFee` | PropertyLeaseAgreementMaster | `CreditCheckFee` | Credit check fee |
| `AmountRent` | ✅ `UnitRentalAmount` | PropertyLeaseAgreementMaster | `UnitRentalAmountPM` | Same as Rent (monthly) |
| `AmountWater` | ✅ `Water` | PropertyLeaseAgreementMaster | `Water` or `_water` | Water charges |
| `AmountElectricity` | ✅ `Electricity` | PropertyLeaseAgreementMaster | `Electricity` | Electricity (prepaid if bool) |
| `AmountRefuse` | ✅ `Refuse` | PropertyLeaseAgreementMaster | `Refuse` or `_refuse` | Refuse charges |
| `AmountSewerage` | ✅ `Sewerage` | PropertyLeaseAgreementMaster | `Sewerage` or `_sewerage` | Sewerage charges |
| `AmountTOTAL` | ❓ CALCULATED | N/A | Computed | Sum of all monthly charges |
| `ParkingBay` | ✅ `CarportParkingBay` | PropertyLeaseAgreementMaster | `CarportParkingBayNumber` | Parking bay number |
| `Storeroom` | ✅ `StoreRooms` | PropertyLeaseAgreementMaster | `StoreRooms` | Storeroom charges |
| `CommencementDate` | ✅ `Date` | PropertyLeaseAgreementMaster | `CommencementDate` | Lease start date |
| `SignedAt` | ❓ NEW | N/A | User input | Location where tenant signed |
| `SignedDay` | ✅ `TenantSignDay` | PropertyLeaseAgreementMaster | `TenantSignDay` | Day of tenant signature |
| `SignedMonth` | ✅ `TenantSignDate` | PropertyLeaseAgreementMaster | `TenantSignDate` | Month/date of tenant signature |
| `SignedAt2` | ❓ NEW | N/A | User input | Location where manager signed |
| `SignedDay2` | ✅ `ManagersSignDay` | PropertyLeaseAgreementMaster | `ManagersSignDay` | Day of manager signature |
| `SignedMonth2` | ✅ `ManagersSignDate` | PropertyLeaseAgreementMaster | `ManagersSignDate` | Month/date of manager signature |
| `TenantSignature` | ✅ `TenantsSignature` | PropertyLeaseAgreementMaster | `TenantSignature` (base64 image) | Drawn signature |
| `PropertyManagerSignature` | ✅ `PropertyManagersSignature` | PropertyLeaseAgreementMaster | `PropertyManagersSignature` | Property manager sig |
| `RevenueManagerSignature` | ✅ `RevenueManagersSignature` | PropertyLeaseAgreementMaster | `RevenueManagersSignature` | Revenue manager sig |
| `Witness1` | ✅ `TenantsWitness1` | PropertyLeaseAgreementMaster | `TenantWitnessONE` | Tenant witness 1 |
| `Witness2` | ✅ `TenantsWitness2` | PropertyLeaseAgreementMaster | `TenantWitnessTWO` | Tenant witness 2 |
| `Witness3` | ✅ `ManagersWitness1` | PropertyLeaseAgreementMaster | `ManagersWitnessONE` | Manager witness 1 |
| `Witness4` | ✅ `ManagersWitness2` | PropertyLeaseAgreementMaster | `ManagersWitnessTWO` | Manager witness 2 |
| `CellNumber` | ❓ NEW | PropertyLeaseApplication | `CellNo` | Applicant mobile number |
| `UnitAddress` | ❓ NEW | LeaseDetails | `LeaAddress` | Full unit address |

---

## CATEGORY 2: Fields NOT in Old Template, BUT Data Exists in System
**Count: 18 fields**  
⚠️ These are new fields in the revised template, but we have the data available in our models/database.

| Field Name | Data Source | Model/Property | Action Required | Notes |
|------------|-------------|----------------|-----------------|-------|
| `Surname` | PropertyLeaseApplication | `LastName` | Add mapping | Separate surname field |
| `WorkNumber` | PropertyLeaseApplication | `WorkNo` | Add mapping | Work phone number |
| `Employer` | ❓ MISSING | N/A | **ASK USER** | Not captured anywhere |
| `Salary` | PropertyLeaseApplication | `GrossIncome` or `NetIncome` | Add mapping | Combined or individual? |
| `BankingDetails` | ❓ MISSING | N/A | **ASK USER** | Bank account info not stored |
| `TenantFullName` | Computed | `FirstName + " " + LastName` | Add mapping | Concatenate names |
| `KeyDeposit` | ❓ MISSING | N/A | **ASK USER** | Separate key deposit field |
| `AccessCard` | ❓ MISSING | N/A | **ASK USER** | Access card number/deposit |
| `RentSubsidy` | ❓ MISSING | N/A | **ASK USER** | Government rent subsidy amount |
| `DepositSubsidy` | ❓ MISSING | N/A | **ASK USER** | Government deposit subsidy |
| `keySubsidy` | ❓ MISSING | N/A | **ASK USER** | Key deposit subsidy |
| `AccessSubsidy` | ❓ MISSING | N/A | **ASK USER** | Access card subsidy |
| `LeaseAdministrationFee` | PropertyLeaseAgreementMaster | `LeaseAdministrationFee` | Add mapping | Already exists in master! |
| `LeaseAdministrationSubsidy` | ❓ MISSING | N/A | **ASK USER** | Subsidy for admin fee |
| `DSTV` | ❓ MISSING | N/A | **ASK USER** | DSTV subscription yes/no |
| `DSTVFee` | ❓ MISSING | N/A | **ASK USER** | Monthly DSTV fee |
| `AmountDSTV` | ❓ MISSING | N/A | **ASK USER** | Total DSTV charges |
| `DstvMonthlyFee` | ❓ MISSING | N/A | **ASK USER** | Same as DSTVFee? |
| `DSTVActivationFee` | ❓ MISSING | N/A | **ASK USER** | One-time DSTV activation |

---

## CATEGORY 3: Fields with NO Data Source (Never Existed Before)
**Count: 20 fields**  
🔴 These are completely new fields with no existing data source. Need user input on how to populate them.

| Field Name | Purpose (Best Guess) | Action Required | Priority |
|------------|---------------------|-----------------|----------|
| `Employer` | Tenant's employer name | **ASK USER** - New field to capture? | HIGH |
| `BankingDetails` | Bank account details for debit order | **ASK USER** - New field to capture? | HIGH |
| `KeyDeposit` | Separate key deposit amount | **ASK USER** - Split from main deposit? | MEDIUM |
| `AccessCard` | Access card number or deposit | **ASK USER** - Physical card tracking? | MEDIUM |
| `RentSubsidy` | Government housing subsidy for rent | **ASK USER** - EHC subsidy amount? | HIGH |
| `DepositSubsidy` | Government subsidy for deposit | **ASK USER** - EHC subsidy amount? | HIGH |
| `keySubsidy` | Subsidy covering key deposit | **ASK USER** - Subsidy breakdown? | MEDIUM |
| `AccessSubsidy` | Subsidy covering access card | **ASK USER** - Subsidy breakdown? | MEDIUM |
| `LeaseAdministrationSubsidy` | Subsidy covering admin fee | **ASK USER** - Subsidy breakdown? | MEDIUM |
| `DSTV` | DSTV subscription indicator (Yes/No) | **ASK USER** - Offer DSTV service? | LOW |
| `DSTVFee` | Monthly DSTV subscription fee | **ASK USER** - If DSTV offered | LOW |
| `AmountDSTV` | Total DSTV charges | **ASK USER** - If DSTV offered | LOW |
| `DstvMonthlyFee` | Duplicate of DSTVFee? | **ASK USER** - Clarify difference | LOW |
| `DSTVActivationFee` | One-time DSTV activation fee | **ASK USER** - If DSTV offered | LOW |
| `Occupant1Name` | Name of additional occupant #1 | **ASK USER** - Expand from 6 to 3? | HIGH |
| `Occupant1ID` | ID number of occupant #1 | **ASK USER** - Expand from 6 to 3? | HIGH |
| `Occupant1Relationship` | Relationship to tenant | **ASK USER** - NEW FIELD | HIGH |
| `Occupant1Contact` | Contact number of occupant | **ASK USER** - NEW FIELD | MEDIUM |
| `Occupant1Salary` | Income of occupant | **ASK USER** - NEW FIELD | MEDIUM |
| `Occupant2Name` through `Occupant2Salary` | Same as Occupant1 fields | **ASK USER** | HIGH |
| `Occupant3Name` through `Occupant3Salary` | Same as Occupant1 fields | **ASK USER** | HIGH |
| `SignedAt` | Physical location of tenant signature | **ASK USER** - Manual entry? | LOW |
| `SignedAt2` | Physical location of manager signature | **ASK USER** - Manual entry? | LOW |

---

## CATEGORY 4: Fields I Don't Understand (Need Clarification)
**Count: 14 fields**  
❓ These field names are unclear or appear to be placeholder names from the PDF template.

| Field Name | Best Guess | Questions for User |
|------------|-----------|-------------------|
| `undefined` | Unknown placeholder | What is this field for? |
| `undefined_2` | Unknown placeholder | What is this field for? |
| `undefined_3` | Unknown placeholder | What is this field for? |
| `undefined_4` | Unknown placeholder | What is this field for? |
| `undefined_5` | Unknown placeholder | What is this field for? |
| `undefined_6` | Unknown placeholder | What is this field for? |
| `undefined_7` | Unknown placeholder | What is this field for? |
| `undefined_8` | Unknown placeholder | What is this field for? |
| `undefined_9` | Unknown placeholder | What is this field for? |
| `undefined_10` | Unknown placeholder | What is this field for? |
| `undefined_11` | Unknown placeholder | What is this field for? |
| `undefined_12` | Unknown placeholder | What is this field for? |
| `undefined_13` | Unknown placeholder | What is this field for? |
| `undefined_14` | Unknown placeholder | What is this field for? |
| `Subject` | Subject line or heading? | What section is this in the PDF? |
| `Description` | Description text box? | What description goes here? |
| `Item` | Line item or list entry? | What item is this? |
| `Item_2` | Second line item? | What item is this? |

**NOTE:** The 14 `undefined` fields suggest the PDF template may have been created with unnamed form fields. These should be named properly in the PDF template itself, or we need to know their physical location in the document to understand their purpose.

---

## Summary Statistics

| Category | Count | Percentage |
|----------|-------|------------|
| **Category 1:** Existing fields with data | 33 | 38.8% |
| **Category 2:** New fields, but data exists | 18 | 21.2% |
| **Category 3:** Completely new fields | 20 | 23.5% |
| **Category 4:** Unclear/undefined fields | 14 | 16.5% |
| **TOTAL** | **85** | **100%** |

---

## Implementation Priority

### ✅ PHASE 1: Quick Win (33 fields - Ready to map immediately)
Map all Category 1 fields using existing code pattern from `pdfDeneratePropertyLeaseAgreement()` method.

### ⚠️ PHASE 2: Database Schema Updates Required (18 fields)
1. Add new columns to `PropertyLeaseAgreementMaster` table:
   - `Employer` (string)
   - `BankingDetails` (string)
   - `KeyDeposit` (decimal)
   - `AccessCard` (string)
   - `RentSubsidy` (decimal)
   - `DepositSubsidy` (decimal)
   - `KeySubsidy` (decimal)
   - `AccessSubsidy` (decimal)
   - `LeaseAdministrationSubsidy` (decimal)
   - `DSTV` (bool)
   - `DSTVFee` (decimal)
   - `DSTVActivationFee` (decimal)

2. Add new columns for expanded occupant details (Relationship, Contact, Salary for each occupant)

### 🔴 PHASE 3: User Input Required (14 fields)
Clarify the purpose of all `undefined` fields and `Subject`, `Description`, `Item`, `Item_2` fields.

---

## Questions for User

### **CRITICAL QUESTIONS (Must answer before implementation):**

1. **Subsidies:** Do you want to track government subsidies separately? If yes:
   - `RentSubsidy` - What percentage/amount of rent is subsidized?
   - `DepositSubsidy` - What percentage/amount of deposit is subsidized?
   - `KeySubsidy`, `AccessSubsidy`, `LeaseAdministrationSubsidy` - Are these separate line items?

2. **DSTV Service:** Do you offer DSTV subscriptions at your properties?
   - If YES: What are the monthly fees and activation fees?
   - If NO: Should these fields be left blank or marked "N/A"?

3. **Occupants:** The new template has 3 occupants with expanded details (Name, ID, Relationship, Contact, Salary).
   - The old template had 6 occupants but only Name and ID.
   - Do you want to REDUCE from 6 occupants to 3 BUT capture more details?
   - OR keep 6 occupants and add Relationship/Contact/Salary for all 6?

4. **Employer & Banking:** These are new fields.
   - Should we capture employer name during application?
   - Should we capture banking details for debit order?
   - Where in the application flow should these be added?

5. **Key Deposit & Access Card:** Are these separate from the main deposit?
   - Should we split `RequiedDepositAmount` into: Main Deposit + Key Deposit?
   - What is an "Access Card" - biometric card, gate remote, key fob?

6. **Undefined Fields (14 fields):** Can you open the PDF and tell me what these fields are for?
   - Are they checkboxes, text boxes, or signature fields?
   - What page/section are they on?

7. **SignedAt / SignedAt2:** These appear to be location fields (e.g., "Kempton Park", "Boksburg").
   - Should this be auto-populated from complex location?
   - Or manually entered by the signing officer?

---

## Old Template Field Mapping Reference
**Fields in OLD template (LA_Template.pdf) that are being REPLACED or RENAMED:**

| Old Field Name | New Field Name | Status |
|----------------|----------------|--------|
| `AgentName` | `AgentName` | ✅ SAME |
| `FullNames` | `FullNames` | ✅ SAME |
| `IdentityNumber` | `IdentityNumber` | ✅ SAME |
| `UnitNumber` | `UnitNumber` | ✅ SAME |
| `Block` | `UnitBlock` | ⚠️ RENAMED |
| `RentalUnit` | `Rent` / `AmountRent` | ⚠️ RENAMED |
| `Date` | `CommencementDate` | ⚠️ RENAMED |
| `TenantsSignature` | `TenantSignature` | ⚠️ RENAMED |
| `Occupant1` + `OccupantIDNO1` | `Occupant1Name` + `Occupant1ID` | ⚠️ RENAMED + EXPANDED |
| `Occupant2` + `OccupantIDNO2` | `Occupant2Name` + `Occupant2ID` | ⚠️ RENAMED + EXPANDED |
| `Occupant3` + `OccupantIDNO3` | `Occupant3Name` + `Occupant3ID` | ⚠️ RENAMED + EXPANDED |
| `Occupant4` + `OccupantIDNO4` | ❌ REMOVED | Old had 6, new has 3 |
| `Occupant5` + `OccupantIDNO5` | ❌ REMOVED | Old had 6, new has 3 |
| `Occupant6` + `OccupantIDNO6` | ❌ REMOVED | Old had 6, new has 3 |

---

## Next Steps

1. **USER:** Answer the 7 critical questions above
2. **USER:** Open `Revised Lease Agreement_v1.pdf` and identify what the 14 `undefined` fields are for
3. **DEV:** Create Entity Framework migration to add new columns to `PropertyLeaseAgreementMaster`
4. **DEV:** Update `PropertyLeaseApplicationController.pdfDeneratePropertyLeaseAgreement()` method to map all new fields
5. **DEV:** Update capture/edit views to collect new field data (Employer, Banking, Subsidies, DSTV, expanded occupant info)
6. **TEST:** Generate a test lease agreement with the new template to verify all fields populate correctly

---

**Document prepared by:** GitHub Copilot  
**Date:** March 2026  
**Status:** Awaiting user input on critical questions before implementation
