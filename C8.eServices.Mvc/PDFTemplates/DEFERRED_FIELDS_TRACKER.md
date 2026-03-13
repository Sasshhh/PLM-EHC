# Deferred Fields Tracker - Revised Lease Agreement v1
**Purpose:** Track fields that are defaulted to 0/blank for later implementation  
**Status:** PENDING FUTURE IMPLEMENTATION  
**Date:** March 2026  

---

## FIELDS DEFAULTED TO ZERO (Functional but Need Real Data)

### 1. SUBSIDY FIELDS (9 fields)
**Priority:** HIGH  
**Reason:** Need to confirm subsidy calculation logic with EHC finance team

| Field Name | Current Value | Action Required | Responsible Party |
|------------|---------------|-----------------|-------------------|
| `RentSubsidy` | `0.00` | Implement subsidy calculation from government/EHC grant | Finance Team |
| `DepositSubsidy` | `0.00` | Implement subsidy calculation from government/EHC grant | Finance Team |
| `keySubsidy` | `0.00` | Implement subsidy calculation (if applicable) | Finance Team |
| `AccessSubsidy` | `0.00` | Implement subsidy calculation (if applicable) | Finance Team |
| `LeaseAdministrationSubsidy` | `0.00` | Implement subsidy calculation (if applicable) | Finance Team |
| `KeyDeposit` | `0.00` | Determine if separate from main deposit | Finance Team |
| `AccessCard` | `N/A` | Determine access card tracking system | Facilities Team |

**Implementation Notes:**
- Need database table: `LeaseSubsidies` with columns:
  - `LeaseDetailsId` (FK)
  - `RentSubsidyAmount` (decimal)
  - `DepositSubsidyAmount` (decimal)
  - `KeySubsidyAmount` (decimal)
  - `AccessSubsidyAmount` (decimal)
  - `LeaseAdminSubsidyAmount` (decimal)
  - `SubsidyReferenceNumber` (string)
  - `SubsidyProvider` (string - "EHC", "National Housing", etc.)
  - `ApprovalDate` (DateTime)

**Questions to Answer:**
1. What is the subsidy percentage/amount for Airport Park?
2. Is subsidy amount fixed or percentage-based?
3. Does subsidy vary by unit type (1-bed, 2-bed, 3-bed)?
4. What documentation is required to prove subsidy eligibility?

---

### 2. DSTV FIELDS (5 fields)
**Priority:** MEDIUM  
**Reason:** Service not yet offered, need vendor agreement

| Field Name | Current Value | Action Required | Responsible Party |
|------------|---------------|-----------------|-------------------|
| `DSTV` | `NO` | Negotiate DSTV service provider agreement | Management |
| `DSTVFee` | `0.00` | Obtain monthly subscription pricing | Management |
| `AmountDSTV` | `0.00` | Implement pricing model (monthly vs activation) | Finance Team |
| `DstvMonthlyFee` | `0.00` | Clarify if different from DSTVFee | Management |
| `DSTVActivationFee` | `0.00` | Obtain one-time activation fee from provider | Finance Team |

**Implementation Notes:**
- Need database table: `UnitDSTVService` with columns:
  - `UnitId` (FK to ApplicationAllocatedProperty)
  - `HasDSTV` (bool)
  - `MonthlyFee` (decimal)
  - `ActivationFee` (decimal)
  - `DecoderNumber` (string)
  - `SmartCardNumber` (string)
  - `ActivationDate` (DateTime)
  - `ContractEndDate` (DateTime)

**Questions to Answer:**
1. Which DSTV package will be offered (Compact, Compact Plus, Premium)?
2. Will DSTV be optional or mandatory for tenants?
3. Who pays for installation (tenant or COE)?
4. What happens when tenant moves out (transfer decoder or cancel)?

---

### 3. OCCUPANT FINANCIAL DETAILS (3 fields per occupant = 9 fields)
**Priority:** HIGH  
**Reason:** Need to capture during application process

