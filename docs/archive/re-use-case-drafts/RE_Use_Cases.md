# Real Estate Development (RED) Use Cases Stub

This document contains stubs and templates for the upcoming Real Estate Development workflows. Use these templates to draft and define the business rules and success scenarios for your RED applications.

---

## RE_UC001: Capture Real Estate Property details
* **Use Case Name:** Capture Real Estate Property details
* **Use Case Number:** RE_UC001
* **Use Description:** Enables the Real Estate Officer/Admin to onboard a new property or land parcel into the Real Estate inventory.
* **Primary Actor:** Real Estate Officer
* **Pre-condition(s):** User logged into the system with appropriate role.
* **Success End Condition:** Property successfully registered and flagged as available in the database.
* **Main Success Scenario:**
  1. Selects **Real Estate Applications** from the side menu $\rightarrow$ Click **Onboard Property**.
  2. Enters property details (Location, Dimensions, Intended Use, Valued Price).
  3. Clicks **Submit**.
  4. System validates inputs, creates a record in `RE_Properties`, and confirms success.

---

## RE_UC002: Submit Real Estate Application
* **Use Case Name:** Submit Real Estate Application
* **Use Case Number:** RE_UC002
* **Use Description:** The customer submits an application for leasing or purchasing a registered Real Estate property.
* **Primary Actor:** Customer (Real Estate Development Department)
* **Pre-condition(s):** Customer is active on the system and has been redirected to `/RealEstate/Inbox`.
* **Success End Condition:** Application created and sent to the Real Estate queue.
* **Main Success Scenario:**
  1. Customer lands on `/RealEstate/Inbox` $\rightarrow$ Click **New Application**.
  2. Selects desired property and uploads required documents (ID, Proof of Funds, Company Registration).
  3. Clicks **Submit**.
  4. System registers the application under status `AwaitingRealEstateOfficerReview` and routes it to the round-robin queue.

---

## RE_UC003: Review and Approve Real Estate Application
* **Use Case Name:** Review and Approve Real Estate Application
* **Use Case Number:** RE_UC003
* **Use Description:** The multi-stage internal review process for approving or rejecting a customer's Real Estate application.
* **Primary Actor:** Real Estate Manager, CEO
* **Pre-condition(s):** Application has been submitted and is in the active review queue.
* **Success End Condition:** Application is approved and a notification is sent to the customer.
* **Main Success Scenario:**
  1. **Real Estate Manager** reviews details $\rightarrow$ Selects **Recommend for Approval** $\rightarrow$ Clicks **Submit**.
  2. Status changes to `AwaitingCEOSignoff`.
  3. **CEO** reviews and selects **Approve** $\rightarrow$ Clicks **Submit**.
  4. Status changes to `RealEstateApplicationApproved`, and a notification is dispatched to the Customer.
