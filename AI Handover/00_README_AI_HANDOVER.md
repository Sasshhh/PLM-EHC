# AI Handover Package — Property Lease Management (PLM) System
**Date Generated:** 2026-08-27  
**System:** City of Ekurhuleni (CoE) Property Lease Management v1  
**Repository:** `C:\REPO\PLM V1\PLM-EHC`

---

## 🎯 Purpose

This folder contains everything a new AI (or developer) needs to **pick up where the previous AI left off**. It is the single authoritative handover package for the Real Estate branch of the PLM system.

---

## 📁 Folder Contents & Reading Order

| # | File | Description |
|---|------|-------------|
| 1 | **`00_README_AI_HANDOVER.md`** | This file — start here |
| 2 | **`01_SYSTEM_ARCHITECTURE.md`** | Multi-department architecture, DepartmentID isolation, table naming conventions |
| 3 | **`02_UNIFIED_USE_CASES_REAL_ESTATE.md`** | The final, authoritative RE_UC001–RE_UC025 use case specification with controller/view mappings |
| 4 | **`03_FILE_MAP.md`** | Complete mapping of use cases → source files (Controllers, Views, Models, Helpers, SQL) |
| 5 | **`04_DATABASE_SCHEMA.md`** | All RE_ tables, columns, relationships, and audit tables |
| 6 | **`05_DATABASE_UPDATES_LOG.md`** | Chronological log of every DB update run directly against the database, with status of what was applied and what's pending |
| 7 | **`06_MASTERDATA_REFERENCE.md`** | All master data tables: statuses, responsibility types, action types, activity tracker messages, email content types, document types — and how they interconnect |
| 8 | **`07_CURRENT_STATE_AND_NEXT_STEPS.md`** | Where development stopped, what's complete, what's pending, known bugs |
| 9 | **`08_TEST_USERS_AND_CREDENTIALS.md`** | All seeded test users, passwords, roles, and which queues they serve |
| 10 | **`RE_Complete_Schema_And_Seed.sql`** | The single consolidated SQL script containing ALL schema + seed data for the Real Estate branch |

---

## ⚠️ Critical Rules for the Next AI

1. **NEVER modify shared layout files** (`RCS_Layout.cshtml`, global sidebar). All RE styling is scoped locally.
2. **ALL Real Estate tables MUST use the `RE_` prefix** (e.g., `RE_Applications`, `RE_Facilities`). EHC tables have no prefix as it was the first branch.
3. **DepartmentId = 3** for all Real Estate users and entities.
4. **Always use idempotent SQL** (`IF NOT EXISTS`) — scripts may be re-run on staging/production.
5. **Consult `02_UNIFIED_USE_CASES_REAL_ESTATE.md`** before modifying any workflow logic.
6. **The `Unified_Use_Cases.md`** in `Use cases/` is the EHC (Housing) branch source of truth — don't confuse it with the Real Estate version.

---

## 🔑 Quick Start Credentials

| User | Password | Purpose |
|------|----------|---------|
| `RealEstateCustomer` | `Arsenal5@` | Client applicant |
| `re_finance_officer` | `Arsenal5@` | Payment verification |
| `re_property_officer` | `Arsenal5@` | Risk assessment, reviews, inspections |
| `re_committee_member` | `Arsenal5@` | Committee resolutions |
| `re_hod` | `Arsenal5@` | Final authorisation |
| `re_facilities_manager` | `Arsenal5@` | Works orders |
| `re_technician` | `Arsenal5@` | Maintenance |
| `BOSystemAdminstrator` | `Arsenal5@` | Legacy EHC admin (do NOT use for RE workflows) |

---

## 🌐 Application URLs

| Environment | URL |
|-------------|-----|
| **Local Dev** | `http://localhost:3450/Account/Login` |
| **Staging/Production** | `http://10.1.2.136:9903/Account/Login` |
