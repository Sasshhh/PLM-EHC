---
title: "Ekurhuleni Housing Company (EHC) Property Lease Management: Standard Operating Procedures"
subtitle: "Step-by-step procedures per role, from application to deposit refund"
date: "10 November 2026"
---

# About this document

These standard operating procedures (SOPs) tell each EHC user exactly what to do in the PLM system, in the order the work happens. Every procedure was taken from the **factory-tested flow**: the EHC factory test run of 24 to 28 September 2026 (*EHC_Factory_Test_Results_V0.5.xlsx*, *EHC_Factory_Evidence.html*, 322 screenshots), cross-checked against the side navigation in the application code on the shared build `e27d7f4` packaged for deployment on 3 November 2026.

Menu names, button captions and pop-up messages are quoted as they appear on screen, including their spelling (for example *Unit Maintanance*). A path such as **Financials › Application Fee** means: click **Financials** in the side navigation, then click **Application Fee** in the list that opens.

Use case numbers follow the CoE EHC-PLM Solution Design Specification V0.14 (UC01 to UC032), which is also the order of the factory workbook. Where the system differs from the SDS, this document describes what the system does.

## Document set

| Document | Use it for |
|---|---|
| **EHC SOPs** (this document) | Step-by-step procedures per role |
| **EHC Quick Reference** | One page per role: menus, queues, what to do next |
| **EHC Go-Live Administration SOP** | Setting up users, queues and routing; running the scheduled checks |

## Roles

The SDS actor names map to system roles as follows. A user only sees the menus for the roles they hold.

| SDS actor | System role (Users Maintenance) | Main work |
|---|---|---|
| Applicant / Tenant | Customers (EHC department) | Apply, pay, accept offer, train, sign lease, serve notice, complaints, disputes |
| Client Services Officer (CSO) | Client Services Officer | Risk assessment, training invites, lease capture, renewals, terminations, evictions, refunds, complaints |
| Client Services Supervisor | Housing Supervisor | Unit inspection slots, pre-occupation and exit inspections |
| Bookkeeper / Financial Officer | Revenue Officer (there is no Bookkeeper role) | Validate application fee and deposit, appraise terminations |
| Revenue Manager | Revenue Manager | Support risk assessments, sign leases, review renewals and refunds |
| CEO | Property Manager (and **CEO** for refund authorisation) | Final approvals, lease and renewal signatures, eviction authorisation, refund authorisation |
| Maintenance Manager | Maintenance Manager | Job cards for units inspected Not Habitable |
| Property & Facilities Manager | Property & Facilities Manager | Approve maintenance job cards |
| Area Manager | Area Manager | Waiting list queue, work allocation, complex assignment |
| System Administrator | Back Office System Administrator | Users, round robin, templates, scheduled checks (Tools) |

## How work reaches you

Every step is a work item in a queue. When one person completes a step, the system assigns the next step to a user who holds the next role, and sends that user an e-mail/SMS. Each queue screen only lists the items assigned to you. If an item you expect is not in your queue, it was assigned to someone else, or to nobody (see the *Go-Live Administration SOP*, section on routing).

## Common screen behaviour

* **Confirm pop-ups.** Almost every Submit asks "Are you sure…?" with **No!** and **Yes, I am sure!**. Nothing is saved until you click **Yes, I am sure!**.
* **Official Number.** Approval screens ask for your official (staff) number before they unlock the action and the signature pad. Some screens need you to click **Authorize** first.
* **Signatures.** Draw your signature in the signature box with the mouse or a stylus. Scroll the box fully into view before signing; a signature drawn on a half-visible box can be lost. Screens refuse an empty signature ("Please sign before submitting.").
* **Reasons.** A comment or reason is mandatory when you reject. Always type a reason when you approve as well; the system does not force it, but the SDS requires it (BR05).
* **Uploads.** After an upload, a grey "Uploading document/s…Please Wait" overlay can stay on screen even though the upload worked. If it does not clear within a few seconds, re-open the page from the side navigation; the uploaded file will be there (defect D027).
* **First sign-in.** A user with a temporary password lands on *Manage Account* and must change the password before using the system.

# The end-to-end EHC flow