| Field Name | Current Value | Action Required | Responsible Party |
|------------|---------------|-----------------|-------------------|
| `Occupant1Relationship` | `"Family Member"` (generic) | Capture specific relationship during application | Dev Team |
| `Occupant1Contact` | Defaults to tenant's `CellNo` | Capture individual contact numbers | Dev Team |
| `Occupant1Salary` | `0.00` | Add salary capture to application form | Dev Team |
| `Occupant2Relationship` | `"Family Member"` (generic) | Same as Occupant1 | Dev Team |
| `Occupant2Contact` | Defaults to tenant's `CellNo` | Same as Occupant1 | Dev Team |
| `Occupant2Salary` | `0.00` | Same as Occupant1 | Dev Team |
| `Occupant3Relationship` | `"Family Member"` (generic) | Same as Occupant1 | Dev Team |
| `Occupant3Contact` | Defaults to tenant's `CellNo` | Same as Occupant1 | Dev Team |
| `Occupant3Salary` | `0.00` | Same as Occupant1 | Dev Team |

**Implementation Notes:**
- Update `PropertyLeaseAgreementMaster` table:
  - Add: `OccupantONERelationship` (string)
  - Add: `OccupantONEContact` (string)
  - Add: `OccupantONESalary` (decimal)
  - Add: `OccupantTWORelationship` (string)
  - Add: `OccupantTWOContact` (string)
  - Add: `OccupantTWOSalary` (decimal)
  - Add: `OccupantTHREERelationship` (string)
  - Add: `OccupantTHREEContact` (string)
  - Add: `OccupantTHREESalary` (decimal)

- Update lease agreement capture form view:
  - Add dropdown for `Relationship` (Spouse, Child, Parent, Sibling, Other)
  - Add text input for `Contact` (phone number)
  - Add numeric input for `Salary`

**Questions to Answer:**
1. Is occupant salary required for affordability calculations?
2. Should combined household income be validated against unit rental?
3. Do occupants need to sign separate declarations?

---

### 4. EMPLOYER & BANKING DETAILS (2 fields)
**Priority:** HIGH  
**Reason:** Required for debit order setup

| Field Name | Current Value | Action Required | Responsible Party |
|------------|---------------|-----------------|-------------------|
| `Employer` | `"To Be Captured"` | Add employer field to application form | Dev Team |
| `BankingDetails` | `"To Be Provided"` | Add banking details capture form | Dev Team + Finance |

**Implementation Notes:**
- Update `PropertyLeaseApplication` table:
  - Add: `EmployerName` (string, max 200)
  - Add: `EmployerAddress` (string, max 200)
  - Add: `EmployerContactNumber` (string, max 15)
  - Add: `BankName` (string, max 100)
  - Add: `BankAccountNumber` (string, max 20, encrypted)
  - Add: `BankBranchCode` (string, max 10)
  - Add: `AccountType` (string - "Cheque", "Savings")
  - Add: `AccountHolderName` (string, max 200)

- Create new view: `CaptureBankingDetails.cshtml`
- Add validation: Account holder name must match tenant name or spouse name

**Security Requirements:**
- Encrypt bank account number in database
- Implement PCI DSS compliance for bank data storage
- Add audit trail for bank details access

**Questions to Answer:**
1. Who verifies employer details (payslip required)?
2. Is bank account number stored encrypted?
3. Who sets up the debit order (tenant or finance team)?
4. What happens if debit order fails (grace period)?

---

## FIELDS DEFAULTED TO BLANK (Not Yet Defined)

### 5. WITNESS FIELDS (4 fields)
**Priority:** LOW  
**Reason:** Witnesses not currently required for digital signatures

| Field Name | Current Value | Action Required | Responsible Party |
|------------|---------------|-----------------|-------------------|
| `Witness1` | `""` (blank) | Determine if witnesses required for legal validity | Legal Team |
| `Witness2` | `""` (blank) | Same as Witness1 | Legal Team |
| `Witness3` | `""` (blank) | Same as Witness1 | Legal Team |
| `Witness4` | `""` (blank) | Same as Witness1 | Legal Team |

**Implementation Notes:**
- If witnesses required, add to `PropertyLeaseAgreementMaster`:
  - `TenantWitnessONE` (already exists)
  - `TenantWitnessTWO` (already exists)
  - `ManagersWitnessONE` (already exists)
  - `ManagersWitnessTWO` (already exists)
  - Add: `TenantWitnessONE_IDNo` (string)
  - Add: `TenantWitnessTWO_IDNo` (string)
  - Add: `ManagersWitnessONE_IDNo` (string)
  - Add: `ManagersWitnessTWO_IDNo` (string)

