---
title: "Human Settlement (HSD) Property Lease Management: Use Cases As Built"
subtitle: "How the PLM system works today for HSD, step by step"
date: "10 November 2026"
---

# About this document

This document describes, step by step, how the Human Settlement Department (HSD) part of the PLM system works **today**. It was written from the application code on the shared (mixed EHC + Real Estate + HSD) build, commit `e27d7f4`, which is the build packaged for deployment on 3 November 2026. Menu names, page titles, field labels, button captions and pop-up messages are quoted exactly as they appear on screen, including their spelling.

The use case numbers and names follow the HSD FRS (Version 3, 27/09/2023) so the two documents can be read side by side. Where the system's flow differs from the FRS, this document describes what the system does, not what the FRS says. The separate *HSD FRS Coverage* document lists the differences.

## Roles

The FRS role names map to system roles as follows.

| FRS role | System role (as assigned in Users Maintenance) |
|---|---|
| Housing Liaison Officer | Housing Liaison Officer |
| Senior Housing Specialist | Senior Housing Specialist |
| Senior Manager Operations | Regional Manager |
| Tenant | Customers (HSD department) |
| System Administrator | Back Office System Administrator / Super Administrators |
| (not in FRS) | Caretaker (view lists and send communication only) |

## How work reaches a user

Every HSD workflow step is a work item in a queue. When a step completes, the system assigns the next step to the user who holds the required role **and** has an active work allocation for the lease's complex/area, choosing the one with the fewest open items. The assigned user receives a "new case loaded" e-mail/SMS. After the hand-off, the queue page shows: *"The application with ref no. {ref} has moved to {queue name} queue assigned to {user}."*

Each queue list only shows items assigned to the logged-in user. Back Office System Administrator and Super Administrators see every item in every queue.

If no user with the role is allocated to the complex/area, the step is not assigned to anyone and the lease does not appear in any user's queue.

## Lease statuses used by HSD

| Stage | Status shown on screen |
|---|---|
| Captured master lease | Active Lease (lease) / Property Lease Application Completed (application) |
| Renewal due | In Awaiting Agreement of Lease Renewal Review |
| Recommended | In Awaiting Agreement Recommendation Review |
| Reviewed | In Awaiting Agreement Recommendation Approval |
| Approved | In Awaiting Tenant Renewal Acceptance |
| Accepted | In Awaiting Agreement of Lease Renewal |
| Renewal declined by tenant | Terminating At Lease Period End |
| Renewal documents uploaded | Agreement Of Lease Renewed Successfully (lease) / In Awaiting Lease Agreement (application) |
| Agreement generated | In Awaiting Lease Agreement Review |
| Agreement reviewed | In Awaiting Lease Agreement Approval |
| Agreement approved | In Awaiting Applicant Signature |
| Agreement signed | In Awaiting Upload Final Agreement of Lease |
| Final agreement uploaded | Signed Lease Agreement Uploaded |
| Agreement rejected at any step | In Awaiting Agreement Of Lease Update |
| Termination captured | Termination Due To Tenant Notice / Termination Due To Lease Not Renewed / Termination Due To Tenant Deceased |
| Termination approved | In Awaiting Exit Inspection |
| Exit inspection done | Awaiting CSO Vacating Confirmation |
| Vacated | Applicant Vacated The Unit |

## The end-to-end HSD flow

1. **Capture Master** (UC-13) loads an existing lease. The lease is active immediately.
2. Three months before the lease end, the lease enters the **renewal** flow: Recommend (UC-24) → Review (UC-25) → Approve (UC-26) → Tenant Accept (UC-27) → Renew Lease / upload documents (UC-28).
3. The renewal then produces a new agreement: **Generate** (UC-14) → **Review** (UC-15) → **Approve** (UC-16) → **Tenant Sign** (UC-17) → **Upload Final Agreement** (UC-18). A rejection at any of these steps sends the lease to **Agreement Update**.
4. At any time while the lease is active: **Manage Tenants** (UC-19), **Transfer Lease** (UC-20), **New Communication** (UC-23).
5. Termination: **Serve Notice** (UC-29) → **Noticed Agreements / Terminate Agreement** (UC-34, UC-30) → **Pending Review** (UC-31) → **Post-Inspection** (UC-32) → **Pending Take-Off** (UC-33).

# UC-03 Login

**Primary actor:** any HSD user. **Trigger:** need to use the system.

**Main flow**

1. User opens the PLM site and goes to the login page. The page shows **Sign in** with the fields *username or e-mail* and *password*, a **Remember login** tick box, the link **Forgot your password?** and the button **Sign In**.
2. User enters their user name (or e-mail) and password and clicks **Sign In**.
3. If the *Active Directory* setting is on, the system checks the password against the City's Active Directory; otherwise against the PLM user store.
4. On success the system opens the landing page for the user's role. Housing Liaison Officer, Senior Housing Specialist and Regional Manager land on the **Human Settlement Dashboard - {user name}**. Back Office System Administrator and Super Administrators land on the admin inbox.

**Alternative flow: wrong credentials**

3a. The system shows an invalid user name or password error on the login page, records the failed attempt, and adds a short delay before the next attempt from the same address.

**Notes on screen behaviour**