| # | Step | Who | Status after the step |
|---|---|---|---|
| 1 | Register profile (UC01) | Applicant | Profile pending approval |
| 2 | Submit application (UC03) | Applicant | In Awaiting Application Fee Upload |
| 3 | Upload application fee proof (UC04) | Applicant | In Awaiting Application Fee Validation |
| 4 | Validate application fee (UC05) | Revenue Officer | Awaiting Risk Assessment |
| 5 | Risk assessment (UC06) | CSO › Revenue Manager › CEO | Added to Waiting List Queue |
| 6 | Accept unit offer (UC07) | Applicant | Awaiting Deposit Payment |
| 7 | Upload deposit proof (UC08) | Applicant | Awaiting deposit validation |
| 8 | Validate deposit (UC010) | Revenue Officer | Payment Validated and Approved |
| 9 | Invite to training, training and exam (UC011) | CSO › Applicant | Awaiting Online Training › training complete |
| 10 | Schedule pre-occupation inspection (UC012) | Housing Supervisor › Applicant | Awaiting Inspection |
| 11 | Conduct inspection (UC013) | Housing Supervisor | Awaiting Lease Agreement (or job card if Not Habitable) |
| 11a | Job card and sign-off (UC014, UC015) | Maintenance Manager › P&F Manager | Unit available |
| 12 | Capture lease details (UC016) | CSO | In Awaiting Lease Agreement |
| 13 | Generate and sign lease (UC017) | CSO › Tenant › Revenue Manager › CEO | **Active Lease** |
| 14 | During the lease (UC019 A-E, UC028) | Tenant, CSO | Complaints, transgressions, service requests, disputes |
| 15 | Renewal (UC020-UC024) | CSO › RM › CEO › Tenant › CSO › Tenant › RM › CEO | Lease end date extended |
| 16 | Termination by notice (UC025) or eviction (UC026, UC027) | Tenant/CSO › Revenue Officer, or CSO › CEO › CSO | In Awaiting Exit Inspection |
| 17 | Exit inspection (UC029) | CSO › Tenant › Housing Supervisor | Awaiting Vacating Confirmation |
| 18 | Confirm vacated (UC030) | CSO | Applicant Vacated The Unit |
| 19 | Deposit refund (UC031, UC032) | CSO › Revenue Manager › CEO | **Lease Terminated** |

# Applicant / Tenant procedures

Role: Customers. Your side navigation shows **PLM Applications**, **My Training**, **My Complaints**, **Notice**, **Renewals**, **Tenants**, **Complaints**, **Service Requests**, **Disputes** and **User Manual**. After sign-in you land on your Inbox.

## SOP-A1 Register a profile (UC01)

1. Open the PLM site. On the sign-in page click **Register**.
2. Choose department **Ekurhuleni Housing Company**. Capture name, surname, SA ID number, preferred method of communication (Email / Sms / Both), username, password and confirmation, mobile number and confirmation, e-mail address and confirmation.
3. Tick *I am not a robot* and submit. An invalid ID number is refused with "Please enter a valid 13 digit ID no."
4. Accept the terms and conditions. Open the activation link in the e-mail you receive.
5. Click **Here** to complete your profile, choose the customer type, save, upload your ID and supporting documents, save, and click **Next**.
6. Your profile now waits for approval by the back office (Area Manager or Back Office System Administrator, under **Registration › Awaiting Registrations**). You are notified by e-mail when it is activated, or asked to re-upload documents.

*Factory status: not tested end to end (stopped at the reCAPTCHA).*

## SOP-A2 Submit an application for a lease (UC03)

1. Click **PLM Applications › Capture**.
2. On **Applicant Information**, choose Applicant Type **Individual (Residential)** and complete all required fields (25), including your preferred complex (Option 1 / Option 2) and house type.
3. Read the declaration and switch on **I accept the declaration above**. Without it, Next is blocked with "Declaration required".
4. Click **Next**. On **Document Upload**, click **Browse** next to each document and upload it. The checklist has 8 document types: Identity Document, Payslip/Proof of Pension Grant, Bank Statement (three months), Confirmation of Employment, Affidavit, Proof of Residential Address, Birth Certificate and Marriage Certificate (where applicable).
5. Click **Submit**, then **OK**. The system gives a reference number in the form **EHC{yyyyMMdd}{sequence}** and shows the banking details for the application fee.

Status after: *In Awaiting Application Fee Upload*.

## SOP-A3 Upload proof of payment of the application fee (UC04)

1. Pay the application fee at the bank shown.
2. Click **PLM Applications › Inbox**. Find your application and click **Upload Proof Of Payment**.
3. Click **Browse**, choose the proof of payment, click **Upload**.

Status after: *In Awaiting Application Fee Validation*. The Revenue Officer now validates it.

## SOP-A4 Accept a unit offer (UC07)

1. When you are notified that a unit was matched, click **PLM Applications › Inbox**. Opening the Inbox also runs the unit matching.
2. Your application shows status *Available Unit Matched*. Click **View**.
3. Read the unit details and the deposit amount. Click **Accept** (or **Reject**).
4. The system shows *Unit Offer Accepted Successfully!* with the deposit amount and banking details.

Status after: *Awaiting Deposit Payment*.

## SOP-A5 Upload proof of payment of the deposit (UC08)