**Questions to Answer:**
1. Are physical witnesses legally required for lease agreements in South Africa?
2. Can digital signatures replace physical witness signatures?
3. If required, who can be a witness (COE staff, tenant's family, anyone)?

---

### 6. HEADING COLUMN FIELDS (4 fields) - ✅ RESOLVED
**Priority:** N/A  
**Reason:** User confirmed these are heading/column labels in the PDF template

| Field Name | Current Value | Purpose | Status |
|------------|---------------|---------|--------|
| `Subject` | `""` (blank) | Heading column label | ✅ LEAVE BLANK |
| `Description` | `""` (blank) | Heading column label | ✅ LEAVE BLANK |
| `Item` | `""` (blank) | Heading column label | ✅ LEAVE BLANK |
| `Item_2` | `""` (blank) | Heading column label | ✅ LEAVE BLANK |

**Resolution:**
User clarified that these fields are just heading columns in the PDF template. They should remain blank but have variables defined in case they're needed in the future. No action required.

---

## IMPLEMENTATION TIMELINE

### PHASE 1: IMMEDIATE (Week 1-2)
- [ ] Implement Employer & Banking Details capture (Priority: HIGH)
- [ ] Implement Occupant Relationship/Contact/Salary fields (Priority: HIGH)
- [ ] Clarify unknown fields: Subject, Description, Item, Item_2 (Priority: CRITICAL)

### PHASE 2: SHORT-TERM (Week 3-4)
- [ ] Meet with Finance Team to clarify subsidy calculation logic
- [ ] Update database schema to add subsidy tracking tables
- [ ] Implement subsidy calculation in lease generation

### PHASE 3: MEDIUM-TERM (Month 2)
- [ ] Negotiate DSTV service provider agreement
- [ ] Add DSTV service tracking to system
- [ ] Update lease generation to include DSTV fees

### PHASE 4: LONG-TERM (Month 3+)
- [ ] Consult with Legal Team on witness requirements
- [ ] Implement witness capture if required
- [ ] Update digital signature workflow

---

## FIELD SUMMARY BY STATUS

| Status | Count | Fields |
|--------|-------|--------|
| ✅ **Implemented** | 51 | Category 1 + Category 2 basic mappings |
| ⏳ **Defaulted to 0** | 14 | Subsidies (9) + DSTV (5) |
| ⏳ **Defaulted to Placeholder** | 11 | Occupant details (9) + Employer/Banking (2) |
| ⏳ **Defaulted to Blank** | 8 | Witnesses (4) + Unknown (4) |
| ❓ **Awaiting Clarification** | 4 | Subject, Description, Item, Item_2 |
| **TOTAL** | **85** | |

---

## DEFERRED FIELD PRIORITIES

### 🔴 CRITICAL (Must implement before production launch)
1. `Employer` field - Required for tenant verification
2. `BankingDetails` field - Required for debit order setup
3. Clarify `Subject`, `Description`, `Item`, `Item_2` fields

### 🟠 HIGH (Implement within 1 month of launch)
1. Occupant Relationship/Contact/Salary fields
2. Subsidy calculation logic (if applicable)

### 🟡 MEDIUM (Implement within 3 months)
1. DSTV service tracking (if service offered)
2. Access Card tracking system

### 🟢 LOW (Future enhancement)
1. Witness signature capture (if legally required)

---

## TESTING CHECKLIST FOR DEFERRED FIELDS

When implementing each deferred field, verify:

- [ ] Database migration runs successfully
- [ ] Form validation works (required fields, format validation)
- [ ] Data saves to database correctly
- [ ] PDF field populates correctly
- [ ] Existing data doesn't break (backward compatibility)
- [ ] Audit trail captures changes
- [ ] User permissions restrict access appropriately

---

## CONTACT & RESPONSIBILITY MATRIX

| Domain | Responsible Party | Contact |
|--------|------------------|---------|
| **Subsidies** | Finance Team | finance@ekurhuleni.gov.za |
| **DSTV Service** | Management | management@ekurhuleni.gov.za |
| **Banking Details** | Finance Team + IT Security | fintech@ekurhuleni.gov.za |
| **Occupant Details** | Dev Team | dev@ekurhuleni.gov.za |
| **Legal Requirements** | Legal Team | legal@ekurhuleni.gov.za |
| **Employer Verification** | HR Team | hr@ekurhuleni.gov.za |

---

**Document Status:** ACTIVE TRACKING  
**Last Updated:** March 2026  
**Next Review Date:** After Phase 1 implementation complete  
**Prepared By:** GitHub Copilot