- There is no "COE SSO" button. The WSO2/IAM sign-in was switched off in the code because the IAM server was decommissioned.

# Home: Human Settlement Dashboard

**Side nav:** PLM Applications › Dashboard.

The dashboard heading is **Human Settlement Dashboard - {user name}**. It shows four count tiles: **All Applications**, **Master Applications**, **New Applications** and **Signed Agreements**, followed by personalised queue cards. Each card shows *Recieved*, *Completed* and open counts for one queue: Renewal Reccomend..., Renewal Acceptance, Renewal Documents, Generate Agreement, Agreement Signature, Finalized Agreements, Update Agreement, Noticed Agreements, Pending Take-Off, Post Inspection, Agreement Review, Termination Review, Renewal Review.

**Side nav:** PLM Applications › All Applications / Master Applications.

These pages list leases with the columns *Applicant Name, Application Type, Application Reference Number, Application Created Date, Status, Action*. **View** opens the read-only lease details page (Lease Information, Tenant Details, Contact Details, Current Addresss, Unit Details, Leased Addresss, Lease Period, Agreement Renewal Details, Agreement of Lease Terminations, Property Application History).

# UC-13 Capture Lease Details (Capture Master)

**Primary actor:** Housing Liaison Officer (also Back Office System Administrator / Super Administrators).
**Side nav:** PLM Applications › Capture Master.
**Pre-condition:** the user is allocated to at least one HSD complex/area (only those complexes are offered).

**Main flow**

1. User selects **PLM Applications** and then **Capture Master**.
2. The system opens a four-step wizard with an **Instructions** panel and the steps **1 Tenant Information**, **2 Spouse Information**, **3 Contact Person**, **4 Agreement Information**. Buttons along the bottom: **Cancel**, **Previous**, **Next**, **Submit**.
3. **Step 1, Tenant Information.** User captures:
   - *Tenant Details:* Tenant Type (Natural Person only), First Name, Surname, ID Number (13 digits, validated), Date of Birth.
   - *Contact Details:* Mobile Number (10 digits), Email Address, Home Number, Work Number.
   - *Income Infomation:* Income Source, Sassa Number (when the source is a SASSA grant), Gross Income, Net Income, Total Income (calculated, read-only).
   - *Current Address:* Address, Suburb, Postal Code (4 digits).

   User clicks **Next**.
