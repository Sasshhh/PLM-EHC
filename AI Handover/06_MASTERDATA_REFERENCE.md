# Master Data & Metadata Reference

This document serves as a comprehensive catalog of all system lookup metadata, status keys, action outcomes, and document definitions used by the Real Estate branch.

---

## 1. Workflow Status Keys (`dbo.Status`)

These statuses manage the state machine of the Real Estate lease applications lifecycle.

| Status Key | Name / Description | Phase |
|:---|:---|:---|
| `s_awaiting_application_fee_validation` | In Awaiting Application Fee Validation | 1. Capture & Payment |
| `s_awaiting_risk_assessment` | Awaiting Risk Assessment Outcome | 2. Vetting |
| `s_property_verified` | Property Verified | 3. Evaluation |
| `re_in_circulation_for_evaluation` | In Circulation for Evaluation | 3. Department Review |
| `re_supported` | Supported | 3. Department Comment |
| `re_supported_conditions` | Supported with Conditions | 3. Department Comment |
| `re_not_supported` | Not Supported | 3. Department Comment |
| `re_additional_info_req` | Additional Information Required | 3. Department Comment |
| `re_pending_committee_outcome` | Pending Committee Outcome | 4. Committee |
| `re_recommended` | Recommended | 4. Committee Resolution |
| `re_recommended_conditions` | Recommended with Conditions | 4. Committee Resolution |
| `re_not_recommended` | Not Recommended | 4. Committee Resolution |
| `re_deferred` | Deferred | 4. Committee Resolution |
| `s_awaiting_hod_response` | In Awaiting HoD Response | 4. HOD Review |
| `re_concluded_approved` | Concluded Approved | 5. HoD Decision |
| `re_concluded_rejected` | Concluded Rejected | 5. HoD Decision |
| `re_awaiting_inspection` | Awaiting Inspection | 6. Inspection |
| `re_awaiting_agreement_conclusion` | Awaiting Lease/User Agreement Conclusion | 6. Contract Signing |
| `re_awaiting_agreement_conclusion_outcome` | Awaiting Lease/User Agreement Conclusion Outcome | 6. Tenant Signed |
| `re_pending_activation` | Pending Activation | 6. HOD Signed |
| `re_active_occupancy` | Active Occupancy | 6. Unit Allocated |
| `re_active` | Active | 6. Tenancy active |
| `re_awaiting_pto_review` | Awaiting PTO Review | 7. Early Access |
| `re_awaiting_pto_approval` | Awaiting PTO Approval | 7. Early Access Review |
| `re_awaiting_pto_signature` | Awaiting PTO Signature | 7. Early Access Authorized |
| `re_pto_approved` | PTO Approved | 7. Early Access Active |
| `re_pto_approved_conditions` | PTO Approved with Conditions | 7. Early Access Active |
| `re_pto_rejected` | PTO Rejected | 7. Early Access Rejected |
| `re_pto_additional_info` | PTO Additional Info Required | 7. Early Access Query |
| `re_revoked_pending_review` | Revoked, Pending Review | 8. Termination |
| `re_expired` | Expired | 8. Expiry |

---

## 2. Responsibility Types (`dbo.ResponsibilityTypes`)

Responsibility keys route items into specific user queues on back-office dashboards.

* **`r_finance_admin`** — Finance Administrator queue (Payment verification)
* **`r_property_manager`** — Property Manager queue (Risk assessments, circulation reviews)
* **`r_area_manager`** — Area Manager queue (Committee reviews)
* **`r_bo_sys_admin`** — HOD queue (Final approvals, PTO authorizations)
* **`r_prop_fac_manager`** — Property & Facilities Manager queue (Inspection schedules, works orders)
* **`r_caretaker`** — Caretaker / Technician queue (Conducting walkthroughs, executing repairs)
* **`r_dept_rep`** — Departmental Representative queue (Circulation comment capture)

---

## 3. RCS Action Types (`dbo.RCSActionTypes`)

Determines the outcome type of a back-office official's decision submission.

* **`Approved`** — Move application forward
* **`Rejected`** — Disregard/turn down application
* **`Supported`** — Circulation support
* **`Supported with Conditions`** — Circulation support with stipulations
* **`Not Supported`** — Circulation disapproval
* **`Request Info`** — Send back to applicant with comment queries

---

## 4. Document Types (`dbo.DocumentTypes`)

Classifies uploaded PDF/image binary streams inside the file attachment tables.

1. **`dt_business_plan`** — Comprehensive Business Plan
2. **`dt_company_profile`** — Company Profile
3. **`dt_cipc_registration`** — CIPC CoR14.3 Registration Certificate
4. **`dt_tax_clearance`** — SARS Tax Pin Clearance PIN
5. **`dt_bee_certificate`** — B-BBEE Certificate / Sworn Affidavit
6. **`dt_declaration_interest`** — Form MBD 4 Declaration of Interest
7. **`dt_csd_registration`** — Central Suppliers Database (CSD) Report
8. **`dt_audited_financials`** — 3-Years Audited Financial Statements
9. **`dt_proof_residence`** — Municipal account utility statement
10. **`dt_identity_document`** — Certified South African Identity Document
11. **`dt_experience_accreditation`** — Facilities Management Accreditation
12. **`dt_funding_intent`** — Bank Letter of Funding Intent
13. **`dt_proof_of_payment`** — Proof of Application Fee Payment
14. **`dt_pto_evidence`** — Certificate of Liability Insurance, Fit-out Plans, Health & Safety Clearances
15. **`dt_signed_agreement`** — Tenant scanned executed lease agreement PDF
16. **`dt_inspection_sheet`** — Signed walkthrough checklist PDF
17. **`dt_technician_jobsheet`** — Completed works order job sheet PDF

---

## 5. System Mappings & Dependencies

The diagram below highlights how these metadata lookup tables connect to route a Real Estate application through its lifecycle:

```
                  ┌───────────────────────┐
                  │    RE_Applications    │
                  └───────────┬───────────┘
                              │
            ┌─────────────────┼─────────────────┐
            ▼                 ▼                 ▼
     ┌─────────────┐   ┌─────────────┐   ┌─────────────┐
     │  StatusId   │   │  Selected   │   │ SystemUserId│
     └──────┬──────┘   │ FacilityId  │   └──────┬──────┘
            │          └──────┬──────┘          │
            ▼                 ▼                 ▼
     ┌─────────────┐   ┌─────────────┐   ┌─────────────┐
     │   Status    │   │RE_Facilities│   │ SystemUsers │
     └─────────────┘   └─────────────┘   └─────────────┘
                              │
                              ▼
                       ┌─────────────┐
                       │ RE_Facility │
                       │    Units    │
                       └─────────────┘
```

When an application is in circulation:
- A comment is added to `RE_DepartmentalComments`.
- The reviewer represents a `DepartmentsCoEs` department.
- The reviewer's account is mapped in `AspNetUserRoles` to the `Departmental Representative` role.
- Upon HOD sign-off, dynamic Word document templates are pre-populated using the application's properties and converted to PDF downloads via `Rotativa`.
