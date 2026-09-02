# Real Estate Development (RED / DPRE) — Unified Use Cases & System Specification

**Target Audience:** Business Analyst (Ayanda Giyose), Project Stakeholders, Development Team  
**System Module:** Property Lease Management (PLM) — Development Planning & Real Estate (DPRE)  
**Date:** July 2026  
**Document Status:** Approved Specification & Implementation Mapping  

---

## 📌 Executive Summary & Architectural Standards

This document establishes the **Unified Use Case Specification & Data Mapping** for the Real Estate Development (RED) module within the City of Ekurhuleni (CoE) Property Lease Management (PLM) system.

### Core Architectural Isolation Rules
1. **Database Schema**: All entities operate exclusively on isolated `RE_*` tables (`RE_Applications`, `RE_Facilities`, `RE_FacilityCategories`, `RE_FacilityUnits`).
2. **Controllers & Views**: Customer-facing workflows are contained in `RealEstateController.cs` (`Views/RealEstate/`), and back-office operations in `RealEstateAdminController.cs` (`Views/RealEstateAdmin/`).
3. **Visual Branding**: Styled locally using Onyx Obsidian gold theme CSS tokens (`var(--re-gold)`, `#c59b27`, `#d2930b`) without altering shared global layout files (`RCS_Layout.cshtml`).

---

## 📋 Comprehensive Unified Use Cases (RE_UC001 — RE_UC027)

### RE_UC001 — RE_UC005: Property Onboarding & Dynamic Lease Application Capture
* **Actors**: Applicant (Customer), Real Estate Officer
* **Controllers & Views**: `RealEstateController.cs` · `Views/RealEstate/Capture.cshtml`, `Inbox.cshtml`, `MyApplications.cshtml`
* **Key Features & Rules**:
  * **Cascading Inventory Dropdowns**: Customer Care Centre (CCC Area) $\rightarrow$ Facility Site $\rightarrow$ Unit Let-Space.
  * **Multi-Unit Selection Grid**: Applicants can select and stack multiple different unit types (or units across different facilities) on a single application.
  * **Live Pricing Calculation Engine**:
    $$\text{Monthly Rental} = \text{Unit Size (m}^2\text{)} \times \text{Gazetted Tariff (R/m}^2\text{)} \times \text{Quantity}$$
  * **Pre-Qualification Document Uploads**: 12 inline document uploads (Business Plan, Profile, Tax/BEE, MBD 4, CSD, Financials, ID, etc.).

---

### RE_UC006 — RE_UC010: Application Fee Verification, Risk Assessment & Departmental Reviews
* **Actors**: Finance Administrator, Assessment Officer, Departmental Representatives, Committee Members
* **Controllers & Views**: `RealEstateAdminController.cs` · `ApplicationFeePayments.cshtml`, `RiskAssessments.cshtml`, `DepartmentalReviews.cshtml`
* **Key Features & Rules**:
  * **Application Fee Verification (RE_UC006)**: Finance officer approves or rejects proof of payment.
  * **Risk Assessment Outcome (RE_UC007)**: Assessment officer captures Credit Bureau, Home Affairs, Deeds Office, SASSA, and CIPC verification results.
  * **Departmental In-Circulation Review (RE_UC008 - RE_UC010)**: Parallel review routing to active CoE departments (Human Settlements, Environmental Development, Electricity, Water & Sanitation, Roads & Stormwater).

---

### RE_UC020: Evaluation Criteria Navigation & Committee Screening Workspace
* **Actors**: Client Services Officer, Working Committee, Evaluation Committee
* **Controllers & Views**: `RealEstateAdminController.cs` (`EvaluationCriteria` action) · `Views/RealEstateAdmin/EvaluationCriteria.cshtml`
* **Navigation Location**: Top-level sidebar menu item: **"Evaluation Criteria"** containing 3 dedicated sub-tabs:

#### 🔹 Tab 1 — Pre-Qualification Documents (Fits into UC 01 / UC 05 Pre-Screening)
Guideline and compliance master checklist for the 12 mandatory applicant uploads:
1. Comprehensive Business Plan
2. Company Profile
3. Company Registration Documents (Tax Clearance, B-BBEE Certificate/Affidavit, CIPC)
4. Declaration of Interest (Form MBD 4)
5. Central Suppliers Database (CSD) Registration Report
6. 3-Years Audited Financial Statements / Management Accounts
7. Proof of Business Location
8. Certified Identity Document (Not older than 6 months)
9. Facilities Management Experience & Accreditation
10. Letter of Funding / Bank Intent
11. Strategic Partnerships & Alliances
12. Ownership & Job Creation Plan

#### 🔹 Tab 2 — Pre-Qualification Evaluation Scoring Matrix (Fits into UC 04 Functional Assessment)
Interactive functional scorecard for assessment officers (Minimum passing threshold: 60 / 90 Points):
* **Track Record & Experience** (15 Points Max):
  * $11+$ Years = 15 Pts | $6 - 10$ Years = 13 Pts | $3 - 5$ Years = 5 Pts | No Submission = 0 Pts.
  * *Evidence*: 3 years financials & 3 testimonial letters.
* **Financial Stability** (20 Points Max):
  * $\text{R}1,500,000+$ Funding = 20 Pts | $\text{R}1,000,000+$ = 15 Pts | $\text{R}750,000+$ = 12 Pts | $\text{R}500,000+$ = 10 Pts | No Funding = 0 Pts.
  * *Evidence*: Bank statement, bank guarantee, or letter of intent.
* **Business Case** (25 Points Max): Strategic Plan (10 Pts), Sector Analysis (5 Pts), Financial Model (10 Pts).
* **Marketing Plan** (20 Points Max): Marketing Methods (10 Pts), Strategic Partnerships (10 Pts).
* **Operations Plan** (10 Points Max): Enterprise Development (5 Pts), Facility Management Strategy (5 Pts).

#### 🔹 Tab 3 — Checklist for Committee Evaluations (Fits into UC 20 Screening & Adjudication)
* **Working Committee Screening Tool**: Interactive compliance checklist (**Attached / Not Attached**) for 11 compliance documents, screening officer sign-off, date, and Declined (Yes/No) toggle.
* **Evaluation Committee Adjudication Tool**: Job Creation impact scoring calculator:
  * $15+$ Jobs Created = 50 Pts | $10 - 14$ Jobs = 40 Pts | $5 - 9$ Jobs = 30 Pts | $2 - 4$ Jobs = 20 Pts.

---

### RE_UC021 — RE_UC025: Lease Administration & User Agreement Contract Execution

#### 📄 RE_UC021: Permission to Occupy (PTO) Certificate & HOD Signature
* **Document Template**: `Updated User Agreement.doc` (Pre-populated PTO Letter).
* **Validity Duration**: **Valid for a Maximum of 12 Months** (Business Rule BR05).
* **Primary Actors**: Property Officer (Drafts PTO), HOD (Signs PTO Canvas / Authorizes).
* **Workflow**:
  1. Officer selects approved application on `/RealEstateAdmin/PtoApprovals` $\rightarrow$ Clicks **Generate PTO Certificate**.
  2. System pre-populates PTO details (Tenant, Unit, Start/End Dates, Purpose).
  3. Officer reviews draft $\rightarrow$ "Are you sure you want to submit the document for signature?".
  4. HOD reviews on `/RealEstateAdmin/PtoSignatureQueue` $\rightarrow$ Authorizes & signs.

#### 📄 RE_UC023: Conclude Full 36-Month Lease Agreement & Dual Signing Modes
* **Document Template**: `URC User Agreement_Latest.doc` (Standardised Municipal User Agreement).
* **Validity Duration**: **Valid for 36 Months (3 Years)**.
* **Primary Actors**: Property Officer, Tenant, Witnesses, HOD.
* **Dual Tenant Signing Modes**:
  * 👁️ **View Agreement**: Displays pre-populated agreement directly on-screen for instant review.
  * 📥 **Mode 1 — Offline Sign (Download & Upload)**: Downloads pre-filled agreement PDF to tenant's device $\rightarrow$ Tenant signs offline together with 2 witnesses $\rightarrow$ Uploads scanned PDF back into system.
  * ✍️ **Mode 2 — Online Sign (Digital Signature Canvas)**: Tenant draws digital signature directly on canvas element.
