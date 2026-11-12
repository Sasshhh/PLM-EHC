# EHC PLM — Requirements Backlog (Not Covered by Any Use Case)

> **Status:** Awaiting business approval before development is scheduled.
> **Generated:** 2026-09-24 from *CoE EHC-PLM Solution Design Specification V.0.14*.

## What this document is

The SDS lists a requirement catalogue (FR / BR / IR) and, separately, 38 use case
narratives. Each narrative declares the requirements it satisfies. The requirements
listed here appear in the catalogue but are **referenced by no use case**, so there is
no specified behaviour to build or test against.

This is a **specification gap, not a defect**. Some of these may already be partially
implemented; others were descoped and never removed from the catalogue. Each needs a
business decision: write a use case, mark it out of scope, or confirm it is delivered
elsewhere.

**These items are deliberately excluded from factory testing** and are marked
*Not Covered* on the Requirements Validation sheet of the factory test results.

## Summary

| Category | Total in SDS | Covered | Not covered |
|---|---:|---:|---:|
| Functional Requirements (leaf) | 96 | 74 | **22** |
| Business Rules | 43 | 41 | **2** |
| Integration Requirements | 14 | 4 | **10** |

## 1. Functional Requirements

### FR2 — Functionality to add AD users to the system (User Management)

*User management / Active Directory provisioning.*

| ID | Requirement | Decision needed |
|---|---|---|
| `FR2` | Functionality to add AD users to the system (User Management) | ☐ Write use case ☐ Out of scope ☐ Already delivered |

### FR8 — Rent and Arrears

*Rent & Arrears. Billing is largely handled by the CoE billing system; confirm how much PLM must own.*

| ID | Requirement | Decision needed |
|---|---|---|
| `FR8.1` | Capture Billing information including applicable penalties and rental escalations | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR8.4` | Provide the ability to set and amend rents for different property types (including affordable, social, and business) | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR8.5` | Functionality for both individual and bulk rental updates | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR8.7` | Ability to track lease billing and payment information | ☐ Write use case ☐ Out of scope ☐ Already delivered |

### FR9 — Credit Control

*Credit Control. Overlaps with IR06 Credit Control Management integration.*

| ID | Requirement | Decision needed |
|---|---|---|
| `FR9.1` | Provide simple, flexible, workflow-based functionality to enable management of arrears - current and former tenant and leasehold arrears. | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR9.2` | Highlight cases for action in relation to each stage of a relevant user specified escalation | ☐ Write use case ☐ Out of scope ☐ Already delivered |

### FR11 — Terminate lease functionality

*Lease termination. Most of FR11 is covered by UC025–UC030; this item is the exception.*

| ID | Requirement | Decision needed |
|---|---|---|
| `FR11.3` | Functionality to confirm that the tenant has vacated | ☐ Write use case ☐ Out of scope ☐ Already delivered |

### FR12 — Communication Functionality

*Communications. The largest gap — template management and scheduled messaging.*

