# Walkthrough: Voluntary Lease Termination Authorization Workflow

This walkthrough details the successful end-to-end implementation and verification of the **Voluntary Lease Termination Appraisal** workflow for the Revenue Manager. 

The shared `PropertyEvictionValidation` view was fully enhanced to conditionally adapt to the voluntary authorization dashboard instead of standard eviction actions, providing a premium, fully auditing interface for the Revenue Manager.

---

## 1. Database & Model Additions

We successfully extended the database and C# model layers to capture and persist the Revenue Manager's signature details for full traceability:
* **Database Migration:** Added `RevenueManagerSignature` (NVARCHAR(MAX)), `RevenueManagerOfficialNumber` (NVARCHAR(50)), and `RevenueManagerSignDate` (DATETIME) to the `LeaseTerminations` table.
* **C# Model (`LeaseTermination.cs`):** Mapped C# properties for the three new database fields.

---

## 2. Backend Routing and Workflow Logic (`PropertyLeaseApplicationController.cs`)

* **GET Action (`PropertyEvictionValidation`):** 
  * Loaded the tenant's uploaded voluntary termination **Proof of Banking Details** (`dt_banking_details_proof`) document using `MatchingHelper.DocumentUploadBakingDetailsProof` and successfully assigned it to `ViewBag.BankingDetailsDvm`.
* **POST Action (`PropertyEvictionValidation`):**
  * Extended the action parameters to accept `OfficialNumber` and `hdnSignatureBlob`.
  * Saved the digital signature, official number, and sign date directly to the `LeaseTermination` entity.
  * Conditional Workflow Status Transitions:
    * **Approved:** Lease status transitions to `s_awaiting_exit_inspec` (`In Awaiting Exit Inspection`, ID 255) rather than eviction service.
    * **Rejected:** Returns to `s_awaiting_termination_approval` (back to CSO, ID 248) and requires a comment.
    * **Legal Referral:** Status changes to `s_legal_referral_pending` and generates an eviction reference number.

---

## 3. Frontend Enhancement (`PropertyEvictionValidation.cshtml`)

* **Nesting-Compliant Architecture:** Replaced the Razor `@using (Html.BeginForm())` helper with a standard flat HTML `<form>` tag to avoid block-scoped scoping compile-time errors.
* **Account Validation Panel:** Implemented a dynamic **Account Validation** button to toggle the visibility of the tenant's uploaded read-only "Proof of Banking Details" panel.
* **Interactive Signature Pad:** Integrated an interactive HTML5 Canvas signature pad for capturing digital signatures, with responsive clear and touch support.
* **Validation & Dynamic UI Feedback:**
  * Implemented an outcome dropdown change handler that dynamically sets a red mandatory star indicator on the comments block when `Rejected` is selected.
  * Form submission validation checks that the Official Number, Signature Pad, and Outcome fields are completely populated for voluntary terminations.

---

## 4. End-to-End Verification

End-to-end flow execution was verified via a local IIS browser subagent:
* **Actor:** Revenue Manager (`COESolarDev10`)
* **Lease Reference:** `EHC2026032600001` (Lease ID: `2117`)
* **Status Before:** `Awaiting Termination Appraisal`
* **Actions Performed:** 
  1. Opened **Authorize Termination** view.
  2. Clicked **Account Validation** to verify the banking details document.
  3. Drew a signature on the signature pad.
  4. Input the CSO official number: `985532`.
  5. Selected **Approved** and successfully confirmed submission.
* **Status After:** `In Awaiting Exit Inspection` (Action: **Conduct Exit Inspection**)
* **Persisted Database Fields:**
  * `RevenueManagerOfficialNumber`: `985532`
  * `RevenueManagerSignDate`: `2026-05-22 11:16:07`
  * `RevenueManagerSignature`: Saved as a base64 image data URL!

### Recorded Verification Session Video

The automated verification session recording has been captured successfully:
![Verification Session Recording](file:///C:/Users/sashe/.gemini/antigravity/brain/c455cb19-1f55-4a31-8e90-d9c1c670f682/login_and_inspect_1779441019750.webp)

---
