# Test Users & Credentials

To support role-based routing and automated end-to-end testing, the database is seeded with dedicated test accounts representing each step of the lease lifecycle.

---

## 🔐 General Credential Settings

- **Default Password for All Accounts:** `Arsenal5@`
- **Application Portal URL (Local):** `http://localhost:3450/Account/Login`
- **Application Portal URL (Staging):** `http://10.1.2.136:9903/Account/Login`

---

## 👥 Seeded Test Users

| Username | Role | Dashboard Queue | Actions Performed |
|:---|:---|:---|:---|
| **`RealEstateCustomer`** | `Lease Applicant` | Client Inbox | Submits capture forms, checks application progress, confirms scheduled inspection dates, uploads banking details, signs contracts, and requests early occupation (PTO). |
| **`re_finance_officer`** | `Finance Administrator` | Payment Verification | Reviews uploaded application fee proof of payment (POP), adds payment comments, and approves/rejects payments. |
| **`re_property_officer`** | `Property Manager` | Risk Assessment / Reviews / Inspections / PTO | Conducts credit/CIPC vetting checks, recommends applications, initiates circulation, gathers department comments, schedules inspections, does walkthrough checks, and reviews PTO requests. |
| **`re_committee_member`** | `Area Manager` | Committee Resolutions | Captures DPRE Evaluation Committee meeting minutes, uploads resolution PDFs, and records decisions (Recommend, Defer, etc.). |
| **`re_hod`** | `Back Office System Administrator` | HoD Authorisations | Signs off final lease awards, signs off early occupation (PTO), signs final contracts. |
| **`re_facilities_manager`**| `Property & Facilities Manager`| Facilities Queue | Schedules preventive maintenance, assigns technicians to logged inspection defects, and signs off works order closures. |
| **`re_technician`** | `Caretaker` | My Tasks / Job Allocations | Accepts assigned repairs and uploads completed maintenance job sheets. |

---

## 🏢 Departmental Representatives

Circulation reviews are routed to individual clerks representing City of Ekurhuleni (CoE) departments. They all use the default password `Arsenal5@` and have the `Departmental Representative` role:

| Username | Department ID | Department Represented |
|:---|:---:|:---|
| **`re_city_planning`** | 1 | City Planning |
| **`re_legal`** | 2 | Corporate Legal Services |
| **`re_disaster`** | 3 | Disaster and Emergency Management |
| **`re_economic`** | 4 | Economic Development |
| **`re_empd`** | 6 | Ekurhuleni Metro Police Department (EMPD) |
| **`re_energy`** | 7 | Energy |
| **`re_environmental`** | 8 | Environmental Resource and Waste Management |
| **`re_finance_officer`** | 9 | Finance |
| **`re_health`** | 10 | Health and Social Development |
| **`re_human_settlements`**| 11 | Human Settlements |
| **`re_ict`** | 12 | Information and Communication Technology |
| **`re_roads`** | 13 | Roads and Stormwater |
| **`re_sports`** | 14 | Sports, Recreation Arts and Culture |
| **`re_transport`** | 15 | Transport Planning and Provision |

---

## ⚠️ Important Configuration Warning

> **DO NOT use the legacy admin account `BOSystemAdminstrator`** for testing Real Estate workflows. This account is mapped to the housing (EHC) branch queues and does not have the department constraints or layout styles configured for DPRE. Always use the specific role-based accounts listed above.