1. Pay the deposit at the bank shown.
2. Click **PLM Applications › Inbox**, then **Upload Proof Of Payment** on the application.
3. Browse to the proof of payment and click **Upload**.

The Revenue Officer now validates the deposit.

## SOP-A6 Confirm you still need a unit (UC09)

While you are on the waiting list, the system asks you every five months to confirm you still need a unit.

1. Click **PLM Applications › Inbox**.
2. Click **Unit Still Required?** and choose **YES** to keep your place (or **No** to withdraw).

*Factory status: only partly proven. The five-month check was run from the administrator Tools screen.*

## SOP-A7 Complete the pre-tenancy training and examination (UC011)

1. After the CSO invites you, click **My Training › Pre-Tenancy Training**.
2. Click **Start Training** (or **Continue Training**) and work through the slides with **Next**. Click **Complete Training** at the end.
3. Open the examination and answer all 15 questions. Click **Submit Examination**.
4. You must score **100%** to pass (BR12). If you do not, the system shows the correct answers and you take the examination again.

**My Training › My Scheduled Training** shows the date and venue of any in-person session.

## SOP-A8 Choose an inspection time slot (UC012 and UC029)

1. When notified, click **PLM Applications › Inbox**.
2. Click **Unit Inspection Schedule** on the application.
3. Pick one of the offered time slots and confirm with **Yes, I am sure!**. The system shows "Timeslot has been scheduled successfully."

The same steps apply to the exit inspection at the end of the lease.

## SOP-A9 Sign the lease agreement (UC017)

1. When notified, click **PLM Applications › Inbox** and click **Agreement Validation**.
2. Click **View** to read the lease agreement.
3. Click **Draw Tenant Signature**, sign in the box and confirm.
4. Enter the witness's name, click **Draw Witness Signature**, and have the witness sign.
5. Choose **Approve** and click **Submit**, then **Yes, I am sure!**.

The lease then goes to the Revenue Manager and the CEO to sign. When the CEO signs, your lease is **Active**.

## SOP-A10 Lodge a complaint (UC019 B)

1. Click **Complaints › Lodge New Complaint** (or **My Complaints › Submit Complaint**).
2. Capture your own details, the complex, block and unit of the person you are complaining about, and choose a **Category** and **Type** (for example *Nuisance and Behavioural › Noise Disturbance*).
3. Type a detailed description, with dates and times. Upload evidence if you have it.
4. Click **Submit Complaint**.

**My Complaints › View Complaints** lists your complaints and their status.

## SOP-A11 Log a service request (UC19D)

1. Click **Service Requests › Create Service Request**.
2. Capture who is reporting, contact details, location (complex, block, unit, room), the category and the priority, and a detailed description. Upload a photo or document if useful.
3. Click **Submit Request**. A reference in the form EHC_SR_###_YYYY is created.

To change a request, open it from **Service Requests › Service Requests Dashboard** and edit it (UC19E, not factory tested).

## SOP-A12 Register a lease dispute (UC028 A)

1. Click **Disputes › Register Lease Dispute**.
2. Capture your name and choose the **Category** (Financial & Billing, Compliance, Maintenance, Tenure, Notices, Administrative). Describe the dispute in detail.
3. Click **Register Dispute** and confirm with **Yes, register!**. A reference in the form **EHC_DISP_###_YYYY** is created, with status *Open, Awaiting Review*.

## SOP-A13 Accept a lease renewal offer (UC022, UC023)

1. When notified, click **Renewals › Lease Renewals**. Your lease shows *Renewal Available*.
2. Click **Renewal Available**, choose **Approve** to accept (or **Reject** to decline), click **Submit**, then **Yes, I am sure!**.
3. The lease moves to *In Awaiting Renewal Document(s) Upload*. Click **Upload Document(s)** and upload the requested documents.

The CSO then prepares the renewal agreement for you and your witness to sign.

## SOP-A14 Give notice to end your lease (UC025)

1. Click **Notice › Serve Notice**. Your active lease is listed. Click **Serve Notice**.
2. Enter the **termination date** and the **reason**. The date must be at least one calendar month ahead and the **last day of a month** (BR29, BR30).
3. Upload your proof of banking details (for the deposit refund) with **Browse** and **Upload**, then **OK**.
4. Click **Submit**, then **Yes, I am sure!**. A termination reference is generated and the CSO is notified.

## SOP-A15 Manage the people living in your unit (UC019 A)

Click **Tenants › Manage Tenants** to see the occupants against your lease, and **Tenants › My Leases** to see your lease. *Factory status: the landing screen shows no capture action, so adding an occupant could not be proven.*

# Client Services Officer procedures