| ID | Requirement | Decision needed |
|---|---|---|
| `FR12.1` | Schedule automatic communications (SMS, E-Mail, Letter, Tenant Portal) with the ability to create user-defined expressions to tailor contacts | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR12.2` | Functionality to set communication preferences per tenant, per type basis e.g., repairs by text, balance by email | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR12.4` | Functionality for relevant officials to amend document templates and where appropriate, individual documents before they are dispatched | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR12.5` | Copies of the communications will be stored and attached to appropriate records for future retrieval as part of that record. The system also automatically generates a daily report detailing the delivery status (e.g. success/failure) of all types of electronic communication | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR12.7` | Functionality to support standard communication template | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR12.8` | Functionality for relevant officials to create new, and amend existing templates | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR12.9` | Functionality to amend individual letters, text, or email communications created from a template before it is sent by relevant users | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR12.11` | Generate automatic appointment reminders to applicants and tenants based on entries in either an embedded appointment calendar or ideally interfacing to a shared Microsoft Outlook calendar | ☐ Write use case ☐ Out of scope ☐ Already delivered |

### FR13 — Electronic Document Management

*Electronic Document Management. Overlaps with IR07 Enterprise Records Management.*

| ID | Requirement | Decision needed |
|---|---|---|
| `FR13.1` | Functionality to store documents and images against a tenancy, leaseholder or property record for future retrieval as part of that record | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR13.2` | Automatically store all communications created through the system that contain attachments of any sort, documents, photographs etc. against the relevant tenant, or lease records for future retrieval as part of that record | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR13.3` | Automatically store any correspondence created through the system e.g., letters, against the relevant tenant or lease records for future retrieval as part of that record | ☐ Write use case ☐ Out of scope ☐ Already delivered |

### FR14 — Incidents and Case Management

*Incidents & Case Management. Partially delivered via UC19D/UC19E service requests.*

| ID | Requirement | Decision needed |
|---|---|---|
| `FR14.4` | Flexible work path with simple configurable stages along with associated notes, alerts, tasks and diary entries | ☐ Write use case ☐ Out of scope ☐ Already delivered |

### FR18 — Notes Functionality

*Notes. No use case describes note capture or the integrated note view.*

| ID | Requirement | Decision needed |
|---|---|---|
| `FR18.1` | Functionality to create notes in respect of tenants, residents, properties and associated events, including visits, customer contacts, phone calls etc. | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `FR18.2` | Provide an integrated view of notes and associated events | ☐ Write use case ☐ Out of scope ☐ Already delivered |

## 2. Business Rules

| ID | Rule | Decision needed |
|---|---|---|
| `BR10` | Call reference number (Service Request Number): Call reference number of tenants and applicants must be unique. Should be open until the call is closed. | ☐ Write use case ☐ Out of scope ☐ Already delivered |
| `BR38` | Service Request Closure: Service request tickets can be closed only if the ticket status is Resolved or Deleted | ☐ Write use case ☐ Out of scope ☐ Already delivered |

## 3. Integration Requirements

Ten of the fourteen integrations in the SDS are named but never referenced by a use
case. None can be factory tested without a specified interaction and an available
endpoint.

| ID | External system | Decision needed |
|---|---|---|
| `IR01` | Identity and Access Management System | ☐ Write use case ☐ Out of scope ☐ Deferred to later phase |
| `IR02` | Solar Asset Management System (SAMS) | ☐ Write use case ☐ Out of scope ☐ Deferred to later phase |
| `IR03` | Customer Relationship Management and Digital Experience (CRM DX) System | ☐ Write use case ☐ Out of scope ☐ Deferred to later phase |
| `IR05` | Expenditure Management | ☐ Write use case ☐ Out of scope ☐ Deferred to later phase |
| `IR06` | Credit Control Management | ☐ Write use case ☐ Out of scope ☐ Deferred to later phase |
| `IR07` | Enterprise Records Management System | ☐ Write use case ☐ Out of scope ☐ Deferred to later phase |
| `IR08` | Committee Management System | ☐ Write use case ☐ Out of scope ☐ Deferred to later phase |
| `IR10` | Home Affairs | ☐ Write use case ☐ Out of scope ☐ Deferred to later phase |
| `IR11` | Deeds | ☐ Write use case ☐ Out of scope ☐ Deferred to later phase |
| `IR12` | South African Social Security Agency | ☐ Write use case ☐ Out of scope ☐ Deferred to later phase |

## 4. Covered integrations, for contrast

| ID | External system | Referenced by |
|---|---|---|
| `IR04` | Billing and Revenue Management | UC020, UC021 |
| `IR09` | Credit Bureaus Tenant Network Profile | UC06 |
| `IR13` | Microsoft Outlook | UC011, UC012, UC029, UC030, UC031, UC032 |
| `IR14` | ARCGIS | UC03, UC07 |

---

*Maintenance: regenerate when the SDS version changes. The matching factory test
workbook is `Factory Testing/EHC_Factory_Test_Results_V0.1.xlsx`.*
