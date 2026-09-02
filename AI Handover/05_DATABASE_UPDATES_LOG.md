# Database Updates Log

This document lists the database migrations and updates performed directly on the SQL Server instance during development. It tracks what has been applied and where the database setup stands.

---

## 1. Migration History & Sequence

The following scripts were run sequentially to initialize and evolve the Real Estate Development database:

| Step | Script File | Target Database | Description / Modifications | Applied Status |
|:---:|:---|:---|:---|:---:|
| **1** | [db/unified/setup.sql](file:///c:/REPO/PLM%20V1/db/unified/setup.sql) | `PropertyLeaseManagementRealEstate` | Initializes the complete core schema: `RE_FacilityCategories`, `RE_Facilities`, `RE_FacilityUnits`, `RE_Applications`, `RE_DepartmentalComments`, and all matching audit tables. Seeds initial CCCs, categories, facilities, letting units, default roles (`Caretaker`), and base test users (`RealEstateCustomer`, `re_finance_officer`, `re_property_officer`, `re_committee_member`, `re_hod`, `re_facilities_manager`, `re_technician`). | **✅ Applied** |
| **2** | [db/migrations/20260630_make_banking_columns_nullable.sql](file:///c:/REPO/PLM%20V1/db/migrations/20260630_make_banking_columns_nullable.sql) | `PropertyLeaseManagementRealEstate` | Alters banking columns (`BankName`, `BankAccountType`, `BankAccountName`, `BankAccountNumber`, `BankBranchCode`) in both `RE_Applications` and `RE_ApplicationsAudit` to `NULL`. Since the system only collects lease details and no client banking info on the front-end, these fields must be nullable to satisfy C# model binding. | **✅ Applied** |
| **3** | [DatabaseScripts/RE_WorkflowSetup_Data.sql](file:///c:/REPO/PLM%20V1/DatabaseScripts/RE_WorkflowSetup_Data.sql) | `PropertyLeaseManagementRealEstate` | Seeds 13 departmental representative users and assigns them the `Departmental Representative` role. Maps the representative SystemUsers to the CoE Vetting Departments inside the shared `dbo.DepartmentsCoEs` table, ensuring inter-departmental circulation works. | **✅ Applied** |
| **4** | [DatabaseScripts/RE_UseCases_21_25_Setup.sql](file:///c:/REPO/PLM%20V1/DatabaseScripts/RE_UseCases_21_25_Setup.sql) | `PropertyLeaseManagementRealEstate` | Adds 17 new columns to `RE_Applications` and `RE_ApplicationsAudit` to support the early occupation (PTO) and lease agreement signing phases (e.g., `PtoRevocationReason`, `LeaseAgreementFileId`, `LeaseCategory`, `UniqueTenancyLeaseNumber`, etc.). Seeds the 7 final status keys (`re_awaiting_pto_signature`, `re_active_occupancy`, etc.) under `StatusTypeId = 20`. | **✅ Applied** |
| **5** | **Consolidated Script** (AI Handover folder) | `PropertyLeaseManagementRealEstate` | Combines the above scripts (Steps 1–4) into a single unified deployment file for onboarding new environments. | **✅ Prepared** |

---

## 2. Where We Stopped

The database has been fully updated and matches the model declarations in the source code. All EF migrations have been synchronized. The schema is complete up to **RE_UC025**.

### ⚠️ Current Active Database State
- Database Name: `PropertyLeaseManagementRealEstate`
- Core Tables: `13`
- Vetting Departments Configured: `14`
- Active Statuses Configured: `29` (under `st_rcs` / `st_customer_account` / `StatusTypeId = 20`)
- Seeded Test Accounts: `19`

### 🔧 Failsafe Maintenance Tasks (Before resuming E2E tests)
If the local database is ever reset or re-created, execute the scripts in the following order:
1. Run `setup.sql` to build tables & seed users.
2. Run `20260630_make_banking_columns_nullable.sql`.
3. Run `RE_WorkflowSetup_Data.sql`.
4. Run `RE_UseCases_21_25_Setup.sql`.
5. Alternatively, run the single consolidated script: [RE_Complete_Schema_And_Seed.sql](file:///c:/REPO/PLM%20V1/PLM-EHC/AI%20Handover/RE_Complete_Schema_And_Seed.sql).

---

## 3. Migration Strategy Strategy (Direct SQL vs. Code First)

**CRITICAL CONTEXT FOR NEXT DEVELOPER/AI:**
Historically, this project used Entity Framework Code First migrations. However, during the recent rapid development phase for the Real Estate branch, **we bypassed Code First and executed schema changes directly against the SQL Server via the scripts listed above.**

Because of this direct SQL approach, the EF `__MigrationHistory` table and the current Code First snapshot may be slightly out of sync with the actual database schema.

### How to Proceed (Meeting in the Middle)
Moving forward, we want to return to the standard Code First workflow. To get the dev server back in line:
1. **Do not write direct SQL scripts for new schema changes.**
2. Instead, update the C# Models in `Models/RE_*.cs`.
3. Run the standard Package Manager Console command: `Add-Migration <MigrationName>`
4. **Carefully review the generated migration code.** Since the database already has the tables (created via our SQL scripts), the newly generated migration might try to create tables that already exist. You will need to manually edit the generated `Up()` and `Down()` methods in the migration file to ignore the tables/columns we already created via SQL, so that running `Update-Database` doesn't crash with "Object already exists" errors.
5. Once the EF snapshot is re-aligned with the database reality, you can resume normal Code First operations exclusively.