Role: Client Services Officer. Your side navigation shows **PLM Applications** (Capture, All Applications, All Leases), **Agreements**, **Assessment**, **Evictions**, **Refund**, **Leases**, **Properties**, **Renewals**, **Tenants**, **Termination** (Termination Queued, Terminate; and Terminated Leases, Terminations), **Training**, **Inspections**, **Complaints**, **Payment Transgressions**, **Service Requests**, **Disputes**, **Manual PLM Applications** and **Communication**.

## SOP-C1 Capture the risk assessment (UC06, step 1 of 3)

1. Click **Assessment › Risk Assessment**. The list shows applications awaiting risk assessment.
2. Find the application and click **Assess**.
3. Click the **Perform TPN/ITC Check** link (https://mrisoftware.tpn.co.za), run the credit check and download the result.
4. Upload the credit check and any supporting documents. If the "Please Wait" overlay stays on screen, re-open the page (D027).
5. Enter your **Official Number**, choose the outcome **Approve** (or **Reject**), and type the **Reason**. Always enter a reason, even on approval.
6. Capture the matching unit details (house type and complex).
7. Click **Submit**, then **Yes, I am sure!**. The system shows "Risk assessment approved for application reference…".

The application goes to the Revenue Manager.

## SOP-C2 Invite the applicant to training (UC011)

1. Click **Training › Invite To Training**. The list shows applicants whose deposit has been validated.
2. Click **Invite To Client Training** on the application.
3. Capture the meeting date, venue and comments. Click **Submit**, then **Yes, I am sure!**.
4. The system shows "Tenant Invited Successfully" and sends the invitation by e-mail and SMS.

**Training › Schedule Training** sets up a training date, time and venue for in-person sessions.

## SOP-C3 Capture lease details (UC016)

1. Click **Leases › Capture Lease Details**. The list shows applications awaiting lease details.
2. Click **Update Details** on the application.
3. Page 1 shows the tenant information. Check it and click **Next**.
4. Page 2: capture the leasing period, lease financials, billing information, DSTV services, additional security deposits, complex, unit number, block number and cell number.
5. Enter the **lease start date**. It must be in a future month. The system calculates the expiry date (24 months), the renewal notice date (3 months before expiry) and the termination notice date (1 month before expiry).
6. **Enter a value in every money field, using 0 where nothing is charged**: preparation fee, credit check fee, electricity, security fee, shade port parking, open parking, store rooms, lease administration fee, initial deposit, deposit tenant contribution and "calculated as follows". If any is left empty, generating the lease agreement fails with a server error (D031).
7. Click **Submit**. (The success pop-up is titled "Tenant Invited Successfully"; that is a wrong title, D033.)

## SOP-C4 Generate the lease agreement (UC017, step 1 of 4)

1. Click **Agreements › Lease Agreement**. Click **Generate Lease Agreement** on the application.
2. Click **View** to check the generated agreement.
3. Choose **Approve** and click **Submit**, then **Yes, I am sure!**. The system shows "Lease agreement generated successfully".

The tenant is notified to sign.

## SOP-C5 Recommend a lease for renewal (UC020)

Leases due for renewal are listed when they are three months from expiry. See the *Go-Live Administration SOP*: in this build the renewal check must be started by the administrator (D044).

1. Click **Renewals › Lease Review**. Due leases show *In Awaiting Application Lease Renewal Review*.
2. Click **Recommend**. Review the tenant's history.
3. Choose the outcome **Renew for 12 months**, **Renew for 24 months** or **Not Renew**. Enter the recommendation reason and upload supporting documents.
4. Click **Submit Recommendation**, then **Yes, Submit**. The Revenue Manager is notified.

## SOP-C6 Prepare the renewal agreement (UC023)

After the tenant accepts and uploads the renewal documents:

1. Click **Renewals › Lease Review**. The lease shows *Awaiting Lease Renewal Agreement Conclusion*. Click **Capture Renewal Details**.
2. Check and update the renewal details (term, monthly rental, deposit, changes to the original lease) and the renewal start date.
3. Click **Generate Lease Renewal Agreement**. The lease moves to *Awaiting Renewal Agreement Generation*.
4. Click **Generate Agreement**. Check the agreement, choose **Approve**, click **Submit**, then **Yes, I am sure!**. The agreement is sent to the tenant.

## SOP-C7 Capture the tenant's renewal signature (UC024, step 1 of 3)

The renewal agreement shows *Awaiting Renewal Tenant Signature* with the action **Tenant Validation** in **Renewals › Lease Review**. In the factory run this was completed from the CSO's queue with the tenant present.

1. Click **Tenant Validation**. View the agreement.
2. Have the tenant sign in the tenant signature box and the witness sign in the witness signature box.
3. Choose **Approve**, click **Submit**, then **Yes, I am sure!**.

The agreement goes to the Revenue Manager and then the CEO.

## SOP-C8 Review a tenant's notice to terminate (UC025)

1. Click **Termination › Terminations**. The notice shows *Awaiting CSO Termination Review*. Click **Review Notice**.
2. Check the notice date, reason and documents.
3. Choose **Approve** to support it (or **Reject**, with a reason; the tenant is told why).
4. Click **Submit Review**, then **Yes, I am sure!**. A supported notice goes to the Revenue Officer for appraisal.

## SOP-C9 Capture an eviction (UC026)

1. Click **Evictions › Capture Eviction**. Enter the lease reference number. An unknown reference is refused.
2. Choose the reason: **Illegal Activities**, **Non-Compliance with the Lease Terms**, **Non-Payment** or **Subletting**.
3. Enter your official number, the eviction date and the eviction outcome summary. Upload supporting documents.
4. Click **Submit**, then **Yes, I am sure!**. The case goes to the CEO for authorisation.

## SOP-C10 Serve the eviction notice and capture proof of service (UC027)

After the CEO authorises the eviction:

1. Click **Termination › Terminations**. The case shows *Awaiting Eviction Service*. Use **Eviction Notice PDF** to print the notice.
2. Click **Serve Eviction Notice**. Choose the method (**Sheriff of the Court**, **Hand Delivery**, **Registered Post**, **Email Service**), the service date and the reference (for example the sheriff's reference). Click **Serve Notice**.
3. The case shows *Eviction Notice Served*. When proof comes back, click **Capture Proof of Service**.
4. Choose the proof type (Sheriff return of service, Signed acknowledgement, Registered mail tracking, Email read receipt, Failed attempt affidavit), the date, details, and upload the proof.
5. Click **Capture Proof**, then **Yes, capture proof!**. The case moves to exit inspection.

## SOP-C11 Schedule the exit inspection (UC029, step 1 of 3)

1. Click **Termination › Terminations**. The lease shows *In Awaiting Exit Inspection*. Click **Schedule Exit Inspection**.
2. Choose the date and tick the available hours. Click **Add Timeslots**, then **Submit**.
3. The tenant picks a slot (SOP-A8). The Housing Supervisor then conducts the inspection.

**Cancellation PDF** on the same row prints the lease cancellation letter.

## SOP-C12 Confirm the unit is vacated (UC030)

1. Click **Termination › Terminated Leases**. The lease shows *In Awaiting Vacating Confirmation*. Click **Confirm Move-Out**.
2. Record the access card, keys and anything else returned.
3. Choose **Vacated**. Click **Submit**, then **Yes, I am sure!**.

**Do not choose Not Vacated.** In this build a lease marked Not Vacated leaves every queue and can only be recovered by the administrator (D051). If the tenant has not moved out, leave the lease in the queue and escalate.

## SOP-C13 Start the deposit refund (UC031)

1. Click **Refund › Start Refund Process**. The lease shows *Applicant Vacated The Unit*. Click **Start Refund Process**.
2. Enter your official number. Choose **Refund Full Deposit**, **Refund Partial Deposit** or **No Refund**.
3. Capture the maintenance cost and approved deductions. The system calculates the nett refund (deposit less maintenance cost and deductions).
4. Justify the recommendation and upload the proof of banking details.
5. Click **Submit Refund Recommendation**, then **Yes, I am sure!**. The Revenue Manager is notified.

## SOP-C14 Capture a payment transgression (UC19C)

1. Click **Payment Transgressions › New Transgression**.
2. Capture the tenancy reference, account number, tenant name and contact details, complex, block, unit, last payment amount and date, and total amount due.
3. Choose the category (**Financial** or **Other**), the type (Payment Misconduct, Levy Arrears, In Violation of Lease Agreement) and the severity (Level 1 Minor, Level 2 Moderate, Level 3 Major). Type a detailed description and upload documents.
4. Click **Submit & Generate Letter**.
5. Choose the letter type (**Payment Transgression Notice**, **Written Warning Letter**, **Final Written Warning Letter**) and click **Generate & Send Letter**. The letter is e-mailed to the tenant.

**Payment Transgressions › All Transgressions** lists all cases.

## SOP-C15 Handle complaints, service requests and disputes (UC019 B, UC19D, UC028)

* **Complaints:** **Complaints › All Complaints** (or **Complaints Dashboard**) lists complaints. Open one to schedule the investigation and capture the outcome. *The investigation steps were not covered by the factory run.*
* **Service requests:** **Service Requests › Service Requests Dashboard** lists requests. Click **NEW REQUEST** to log one for a tenant (same fields as SOP-A11).
* **Disputes:** **Disputes › Disputes Dashboard** lists disputes with the next action for each:
    1. **Review** (*Open, Awaiting Review*): choose the risk (Low, Medium, High, Other), click **Submit Review**, then **Yes, submit!**. Status becomes *Referred*.
    2. **Resolve** (*Referred*): choose **Resolved** or **Not Resolved**, add the summary, click **Submit**, then **Yes, submit!**.
    3. **Close** (*Resolved*): choose **Close Dispute**, click **Submit Decision**, then **Yes, submit!**.

  The SDS gives the review to the Revenue Manager and the closure to the CEO, but only CSO, tenant and administrator roles have the Disputes menu, and the system does not check who acts (D052). Until that is fixed, the CSO must only take the review and closure steps on the documented instruction of the Revenue Manager and the CEO.

## SOP-C16 Send a communication to tenants

Click **Communication › New Communication** to send a message to tenants. (Also available to the Housing Supervisor, Revenue Officer, Revenue Manager, Letting Officer and Property Manager roles.)

# Housing Supervisor procedures

Role: Housing Supervisor (the SDS *Client Services Supervisor*). Your side navigation shows **Inspections › Inspection**, **Properties › Unit Maintanance** and **Termination** (Terminated Leases, Terminations).

## SOP-H1 Publish pre-occupation inspection slots (UC012)

1. Click **Inspections › Inspection**. Applications waiting for an inspection are listed.
2. Click **Inspection Schedule Slots** on the application.
3. Pick the date, tick the available hours and click **Add Timeslots**. Repeat for more days.
4. Click **Submit**. The applicant is notified to pick a slot (SOP-A8).

## SOP-H2 Conduct the pre-occupation inspection (UC013)

1. Click **Inspections › Inspection**. Click **Conduct Unit Inspection** on the application.
2. Click **Download Pre-Inspection Form Template** and complete the form at the unit.
3. Upload the completed form and any photos with **Upload**.
4. Choose the outcome: **Habitable - No defects**, **Habitable - Minor defects** or **Not Habitable** (BR15).
5. Click **Submit**, then **Yes, I am sure!**.

*Habitable* moves the application to *Awaiting Lease Agreement* for the CSO. *Not Habitable* sends the unit to the Maintenance Manager for a job card.

## SOP-H3 Conduct the exit inspection (UC029)

1. Click **Inspections › Inspection**. The lease shows *In Awaiting Exit Inspection*. Click **Conduct Exit Inspection**.
2. Download and complete the inspection form and the exit interview form at the unit.
3. Upload the completed forms and photos.
4. Choose the outcome (**Habitable - No defects**, **Habitable - Minor defects**, **Not Habitable**). Click **Submit**, then **Yes, I am sure!**.

The lease moves to the CSO to confirm the tenant has vacated.

# Revenue Officer procedures

Role: Revenue Officer (the SDS *Bookkeeper* / *Financial Officer*). Your side navigation shows **Financials** (Application Fee, Deposit Payments, Debit Order), **Inspections › Risk Assessment**, **Termination › Terminations** and **Complaints › All Complaints**.

## SOP-R1 Validate the application fee (UC05)

1. Click **Financials › Application Fee**. Applications *In Awaiting Application Fee Validation* are listed.
2. Click **View** on the application. The attached documents are shown.
3. Open the proof of payment and check it against the bank statement.
4. Set the document status to **Approved** (the screen reloads), then choose the outcome **Approve** (or **Reject**, with a reason).
5. Click **Submit**, then **Yes, I am sure!**. The pop-up says "Application Fee payment accepted" (its title reads "Deposit Payment Verification"; that is a wrong title, D025).

The application moves to *Awaiting Risk Assessment* for the CSO.

## SOP-R2 Validate the deposit (UC010)

1. Click **Financials › Deposit Payments**. Click **View** on the application.
2. Check the proof of payment, set the document status to **Approved**, choose **Approve**.
3. Click **Submit**, then **Yes, I am sure!**. The pop-up says "Deposit payment accepted".

Status after: *Payment Validated and Approved*. The CSO is asked to invite the applicant to training.

## SOP-R3 Appraise and authorise a termination (UC025)

1. Click **Termination › Terminations**. The notice shows *Awaiting Termination Appraisal*. Click **Authorize Termination**.
2. Check the notice and the tenant's account and banking details.
3. Enter your **Official Number** and choose **Approve**, **Referral to Legal** or **Reject** (with a reason).
4. Sign in the signature box. Click **Submit**, then **Yes, I am sure!**.

Status after: *Awaiting Exit Inspection*. The tenant and the CSO are notified.

## SOP-R4 Validate a debit order (UC018)

**Not in use in this build.** The **Financials › Debit Order** screen exists, but no lease is ever routed to it: when the CEO signs the lease it becomes active straight away (D040, decision DEC-08). Leave this queue alone until the business decides whether debit order validation belongs in the flow.

# Revenue Manager procedures

Role: Revenue Manager. Your side navigation shows **PLM Applications › Inbox Applications**, **Agreements › Lease Agreement**, **Renewals › Lease Review** and **Termination › Terminations**.

## SOP-M1 Support the risk assessment (UC06, step 2 of 3)

1. Click **PLM Applications › Inbox Applications**. Click **Process** on the application.
2. Read the CSO's recommendation and the uploaded documents.
3. Enter your **Official Number**, choose **Supported** (or **Not Supported**) and enter comments.
4. Click **Submit**, then **Yes, I am sure!**. The system shows "Supported. Forwarded to CEO."

## SOP-M2 Sign the lease agreement (UC017, step 3 of 4)

1. Click **Agreements › Lease Agreement**. The application shows the action **Signature**.
2. Click **Signature**, then **View** to read the agreement and the tenant's signature.
3. Click **Draw Signature** and sign.
4. Choose **Approve**. To send it back, choose **Not Supported - CSO** or **Not Supported - Tenant** and type the reason (mandatory).
5. Click **Submit**, then **Yes, I am sure!**. The agreement goes to the CEO.

## SOP-M3 Review a renewal recommendation (UC021)

1. Click **Renewals › Lease Review**. The lease shows *Awaiting Renewal Review Outcome*. Click **Revenue Review**.
2. Review the CSO's recommendation. Choose **Approve** or **Reject** and enter the comment.
3. Click **Submit Decision**, then **Yes, Submit**. Approved renewals go to the CEO.

## SOP-M4 Sign the renewal agreement (UC024, step 2 of 3)

1. Click **Renewals › Lease Review**. The lease shows *Awaiting Renewal RM Signature*. Click **RM Validation**.
2. View the agreement. Choose **Approved** (or **Rejected**), sign, and click **Submit**.

## SOP-M5 Respond to a deposit refund recommendation (UC032, step 1 of 2)

1. Open **Update Refund Response** (the queue at */PropertyLeaseApplication/UpdateRefundResponse*). The lease shows *Awaiting Refund Response*. Click **Update Response**.
2. Check the deductions and the nett refund. Enter your **RM Official Number**, choose **Supported** or **Not Supported**, and upload supporting documents.
3. Click **Submit Review**, then **Yes, Submit!**. The CEO is asked to authorise the refund.

The **Refund** menu is currently only in the CSO navigation, so the Revenue Manager has no menu item for this queue (in the factory run it was completed from the CSO's login). Until the menu is added, open the queue from a bookmark, or the administrator gives the Revenue Manager the link.

## SOP-M6 Review and resolve disputes (UC028 B, C)

The SDS gives you the review and resolution of lease disputes. The Disputes menu is not in your navigation in this build; see SOP-C15.

# CEO procedures

Role: Property Manager for approvals and signatures, plus the **CEO** role for refund authorisation. Your side navigation shows **PLM Applications** (Inbox Applications, Waiting List), **Agreements › Lease Agreement**, **Renewals › Lease Review**, **Maintenance Oversight › Maintenance Overview**, **Termination › Terminations** and, with the CEO role, **Termination › Refund Authorisations**.

## SOP-E1 Finalise the risk assessment (UC06, step 3 of 3)

1. Click **PLM Applications › Inbox Applications**. Click **Finalize** on the application.
2. Read the CSO and Revenue Manager decisions.
3. Enter your **Official Number** and sign. Choose **Approved** (or reject, with a reason).
4. Click **Submit**, then **Yes, I am sure!**. The system shows "Application Approved & Added to Waiting List Queue."

**PLM Applications › Waiting List** shows approved applicants waiting for a unit.

## SOP-E2 Sign the lease agreement (UC017, step 4 of 4)

1. Click **Agreements › Lease Agreement**. Click **Signature** on the application.
2. View the agreement. Click **Draw Signature** and sign.
3. Choose **Approve** (or **Not Supported - CSO** / **Not Supported - Tenant** with a reason).
4. Click **Submit**, then **Yes, I am sure!**. The system shows "Lease agreement approved and lease activated". The lease is now **Active**.

## SOP-E3 Decide on a renewal (UC022)

1. Click **Renewals › Lease Review**. The lease shows *Awaiting Renewal Outcome*. Click **Director Decision**.
2. Choose **Approve** or **Reject** and enter the comment.
3. Click **Submit Decision**, then **Yes, Submit**. On approval the tenant is asked to accept or decline.

## SOP-E4 Sign and conclude the renewal agreement (UC024, step 3 of 3)

1. Click **Renewals › Lease Review**. The lease shows *Awaiting Renewal CEO Signature*. Click **CEO Validation**.
2. View the agreement. Choose **Approved**, sign, and click **Submit & Conclude Renewal**.
3. The renewal is concluded and the lease end date is extended.

## SOP-E5 Authorise an eviction (UC027)

1. Click **Termination › Terminations**. The case shows *Awaiting Eviction CEO Authorization*. Click **CEO Authorization**.
2. Read the eviction details. Enter your **Official Number**, choose **Approve** (or **Reject**), and sign.
3. Click **Submit Authorization**, then **Yes, I am sure!**. The case goes to the CSO to serve the notice.

## SOP-E6 Authorise a deposit refund (UC032, step 2 of 2)

Needs the **CEO** role, and the user must be the refund queue owner (the `s_ceo` setting; see the *Go-Live Administration SOP*).

1. Click **Termination › Refund Authorisations**. The lease shows *Awaiting Refund Authorisation* with the refund amount. Click **Authorize Refund**.
2. Check the deductions and the nett refund. Enter your **CEO Official Number**.
3. Choose **Approve Full Refund**, **Approve Partial Refund**, **No Refund**, **Reject** or **Return to FO**. Upload supporting documents and sign.
4. Click **Submit Authorization**, then **Yes, Submit!**. On approval the lease is **Terminated**.

# Maintenance procedures

## SOP-T1 Manage the job card (UC014), Maintenance Manager

A job card is raised when a unit is inspected **Not Habitable**.

1. Click **Maintenance › Maintenance Job Sheets**. The unit shows *In Awaiting Maintenance Job Sheet*. Click **Maintenance Job Sheet**.
2. For each task capture the start date, end date, activity, material used, quantity, labour, total cost and comments, upload a supporting document, and click **Add Task**. Repeat for every task.
3. Upload the before and after images with **Upload Before Image** and **Upload After Image**.
4. Click **Submit Job Card for Sign-Off**, then **Yes, I am sure!**. The signature section opens.
5. Enter your **Official Number** and click **Authorize** ("You may now capture your approval and signature").
6. Choose **Approve**, enter the reason, sign, and click **Submit Signature**, then **Yes, I am Sure!**.

## SOP-T2 Approve the maintenance outcome (UC015), Property & Facilities Manager

1. Click **Facilities Management › Maintenance Reviews**. Job cards awaiting approval are listed.
2. Open the job card and review the tasks, costs and images.
3. Enter your **Official Number**, click **Authorize**, choose **Approve** (or **Reject**), enter the reason and sign.
4. Click **Submit**, then **Yes, I am Sure!**. On approval the unit becomes available and the CSO and Maintenance Manager are notified.

The CEO can follow all maintenance under **Maintenance Oversight › Maintenance Overview**.

# Area Manager procedures

Role: Area Manager. Your side navigation shows **Waiting List Queue** (Add To Waiting Queue, Manage Queue), **Work Queues** (Manual Work Allocation, Bulk Work Allocation, Complex Area Assigning), **Registration** (Awaiting Registrations, Register User), **Reports** (Application Report, New Lease Report) and **Manage Users**.

## SOP-W1 Manage the waiting list

1. **Waiting List Queue › Manage Queue** shows applicants on the waiting list and their positions.
2. **Waiting List Queue › Add To Waiting Queue** places an approved application on the queue.

An applicant needs a waiting list position before the five-month "still required" check (UC09) can reach them.

## SOP-W2 Reallocate work

* **Work Queues › Manual Work Allocation**: move one work item to another user, for example when a user is on leave.
* **Work Queues › Bulk Work Allocation**: move all of one user's items to another user.
* **Work Queues › Complex Area Assigning**: set which officers serve which complex. New applications are routed to the complex's letting officer.

# Known issues to work around

These are open in the build packaged on 3 November 2026. Each is in the factory *DEFECT_REGISTER.md*.

| Ref | What happens | What to do |
|---|---|---|
| D044 | Nothing flags a lease for renewal in normal running | The administrator runs the renewal scanner (Go-Live Administration SOP) until a scheduled job is built |
| D051 | Confirm Move-Out › **Not Vacated** drops the lease out of every queue | Do not use Not Vacated; escalate instead |
| D040 | Debit order validation is never reached | Do not use Financials › Debit Order (decision DEC-08) |
| D031 | Empty lease money fields crash lease generation | Enter 0 in every money field (SOP-C3) |
| D027 | Upload overlay stays on screen | Re-open the page; the file was uploaded |
| D052 | Anyone with the Disputes Dashboard can review, resolve and close a dispute | CSO acts only on RM/CEO instruction (SOP-C15) |
| D017 | One ID can hold several applications (BR01 not enforced) | CSO checks for an existing application or lease before assessing (decision DEC-01) |
| none | Revenue Manager has no menu item for Update Refund Response | Use the queue link (SOP-M5) |
| D025, D033 | Some success pop-ups have the wrong title | Read the message text, not the title |