4. The system validates the required fields on the step. Missing fields are highlighted and the wizard stays on the step. Otherwise it moves to **Spouse Information**.
5. **Step 2, Spouse Information.** If there is a spouse, user ticks **Spouse Information?** and captures *Spouse Details* (First Name, Last Name, Gender, Title, Identification Type = South African ID, ID Number, **Nomimate For Take-Over?**), *Contact Details* (Mobile Number, Email Address), *Income Infomation* (Income Source, Sassa Number, Gross Income, Net Income, Total Income) and *Current Address* (Address, Suburb, Postal Code). Without the tick box the spouse fields stay hidden. User clicks **Next**.
6. **Step 3, Contact Person.** User captures *Contact Person Details* (First Name, Surname, Relationship, Title) and *Contact Details* (Mobile Number, Email). User clicks **Next**.
7. **Step 4, Agreement Information.** User captures:
   - *Space Unit Details:* Complex/Area (only the user's allocated HSD complexes), Space/Unit No., Solar Ref., Income Brackets, Category, Typology (enabled after Category is chosen; options depend on the Category).
   - *Leasing Period:* Start Date. The start date must be the first day of a month; otherwise the system shows **"Please select first day of the month!"** and clears the dates. When a valid start date is picked the system fills the read-only fields **End Date** (last day of the 24th month), **Renewal Notice** (first day of the month two months before the end month) and **Termination Notice** (first day of the end month).
   - *Communication:* Communication Method, tick **Email** and/or **SMS**.
   - *Billing Information:* Rental Amount (Incl VAT), Deposit Required, Store Rooms, Shade Port Parking, Open Parking, Open Parking Bay. Rental Amount and Deposit Required are typed in by the user.
8. User clicks **Submit**.
9. If no start date was chosen the system shows **"Please make sure you have selected start date!"** Otherwise it saves the lease and:
   - gives it a reference number in the form `HSD{yyyyMMdd}{daily sequence}`;
   - sets the application status to *Property Lease Application Completed* and the lease status to *Active Lease*;
   - calculates VAT at 15% of the rental amount;
   - opens **Master Applications** and shows **Successfully Captured** – *"Master Lease has been captured successfully, the reference no {ref}, it is currently Property Lease Application Completed"*.

**Alternative flow: Cancel**

3a/5a/6a/7a. User clicks **Cancel** on any step. The system reloads an empty Capture Master wizard at step 1.

**Alternative flow: Previous**

5b/6b/7b. User clicks **Previous**. The system returns to the previous step with the captured values kept.

# UC-24 Recommend for Lease Renewal

**Primary actor:** Housing Liaison Officer.
**Side nav:** Agreement Renewal › Recommendations.
**Pre-condition:** today is on or after the lease's Renewal Notice date and the lease has not already been sent for renewal.

**How leases enter the queue**

Each time any user opens an **Agreement Renewal** list page, the system checks all HSD leases. For each lease whose Renewal Notice date has passed and which is not yet flagged, it:

- sets the lease status to *In Awaiting Agreement of Lease Renewal Review*;
- assigns a **Renewal Recommendation** work item to a Housing Liaison Officer for the complex;
- sends the tenant the "up for renewal" e-mail/SMS.

**Main flow**

1. User selects **Agreement Renewal** and then **Recommendations**.
2. The system lists the leases assigned to the user under the title *Awaiting Agreement Recommendation*, with *Applicant Name, Application Type, Application Reference Number, Agreement Created Date, Status, Action*.
3. User clicks **Review** on a lease.
4. The system opens the **Evaluating Application** screen with the lease row (and a **View** button for full details), **Tenant Account Balance** (*Deposit Amount Paid*, *Amount Due*) and **Complaints Against Tenant** (*Complaint Date, Comment, Complaint By*).
5. User selects **Outcome** (*Renew for 24 months* or *Not Renew*), types a **Comment** and clicks **Submit**.
6. The system shows **"Are you sure you want to submit this recommendation?"** with **No!** and **Yes, I am sure!**.
7. User clicks **Yes, I am sure!**.
8. The system records the recommendation ("Recommended for 24 months: {comment}" or the Not Renew equivalent), sets the lease to *In Awaiting Agreement Recommendation Review* and assigns **Review Lease Renewal Recommendation** to a Senior Housing Specialist for the complex. It returns to the Recommendations list with the hand-off message.

**Alternative flow: mandatory field missing**

5a. User clicks **Submit** without an Outcome or Comment. After **Yes, I am sure!** the browser highlights the missing required field and nothing is submitted.

**Alternative flow: No**

7a. User clicks **No!**. The pop-up closes and the captured values stay on the screen.

# UC-25 Review Lease Renewal Recommendation

**Primary actor:** Senior Housing Specialist.
**Side nav:** Agreement Renewal › Pending Review.

**Main flow**

1. User selects **Agreement Renewal** and then **Pending Review**.
2. The system lists the user's leases under *Awaiting Recommendation Review*.
3. User clicks **Review**.
4. The system opens **Evaluating Application** with the lease row, **Tenant Account Balance**, **Housing Liason Officer's Recommendation** (*Housing Liason Officer, Action Comment, Action Date*) and **Complaints Against Tenant**.
5. User selects **Outcome** (*Seconded* or *Not Seconded*), types a **Comment** and clicks **Submit**.
6. The system shows **"Are you sure you want to submit this recommendation?"**.
7. User clicks **Yes, I am sure!**.
8. The system records the review, sets the lease to *In Awaiting Agreement Recommendation Approval* and assigns **Approve Lease Renewal** to a Regional Manager for the complex. Both outcomes route the same way.

**Alternative flows:** as UC-24 (missing field highlighted; **No!** keeps the screen).

# UC-26 Approve Lease Renewal

**Primary actor:** Regional Manager (FRS: Senior Manager Operations).
**Side nav:** Agreement Renewal › Pending Approval.

**Main flow**

1. User selects **Agreement Renewal** and then **Pending Approval**.
2. The system lists the user's leases under *Awaiting Recommendation Approval*.
3. User clicks **Review**.
4. The system opens **Evaluating Application** with the lease row and **Agreement Renewal Recommendation/s** (*Actioned By, Action Comment, Action Date*) showing the earlier recommendation and review.
5. User selects **Outcome** (*Approve* or *Reject*), types a **Comment** and clicks **Submit**.
6. The system shows **"Are you sure you want to submit this recommendation?"**; if *Reject* is chosen with a comment, it then asks **"Are you sure you want to oppose this recommendation?"**.
7. User clicks **Yes, I am sure!**.
8. The system compares the Regional Manager's outcome with the Housing Liaison Officer's recommendation:
   - **Approve** a *Renew for 24 months* recommendation, or **Reject** a *Not Renew* recommendation: the lease goes to *In Awaiting Tenant Renewal Acceptance*. The system generates a 4-digit One Time Pin, e-mails the tenant *"PLM: Agreement Renewal Acceptance"* with a link to accept or decline, sends the "renewal approved" message containing the OTP, and assigns **Accept Lease Renewal** to a Housing Liaison Officer.
   - **Reject** a *Renew for 24 months* recommendation, or **Approve** a *Not Renew* recommendation: the lease goes back to *In Awaiting Agreement Recommendation Review* (Senior Housing Specialist) and the tenant gets the "renewal rejected" message.

**Alternative flows:** as UC-24.

# UC-27 Accept Lease Renewal

**Primary actor:** Housing Liaison Officer on behalf of the tenant, or the tenant through the e-mail link.
**Side nav:** Agreement Renewal › Pending Acceptance.

**Main flow (Housing Liaison Officer)**

1. User selects **Agreement Renewal** and then **Pending Acceptance**.
2. The system lists the user's leases under *Awaiting Renewal Acceptance*.
3. User clicks **Review**.
4. The system opens **Evaluating Application** with **ACTION** (*Approve* / *Reject*) and **ONE TIME PIN**.
5. User selects *Approve*, enters the tenant's OTP and clicks **Verifiy**.
6. The system shows **"Are you sure you want to submit this action?"**.
7. User clicks **Yes, I am sure!**.
8. The system validates the OTP, sets the lease to *In Awaiting Agreement of Lease Renewal*, sends the tenant the "renewal accepted" message, assigns **Renew Lease** to a Housing Liaison Officer and shows **"Successfully actioned, communication sent."**

**Alternative flow 1: self-service (tenant)**

1. Tenant clicks the link in the *PLM: Agreement Renewal Acceptance* e-mail. No login is needed.
2. If the lease is still awaiting acceptance, the system opens the renewal acceptance screen with **ACTION** and **ONE TIME PIN**; otherwise it shows *"An error has occurred while processing your request. Your application has moved to the next step!"*.
3. Tenant selects the action, enters the OTP and clicks **Verifiy**, then **Yes, I am sure!**.
4. The system processes it as in step 8 and shows the result.

**Alternative flow 2: wrong OTP**

5a. The system shows *"Invalid One Time Pin {pin}"*, or *"The One Time Pin {pin} has been used"* if it was already used. Nothing changes.

**Alternative flow 3: Reject**

5b. User (or tenant) selects *Reject*. The system sets the lease to *Terminating At Lease Period End* and sends the "renewal rejected by tenant" message. No termination work item is created.

# UC-28 Renew Lease

**Primary actor:** Housing Liaison Officer.
**Side nav:** Agreement Renewal › Pending Renewal.

**Main flow**

1. User selects **Agreement Renewal** and then **Pending Renewal**.
2. The system lists the user's leases under *Awaiting Agreement Renewal*.
3. User clicks **Renewal Documents**.
4. The system opens the **Documents Upload** panel for the six renewal documents: Identity Document, Payslip/Proof of Pension Grant, Bank Statemant, Confirmation of Employment, Affidavit Document, Proof of Residential Address (plus an optional Additional Document).
5. For each document the user clicks the file chooser, selects the file and clicks **Upload**.
6. User clicks **Finish**.
7. The system checks that all six documents are uploaded. If any is missing it shows *"Please upload complete documents, {n} of 6 missing!"*.
8. When complete, the system asks **"Are you sure you want to submit these documents?"**; user clicks **Yes, I am sure!**.
9. The system extends the lease End Date, Renewal Notice and Termination Notice by 24 months (12 months for Dormitory units), sets the lease to *Agreement Of Lease Renewed Successfully*, sets the application to *In Awaiting Lease Agreement*, sends the "renewal documents uploaded" message and assigns **Generate Lease Agreement** to a Housing Liaison Officer.

**Alternative flow:** user clicks **Back To List** to return without submitting.

# UC-14 Generate Lease Agreement

**Primary actor:** Housing Liaison Officer.
**Side nav:** Agreements › Agreement Generate.
**Pre-condition:** application status *In Awaiting Lease Agreement* (after UC-28 or after an Agreement Update).

**Main flow**

1. User selects **Agreements** and then **Agreement Generate**.
2. The system lists the user's leases under *Generate Lease Agreement*.
3. User clicks **Generate Lease Agreement**.
4. The system immediately generates the agreement of lease data, sets the application to *In Awaiting Lease Agreement Review*, assigns **Agreement Review** to a Senior Housing Specialist, sends the related notification and opens the **Generated Lease Agreement** screen (lease row under **Evaluating Application** plus *Document Type / Document*).
5. User clicks **View** to download the Agreement of Lease PDF, filled from the HSD agreement template (tenant name, ID, unit, commencement and expiry dates, monthly amount, nominee).
6. User clicks **Finish**.
7. The system returns to the Agreement Generate list and shows *"Agreement of Lease Successfully Generated!"*.

# UC-15 Review Lease Agreement

**Primary actor:** Senior Housing Specialist.
**Side nav:** Agreements › Agreement Review.

**Main flow**

1. User selects **Agreements** and then **Agreement Review**.
2. The system lists the user's leases under *Awaiting Agreement Review*.
3. User clicks **Agreement Review**.
4. The system opens **Evaluating Application** and **Generated Agreement Of Lease** with a **View** button for the PDF.
5. User selects **Approval Action** = *Approve* and clicks **Submit**.
6. The system shows **"Are you sure you want to submit the following agreement?"**.
7. User clicks **Yes, I am sure!**.
8. The system sets the application to *In Awaiting Lease Agreement Approval* and assigns **Approve Lease Agreement** to a Regional Manager.

**Alternative flow: Reject**

5a. User selects *Reject*. The system shows **Rejecting Comment** (required). User types the reason and clicks **Submit**, then **Yes, I am sure!**. The system sets the application to *In Awaiting Agreement Of Lease Update*, logs "Reject Agreement Of Lease" with the reason, and assigns **Update Agreement** to a Housing Liaison Officer (see *Agreement Update* below).

# UC-16 Approve Lease Agreement

**Primary actor:** Regional Manager.
**Side nav:** Agreements › Agreement Approval.

**Main flow**

1. User selects **Agreements** and then **Agreement Approval**.
2. The system lists the user's leases under *Awaiting Agreement Approval*.
3. User clicks **View Agreement**.
4. The system opens **Evaluating Application** and **Generated Agreement Of Lease** with **View** and a **Signature** drop-down.
5. User selects **Signature** = *Approve*. The system inserts the Regional Manager's signature details on the agreement and downloads the signed PDF.
6. User selects **Approval Action** = *Approve* and clicks **Submit**, then **Yes, I am sure!** on **"Are you sure you want to submit the following agreement?"**.
7. The system sets the application to *In Awaiting Applicant Signature*, generates a 4-digit OTP, e-mails the tenant *"PLM: Agreement Signature"* with a link and the OTP, sends the "awaiting applicant's signature" message and assigns **Accept Lease Agreement** to a Housing Liaison Officer.

**Alternative flow: Reject**

6a. User selects *Reject*, types the **Rejecting Comment** and submits. The system clears the lessor signature, sets *In Awaiting Agreement Of Lease Update* and assigns **Update Agreement** to a Housing Liaison Officer.

# UC-17 Accept Lease Agreement

**Primary actor:** Housing Liaison Officer on behalf of the tenant, or the tenant through the e-mail link.
**Side nav:** Agreements › Agreement Signature.

**Main flow (Housing Liaison Officer)**

1. User selects **Agreements** and then **Agreement Signature**.
2. The system lists the user's leases under *Awaiting Aplicants Signature*.
3. User clicks **Signature**.
4. The system opens **Evaluating Application** and **Generated Agreement Of Lease** with **View**, **Signature**, **Approval Action** and **One Time Pin**.
5. User selects **Signature** = *Approve*. The system inserts the tenant's initials and ID number in the signature fields and downloads the signed lease.
6. User selects **Approval Action** = *Approve*, enters the tenant's 4-digit OTP and clicks **Verify**.
7. If the lease was not signed in step 5, the system shows **Error – "Please sign the agreement of lease to submit!"**.
8. The system shows **"Are you sure you want to submit the following agreement?"**; user clicks **Yes, I am sure!**.
9. The system checks the OTP. If valid, it sets the application to *In Awaiting Upload Final Agreement of Lease*, marks the OTP used and assigns **Finalized Agreement** to a Housing Liaison Officer.

**Alternative flow 1: self-service (tenant)**

1. Tenant clicks the link in the *PLM: Agreement Signature* e-mail. No login is needed.
2. The system opens the agreement with **View**, **Signature**, **Approval Action**, **One Time Pin** and **Submit**.
3. Tenant signs, selects *Approve*, enters the OTP, clicks **Submit** and confirms.
4. The system processes it as in step 9 and shows the confirmation page.

**Alternative flow 2: wrong OTP**

9a. The system shows **"Invalid One Time Pin Entered!"** and stays on the screen.

**Alternative flow 3: Reject**

6a. User selects *Reject*, types the **Rejecting Comment** and submits. The system sets *In Awaiting Agreement Of Lease Update* and assigns **Update Agreement**.

# UC-18 Upload Signed Lease Agreement

**Primary actor:** Housing Liaison Officer.
**Side nav:** Agreements › Finalized Agreement.

**Main flow**

1. User selects **Agreements** and then **Finalized Agreement**.
2. The system lists the user's leases under *Finalized Agreement Of Lease*.
3. User clicks **Upload Final Agreement**.
4. The system opens the agreement screen with a **Final Agreement Of Lease** upload panel.
5. User selects the signed file and clicks **Upload**. The system shows the upload progress and confirmation.
6. User selects **Approval Action** = *Approve* and clicks **Submit**, then **Yes, I am sure!**.
7. The system checks that the Final Agreement Of Lease document is uploaded, sets the application to *Signed Lease Agreement Uploaded*, closes the work item and removes the lease from the Finalized Agreement list.

**Alternative flow 1: no document**

7a. The system shows **"Please upload the finalized agreement!"** and does not submit.

**Alternative flow 2: Reject**

6a. User selects *Reject* with a **Rejecting Comment**. The system sets *In Awaiting Agreement Of Lease Update* and assigns **Update Agreement**.

**Alternative flow 3: No**

6b. User clicks **No!** on the pop-up; the pop-up closes and the screen is unchanged.

# Agreement Update (rework after a rejection)

**Primary actor:** Housing Liaison Officer.
**Side nav:** Agreements › Agreement Update.

1. User selects **Agreements** and then **Agreement Update**.
2. The system lists the user's leases under *Awaiting Agreement Update*.
3. User clicks **Update Agreement**.
4. The system opens the Capture Master wizard pre-filled with the lease (same four steps and fields as UC-13).
5. User corrects the details and clicks **Submit**.
6. The system saves the changes, sets the application to *In Awaiting Lease Agreement* and assigns **Generate Lease Agreement** to a Housing Liaison Officer. The agreement then runs through UC-14 to UC-18 again.

# UC-19 Capture Tenant Details (Manage Tenants)

**Primary actor:** Housing Liaison Officer.
**Side nav:** Department Tenants › Manage Tenants.

**Main flow**

1. User selects **Department Tenants** and then **Manage Tenants**.
2. The system lists the active leases (excluding vacated and deactivated ones) with *Lease Referrence No., Tenant Name, Application Type, Lease Date, Status, Action*.
3. User clicks **Manage** on a lease.
4. The system opens the tenant management screen with **Add New Occupant**, **List of Active Occupant/s** and **List of Deactive Occupant/s** (*Full Name, Title, Relationship, Identity No., Status, Action*).
5. User captures the occupant: First Name, Last Name, Relationship, Nominated (tick box), Identification Number, Title, Cell No., Email Address, and clicks **Add Occupant**.
6. The system shows **"Are you sure you want to add this occupant?"**; user clicks **Yes, I am sure!**.
7. The system saves the occupant as *Active Occupant* and shows **"Agreement Occupant Added Successfully."**
8. User clicks **Upload Documents**.
9. The system opens the document upload page for the lease (Identity Document, Payslip/Proof of Pension Grant, Bank Statemant, Confirmation of Employment, Affidavit Document, Proof of Residential Address, Additional Document). User browses, uploads and saves.

**Alternative flow 1: duplicate ID**

5a. The ID number already exists on the lease. The system shows **Error – "Occupant has been added with the same ID"**.

**Alternative flow 2: Activate / Deactivate**

1. In **List of Active Occupant/s** user clicks **Deactivate**; the occupant moves to **List of Deactive Occupant/s**.
2. In **List of Deactive Occupant/s** user clicks **Activate**; the occupant moves back to the active list.

**Alternative flow 3:** user clicks **Return To Manage** to go back to the lease list.

# UC-20 Transfer Lease

**Primary actor:** Housing Liaison Officer.
**Side nav:** Department Tenants › Transfer Lease.

**Main flow**

1. User selects **Department Tenants** and then **Transfer Lease**.
2. The system opens the transfer search with **Application Reference Number**.
3. User enters the lease reference number and clicks **Submit**.
4. The system shows **"Are you sure you want to this search this application?"**; user clicks **Yes, I am sure!**.
5. The system opens the lease (*Lease Reference No, Tenant Type, Status, Captured Date, Tenant Name, Action/View*) and the transfer form.
6. User selects **Reason For Transfer** (*Tenant Deceased* or *Other*) and **Preference** (the replacement tenant: one of the lease's active occupants, or the spouse captured on the lease). For *Other*, user can tick **Make main tenant an occupant?**. User confirms or edits **Email** and **Cell No.** and clicks **Submit**.
7. The system shows **"Are you sure you want to submit this recommendation?"**; user clicks **Yes, I am sure!**.
8. The system replaces the main tenant's name, ID and contact details on the same lease with the selected person's details, deactivates that person as an occupant, and for *Other* with the tick box adds the previous main tenant as an active occupant. It sends the "transfer successful" message and shows **"Agreement Of Lease Successfully Transferred to {name}."**

**Alternative flow 1: reference not found**

3a. The system shows *"The Reference Number {ref} Did Not Fid A Match, Try Again!"*.

**Alternative flow 2: tenant vacated**

3b. The system shows *"The tenant has vacated the unit for the ref. no entered!"*.

**Alternative flow 3: No**

4a/7a. User clicks **No!**; the pop-up closes and the screen is unchanged.

# UC-23 Send Communication to Tenants

**Primary actor:** Housing Liaison Officer, Senior Housing Specialist, Regional Manager, Caretaker.
**Side nav:** Communication › New Communication.

**Main flow: Individual**

1. User selects **Communication** and then **New Communication**.
2. The system opens **Send Tenant Communication**.
3. User selects **Communication Type** = *Individual*, ticks one or more of **SMS**, **EMAIL**, **POSTAL** under **Communication Method**, selects the tenant in **To Tenant** (only tenants in the user's allocated complexes are listed), types **Message Title** and **Message Body**, and clicks **Send Message/s**.
4. The system shows **"Are you sure you want to send this message?"**; user confirms.
5. The system queues the e-mail and/or SMS addressed to the tenant by full name, stores a copy of the message in the lease's history log, and shows how many messages were sent (e.g. *"1/1 message/s"*).

**Alternative flow 1: Per Complex**

3a. User selects **Communication Type** = *Per Complex*. The system shows **To Complex** with a tick box per complex the user is allocated to. User ticks the complex(es), types the title and body and clicks **Send Message/s**. The system sends to every tenant in the selected complexes, addressed as "Tenants".

**Alternative flow 2: mandatory fields missing**

3b. The browser highlights the missing required fields and the message is not sent.

# UC-29 Capture Notice to Terminate Lease (Serve Notice)

**Primary actor:** Housing Liaison Officer.
**Side nav:** Notice › Serve Notice.

**Main flow**

1. User selects **Notice** and then **Serve Notice**.
2. The system opens the notice search with **Application Reference Number**.
3. User enters the reference number and clicks **Submit**.
4. The system opens the lease (*Applicant Name, Application Type, Application Reference Number, Agreement Created Date, Status, Action/View*) and the **Notice Date** field.
5. User selects the notice (termination) date and clicks **Submit**.
6. The system shows **"Are you sure you want to send notice for this application?"**; user clicks **Yes, I am sure!**.
7. The system records the notice date on the lease (the lease stays active), sends the "serve notice" message, assigns **Noticed Agreements** to a Housing Liaison Officer and shows **Success!**.

**Alternative flow 1: invalid reference**

3a. The system shows *"The Reference Number You Have Entered Is Invalid, Try Again!"*.

**Alternative flow 2: tenant vacated**

3b. The system shows *"The tenant has vacated the unit for the ref. no entered!"*.

**Alternative flow 3: date rules**

5a. Less than a full calendar month away: *"Please select full calender month!"*. Not the last day of a month: *"Please select last day of the month!"*. The date is cleared.

**Alternative flow 4: No**

6a. User clicks **No!**; the pop-up closes and the screen is unchanged.

# UC-34 Serve Notice (Noticed Agreements)

**Primary actor:** Housing Liaison Officer.
**Side nav:** Terminations › Noticed Agreements.
**Pre-condition:** a notice was captured in UC-29.

**Main flow**

1. User selects **Terminations** and then **Noticed Agreements**.
2. The system lists the user's noticed leases (*Applicant Name, Application Type, Application Reference Number, Application Created Date, Status, Action*).
3. User clicks **Terminate Agreement**.
4. The system opens the termination screen for that lease (as UC-30 step 4) with **Notice Date** (read-only), **Termination Date** and **Termination Reason**, where *Tenant Notice* is also offered.
5. User selects the Termination Date and **Termination Reason** = *Tenant Notice* and clicks **Submit**, then **Yes, I am sure!**.
6. The system records the termination, sets the lease to *Termination Due To Tenant Notice*, closes the Noticed Agreements item, assigns **Pending Termination Review** to a Senior Housing Specialist and returns to the Noticed Agreements list.

**Alternative flows:** date rules and **No!** as UC-30.

# UC-30 Capture Lease Termination (Terminate Agreement)

**Primary actor:** Housing Liaison Officer.
**Side nav:** Terminations › Terminate Agreement.

**Main flow**

1. User selects **Terminations** and then **Terminate Agreement**.
2. The system opens **Agreement Of Lease Termination** with **Application Reference Number**.
3. User enters the reference number and clicks **Submit**; confirms with **Yes, I am sure!**.
4. The system opens **Evaluating Application** with the lease row, **Notice Date** (only if a notice was served), **Termination Date** and **Termination Reason**.
5. User selects **Termination Date** and **Termination Reason** (*Lease not renewed* or *Tenant Deceased*; *Tenant Notice* is added when a notice was served) and clicks **Submit**.
6. The system shows **"Are you sure you want to send notice for this application?"**; user clicks **Yes, I am sure!**.
7. The system creates the termination record (status *In Awaiting Termination Approval*), sets the lease to *Termination Due To Lease Not Renewed* / *Termination Due To Tenant Deceased* / *Termination Due To Tenant Notice*, assigns **Pending Termination Review** to a Senior Housing Specialist and shows **Success!**.

**Alternative flow 1: invalid reference / vacated** – as UC-29.

**Alternative flow 2: date rules**

5a. Date after the lease end: *"The lease terminates before date selected!"*. Less than a full calendar month: *"Please select full calender month!"*. Not the last day of the month: *"Please select last day of the month!"*.

**Alternative flow 3: No** – the pop-up closes and the screen is unchanged.

# UC-31 Review Lease Termination and Validate Account

**Primary actor:** Senior Housing Specialist.
**Side nav:** Terminations › Pending Review.

**Main flow**

1. User selects **Terminations** and then **Pending Review**.
2. The system lists the user's leases with a termination status (Tenant Notice, Lease Not Renewed, Tenant Deceased, End of Lease Term).
3. User clicks **Review**.
4. The system opens **Evaluating Application**, **Tenant Account Balance** (*Deposit Amount Paid*, *Amount Due*) and **Upload Financial Documents** (Financial Supporting Document, Additional Document).
5. User uploads the financial documents (browse, **Upload**), selects **Approval Status** = *Approve* and clicks **Submit**.
6. The system shows **"Are you sure you want to send notice for this application?"**; user clicks **Yes, I am sure!**.
7. The system sets the lease to *In Awaiting Exit Inspection*, updates the lease termination dates, closes the review item and assigns **Post Inspection** to a Housing Liaison Officer.

**Alternative flow: No** – the pop-up closes and the screen is unchanged.

# UC-32 Conduct Exit Unit Inspection

**Primary actor:** Housing Liaison Officer.
**Side nav:** Inspections › Post-Inspection.

**Main flow**

1. User selects **Inspections** and then **Post-Inspection**.
2. The system lists the user's leases in *In Awaiting Exit Inspection*.
3. User clicks **Exit Inspection**.
4. The system opens the exit inspection screen: the lease row (*Applicant Name, Application Type, Application Reference Number, Agreement End Date, Status, Action/View*), **Conduct Exit Inspection Template** (download the blank form) and **Upload Completed Exit Inspection** (Completed Post-Insection Document, Additional Document).
5. User downloads the template, completes the inspection, uploads the completed document(s) with **Upload**.
6. User selects **Approval Status** (*Habitable - No defects*, *Habitable - Minor defects* or *Not Habitable*) and clicks **Submit**.
7. The system shows **"Are you sure you want to submit the following document approval?"**; user clicks **Yes, I am sure!**.
8. For all three outcomes the system sets the lease and application to *Awaiting CSO Vacating Confirmation*, sends the tenant a notice message, closes the inspection item and assigns **Pending Take-Off** to a Housing Liaison Officer.

**Alternative flow: No** – the pop-up closes and the screen is unchanged.

# UC-33 Confirm Tenant Vacated

**Primary actor:** Housing Liaison Officer.
**Side nav:** Terminations › Pending Take-Off.

**Main flow**

1. User selects **Terminations** and then **Pending Take-Off**.
2. The system lists the user's leases in *Awaiting CSO Vacating Confirmation*.
3. User clicks **Confirmation**.
4. The system opens **Evaluating Application** and **Vacating Applicant Action** with **Tenant Vacated** (*Vacated* / *Not Vacated*).
5. User selects *Vacated* and clicks **Submit**.
6. The system shows **"Are you sure you want to confirm this applicant vacating?"**; user clicks **Yes, I am sure!**.
7. The system sets the lease and application to *Applicant Vacated The Unit*, closes the take-off item and returns to Pending Take-Off.

**Alternative flow 1: Not Vacated**

5a. User selects *Not Vacated*. The lease stays in *Awaiting CSO Vacating Confirmation* and the take-off item is closed.

**Alternative flow 2: No** – the pop-up closes and the screen is unchanged.

# Appendix A: Side navigation by role

**Housing Liaison Officer**

| Menu | Sub-menu items |
|---|---|
| PLM Applications | All Applications, Capture Master, Dashboard, Master Applications |
| Agreements | Agreement Generate, Agreement Signature, Agreement Update, Finalized Agreement |
| Agreement Renewal | Recommendations, Pending Acceptance, Pending Renewal |
| Communication | New Communication |
| Department Tenants | Manage Tenants, Transfer Lease |
| Notice | Serve Notice |
| Inspections | Post-Inspection |
| Terminations | Noticed Agreements, Pending Take-Off, Terminate Agreement |

**Senior Housing Specialist**

| Menu | Sub-menu items |
|---|---|
| PLM Applications | All Applications, Dashboard, Master Applications |
| Agreements | Agreement Review |
| Agreement Renewal | Pending Review |
| Communication | New Communication |
| Terminations | Pending Review |

**Regional Manager (Senior Manager Operations)**

| Menu | Sub-menu items |
|---|---|
| PLM Applications | All Applications, Dashboard, Master Applications |
| Agreements | Agreement Approval |
| Agreement Renewal | Pending Approval |
| Communication | New Communication |

**Caretaker**

| Menu | Sub-menu items |
|---|---|
| PLM Applications | All Applications, Master Applications |
| Communication | New Communication |

**Back Office System Administrator / Super Administrators** see the HSD menus with every queue: PLM Applications (Dashboard, All Applications, Master Applications, Capture Master); Agreements (Agreement Generate, Agreement Review, Agreement Approval, Agreement Signature, Agreement Update, Finalized Agreement); Department Tenants (Manage Tenants, Transfer Lease); Agreement Renewal (Recommendations, Pending Review, Pending Approval, Pending Acceptance, Pending Renewal); Communication (New Communication); Notice (Serve Notice); Inspections (Post-Inspection); Terminations (Noticed Agreements, Terminate Agreement, Pending Review, Pending Take-Off).

**Tenant (Customers role, HSD department):** no HSD side-nav menu. Tenants act only through the e-mail links for renewal acceptance (UC-27) and agreement signature (UC-17). One hard-coded test account (`Hsduser09`) sees a test menu; it is not part of the HSD process.

# Appendix B: Work queues

| Queue (work item) | Assigned to | Created by | Side nav |
|---|---|---|---|
| Renewal Recommendation | Housing Liaison Officer | Renewal Notice date reached | Agreement Renewal › Recommendations |
| Review Lease Renewal Recommendation | Senior Housing Specialist | UC-24 | Agreement Renewal › Pending Review |
| Approve Lease Renewal | Regional Manager | UC-25 | Agreement Renewal › Pending Approval |
| Accept Lease Renewal | Housing Liaison Officer | UC-26 | Agreement Renewal › Pending Acceptance |
| Renew Lease | Housing Liaison Officer | UC-27 | Agreement Renewal › Pending Renewal |
| Generate Lease Agreement | Housing Liaison Officer | UC-28, Agreement Update | Agreements › Agreement Generate |
| Agreement Review | Senior Housing Specialist | UC-14 | Agreements › Agreement Review |
| Approve Lease Agreement | Regional Manager | UC-15 | Agreements › Agreement Approval |
| Accept Lease Agreement | Housing Liaison Officer | UC-16 | Agreements › Agreement Signature |
| Finalized Agreement | Housing Liaison Officer | UC-17 | Agreements › Finalized Agreement |
| Update Agreement | Housing Liaison Officer | any agreement rejection | Agreements › Agreement Update |
| Noticed Agreements | Housing Liaison Officer | UC-29 | Terminations › Noticed Agreements |
| Pending Termination Review | Senior Housing Specialist | UC-30, UC-34 | Terminations › Pending Review |
| Post Inspection | Housing Liaison Officer | UC-31 | Inspections › Post-Inspection |
| Pending Take-Off | Housing Liaison Officer | UC-32 | Terminations › Pending Take-Off |