* **HOD Execution**: HOD reviews tenant signature, signs canvas, and confirms via `#modalHodConfirm`.

#### 🔄 Lifecycle & Transition Rule (12-Month PTO $\rightarrow$ 36-Month Full Lease)
> **Transition Rule**: The system initially issues the 12-month PTO Certificate (`Updated User Agreement.doc`) for temporary occupation. When the back-office completes full processing and approves the 36-month Full Lease Agreement (`URC User Agreement_Latest.doc` under UC23), the system automatically flags the 12-month PTO as **Superseded & Inactive**, transitioning the tenant to active 36-month tenancy.

#### 📍 RE_UC024 & RE_UC025: Unit Allocation & Tenancy Reference Assignment
* **RE_UC024 (Space Allocation)**: Allocates active `RE_FacilityUnits` space to the lease on `/RealEstateAdmin/Allocations`.
* **RE_UC025 (Lease Classification)**: Captures tenancy classification and assigns unique reference number (`UTLN-2026-00066`).

---

## 📊 Inventory & Pricing Matrix (SDS Master Data)

| Category / Facility Grade | Gazetted Tariff Rate (VAT Inclusive) | Example Sites & Locations |
| :--- | :--- | :--- |
| **Township Industrial Parks** | **R 57.00 / m²** | Fannie Malape (Tokoza), Bhelekazi Fulathela Hive (Katlehong), Oscar Mabika Hive (Daveyton), Motsu & Sethokga Buy Back Centres (Thembisa), Sedibeng Hive |
| **Township Business Hubs** | **R 57.00 / m²** | Tokoza Traders Market, Tsakane Business Park, KwaThema Business Park, Springs Traders Market, Brakpan Civic Kiosk, Barcelona Market (Etwatwa), Reiger Park Enterprise Hub (Boksburg), Nigel Traders Market |
| **Township Automotive Hubs** | **R 67.00 / m²** | Katlehong Automotive Manufacturing Hub, Vosloorus Skills Centre |
| **FabLab Facilities** | **R 36.00 / m²** | Thokoza FabLab, Tembisa FabLab, Tsakane FabLab, Duduza FabLab, Vosloorus FabLab |
| **Incubation Farms / Agri-Parks** | **R 0.70 / m²** | Essellen Park Incubation Farm (Thembisa), Spaarwater Incubation Farm (Duduza), Etwatwa Unserviced Agri Portions |

---

## 📁 Repository Deliverables & Test Artifact Links

1. **Copied Template Files**:
   * [PDFTemplates/RealEstate/Updated User Agreement.doc](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/PDFTemplates/RealEstate/Updated%20User%20Agreement.doc) (12-Month PTO Template)
   * [PDFTemplates/RealEstate/URC User Agreement_Latest.doc](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/PDFTemplates/RealEstate/URC%20User%20Agreement_Latest.doc) (36-Month Lease Template)
   * [PDFTemplates/RealEstate/SDS Input Sheet (1).xlsx](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/PDFTemplates/RealEstate/SDS%20Input%20Sheet%20(1).xlsx) (Master Data Sheet)

2. **Source Code & Controller Updates**:
   * [RealEstateAdminController.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Controllers/RealEstateAdminController.cs#L2367-L2425) (`DownloadPtoCertificatePdf` & `DownloadLeaseAgreementPdf` endpoints)
   * [RealEstateUserAgreementHelper.cs](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Helpers/RealEstateUserAgreementHelper.cs) (Dynamic PDF Generation & Lifecycle comments)
   * [EvaluationCriteria.cshtml](file:///c:/REPO/PLM%20V1/PLM-EHC/C8.eServices.Mvc/Views/RealEstateAdmin/EvaluationCriteria.cshtml) (3-Tab Evaluation & Screening Workspace)
