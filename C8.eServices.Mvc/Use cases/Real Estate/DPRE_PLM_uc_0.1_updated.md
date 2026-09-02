---

title: Real Estate Lease Management System - Use Cases (UC 01 to UC 20)

label: DPRE PLM uc 0.1 updated

source: Business Analyst Email & Policy Framework 2025-2027

document_type: Use Case Specifications

system_target: Property Lease Management System

parser_target: ANTIGRAVITY AI Engine

---



# 📝 Version Updates (DPRE PLM uc 0.1 updated)



**Based on the email instructions from Ayanda and the 2025-2027 Policy Framework for Management of Township Facilities, the following updates have been verified and integrated into UC 05 (Submit Application for Lease):**



1. **Removed Banking Details (Step 5):** The requirement to capture Banking Institution, Account Number, Branch Code, and Type of Account has been entirely removed for both Entity and Individual applicant types.

2. **Updated List of Facilities & Dynamic Selection (Steps 9-13):** Replaced static checkboxes with dynamic dropdown logic. Step 9 captures the Customer Care Area (CCC), Step 11 dynamically filters the specific Facility Name, and Step 13 captures the number and type of unit to dynamically calculate the rental amount.

3. **Updated Pre-Qualification Documents (Step 15):** The mandatory document upload list has been expanded. It now requires a Business Plan, Declaration of Interest (Form MBD 4), 3-years Audited Financial Statements (or equivalents), and a detailed Facilities Management Experience description. ID copies must be under 6 months old, and the Company Profile must include Empowerment Plans, Job Creation, and Social Benefits.



*(Note: UC 01 to UC 04 were parsed and compared; no undocumented changes or deviations were found. They contain standard profile registration, update, approval, and login flows).*



---



# Real Estate Lease Management System - Use Cases (UC 01 to UC 20)



## UC 01: Register User Profile

* **Use Case Number:** UC-01

* **Description:** A new applicant registers on the system to create a verified profile. Registration captures personal and contact details, sends OTP/verification, and enables access to the system to enable submission of lease applications.

* **Primary Actor:** Applicant

* **Secondary Actor(s):** Property Lease Management System

* **Pre-condition(s):** Access to the internet

* **Trigger:** Need to register a profile with Property Lease Management System

* **Success End Condition:** User profile registration email sent. Account successfully activated.

* **Failed End Condition:** User profile not successfully registered. User account not activated.

* **mSCOA ID:** N/A | **Integration:** N/A



**Business Requirements:**

* **FR1:** Profile Registration

* **FR1.1:** Functionality for Lease Applicant to register profile



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Navigate to the Property Lease Management > Development Planning & Real Estate > PLM Login Page. | Step 2 | The Development Planning & Real Estate - PLM Login Page displays |

| Step 3 | Clicks on the Register Link | Step 4 | Displays the Registration Page |

| Step 5 | Capture the following details: First Name, Last Name, RSA ID Number, Username, Password, Confirm Password, Mobile Number, Confirm Mobile Number, Email Address, Confirm Email Address. Select preferred method of communication. Complete the reCAPTCHA. Click on the Register Button. | Step 6 | Validates the field information entered |

| Step 7 | Accepts the Terms and Conditions by scrolling to the bottom of the page and tick the checkbox | Step 8 | Displays confirmation message that the email has been sent. |

| Step 9 | Clicks the OK button on the confirmation pop-up. | Step 10 | Displays the Registration Page. |



### User Account Activation (Outside of the system)

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Clicks on the email link | Step 2 | Displays message Account Successfully activated. |



### Alternative Flows

**Alternative Flow 1 (Alternative at Step 5 - Invalid Fields)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 5 | User didn’t enter some of the fields or entered invalid fields information before clicking Register Button | Step 6 | Displays error message and prompt user to enter the valid information |



---



## UC 02: Update User Profile

* **Use Case Number:** UC-02

* **Description:** The process of updating profile by the Applicant in the Property Lease Management System.

* **Primary Actor:** Applicant

* **Secondary Actor(s):** Property Lease Management System

* **Pre-condition(s):** Applicant must have activated the account after registering.

* **Trigger:** Need to update the profile after activating the account.

* **Success End Condition:** Profile successfully updated.

* **Failed End Condition:** Unable to update profile.



**Business Requirements:**

* **FR1.1.1:** Functionality to Update User Profile



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Access the system: Capture credentials > Click the Login Button | Step 2 | Logs in Applicant, and directed to the Customer Information page to update the user profile |

| Step 3 | Clicks the “Update Profile” option | Step 4 | Displays Manage Profile Details page |

| Step 5 | Capture the details to be updated:<br>- Residential Address (Street Address, Suburb/Town, Postal Code)<br>- Postal/Billing Address (Street Address, Suburb/Town, Postal Code)<br>Click Save. | Step 6 | Displays Pop-up message “Are the updated details correct?” |

| Step 7 | Clicks Yes button | Step 8 | Closes the Pop-up message |



### Alternative Flows

**Alternative Flow 1 (Alternative at Step 5 - Missing Info)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 11 | User doesn’t enter any information in mandatory fields | Step 12 | System displays error message and prompts user to capture the missing fields |



---



## UC 03: Approve Profile Registration 

* **Use Case Number:** UC-03

* **Description:** Approve Customer on Development Planning and Real Estate – PLM system.

* **Primary Actor:** System Administrator

* **Secondary Actor(s):** Development Planning and Real Estate – PLM system

* **Pre-condition(s):** User must be successfully logged into the system. Customer has registered a profile.

* **Trigger:** Registered profile needs to be approved.

* **Success End Condition:** Customer profile is approved.

* **Failed End Condition:** Customer profile is not approved.



**Business Requirements:**

* **FR1.2:** Functionality for Backoffice Official to approve the profile



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Click on Maintenance option from the navigation panel | Step 2 | Maintenance option will expand |

| Step 3 | Select Pending Approval | Step 4 | All Customer applications pending approval applications displays |

| Step 5 | Select the desired user for approval from the grid-view | Step 6 | System accepts user selection |

| Step 7 | Click the Details button under the Action column | Step 8 | Customer Profile Information will be displayed |

| Step 9 | Review information > Click the Approve Profile button | Step 10 | Confirmation Message will display |

| Step 11 | Click the OK button | Step 12 | Pop-up will close. Maintenance Page Reloads |



### Alternative Flows

**Alternate Flow 1 (Alternative at Step 9 - Reject)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 9 | Reviews customer profile information and Click Reject Profile | Step 10 | Accepts user selection and displays the mandatory Reason text field |

| Step 11 | Capture Reason for Rejection | Step 12 | System displays pop-up message "Are you sure you want to submit the feedback?" |

| Step 13 | Click on the “Yes, I am sure” button | Step 14 | Notification sent to Applicant with captured reason |



**Alternate Flow 2 (Alternative at Step 13 - Cancel)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 13 | Click Cancel | Step 14 | System cancels transaction |



---



## UC 04: Login

* **Use Case Number:** UC-04

* **Description:** The process of user logging into the Property Lease Management System.

* **Primary Actor:** Applicant

* **Secondary Actor(s):** Property Lease Management System

* **Pre-condition(s):** Registered profile or login credentials to access the Property Lease Management System.

* **Trigger:** Need to access the Property Lease Management System.

* **Success End Condition:** User logged in successfully into the system.

* **Failed End Condition:** User does not log in successfully into the system.

* **mSCOA ID:** RT25-013; RT25-015 | **Integration:** `IR01` Identity and Access Management System



**Business Requirements:**

* **FR2:** Login | **FR2.1:** Login functionality

* **FR3:** Dashboard | **FR3.1:** Dashboard view of all the leases including their status, and visual representation of the leased premises



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Clicks the Property Lease management Link | Step 2 | Presents the Login screen |

| Step 3 | Enters the login credentials and click SIGN IN | Step 4 | Validates the information entered and grants user access to the system |



### Alternative Flows

**Alternate Flow 1 (Alternative at Step 3 - Incorrect Credentials)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 3 | User enters incorrect information | Step 4 | System displays error message and prompts user to enter correct credentials |



**Alternate Flow 2 (Alternative at Step 3 - Forgotten Password)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 3 | User clicks Forgot your password? Option | Step 4 | Gives the user options to select email address / Mobile number |

| Step 5 | Select either Email or Mobile number radio button | Step 6 | Prompts user to enter the email address/ Mobile number to recover the password |

| Step 7 | Provide email address / Mobile number and click on reCAPTCHA checkbox | Step 8 | Email / Mobile number successfully accepted, and the user successfully checked reCAPTCHA and Images presented |

| Step 9 | Select images to confirm that you are not a robot | Step 10 | Images successful selected |

| Step 11 | Click “Reset” password | Step 12 | Notification presented on the system “Please check your email/phone for password reset. Use the details provided to reset” |

| Step 13 | Login to the system using Username and Temporary password received on the email. | Step 14 | Manage account page presented and change password presented below, with the following text fields: Current password, New password and Confirm new password |

| Step 15 | User provide Current password, New password and Confirm new password and Click “Save” | Step 16 | Notification presented “Your password has been changed successfully.” |



---



## UC 05: Submit Application for Lease

* **Use Case Number:** UC-05

* **Description:** The process for an Applicant to submit a Lease Application.

* **Primary Actor:** Applicant, Property Officer

* **Secondary Actor(s):** Property Lease Management System

* **Pre-condition(s):** Must be successfully logged into the system.

* **Trigger:** Need to apply for space to let with CoE (Development Planning & Real Estate Department).

* **Success End Condition:** Lease Application successfully submitted.

* **Failed End Condition:** Lease Application not submitted.

* **mSCOA ID:** N/A | **Integration:** N/A



**Business Requirements:**

* **FR4:** Lease Application Functionality (`FR4.1` to `FR4.6`, `FR4.9` to `FR4.11`, `FR4.14`).



**Business Rules:**

* **BR01:** The following statuses must be available on the system: Application Submitted, Pending Review; In Circulation for Evaluation; Evaluation Outcome (Recommended/Not Recommended); Valuation Requested; Application Concluded & Approved.

* **BR02:** Applications must include ID, residency documents and other supporting documents.

* **BR03:** Facilities must only be used for these categories: 1. Retails Units; 2. Offices/Professional Units.



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Clicks on Applications | Step 2 | Opens Lease Application Screen with details obtained from the user profile:<br>- Residential Address (Street, Suburb/Town, Postal Code)<br>- Postal/Billing Address (Street, Suburb/Town, Postal Code) |

| Step 3 | Selects Applicant Type from the dropdown list: Individual, Close Corporation, Company (PTY LTD)/Partnership, NPO/NGO, Government Entity (Organ of State) | Step 4 | Accepts user selection |

| Step 5 | **If Non-Individual Type is selected:**<br>Capture Name of Entity/Business, Company Regn. Number, VAT Regn. Number, Tax Reference Number, Entity Registered Address, Postal Code, Authorized Representative, Capacity, Telephone number, Mobile Number, Fax number, E-mail address.<br><br>**If Individual Applicant Type is selected:**<br>View/Capture the following information: Applicant’s surname, Applicant’s full name, Gender, Nationality, ID Number, Tax Reference Number, Residential Address, Postal/Billing Address, Contact Details (Mobile, Home, Fax, E-mail). | Step 6 | Accepts user input |

| Step 7 | Select Purpose of Lease from the dropdown: Retail, Office/Professional | Step 8 | Accepts user selection |

| Step 9 | Select Customer Care Area from the CCC dropdown list | Step 10 | Accepts user selection and displays available facilities in selected CCC |

| Step 11 | Select Facility Name from the list | Step 12 | Displays information |

| Step 13 | Select Number and Type of unit required (can select more than 1 unit in the facility) | Step 14 | Accepts user selection and displays indicative monthly rental amount |

| Step 15 | Browse and upload Mandatory Pre-Qualification Documents:<br>• Certified Applicant(s) ID(s) (Not more than 6 months old)<br>• Proof of Address<br>• CIPC Compliance Documents (e.g. Company Registration Documents)<br>• SARS Tax Clearance Certificate<br>• Company Profile/Proposal (Company Directorship/Ownership structure, Management Experience, Number of jobs to be created, Empowerment Plan, Social Benefits)<br>• A description of Facilities Management Experience and/or accreditation<br>• Contactable References<br>• Supporting/Motivational Letters<br>• 3-years Audited Financial Statements (Letter of Funding/Proof of Income (3 months Bank Statement)/Management Accounts/Affidavit)<br>• Proof of Application Fee Payment<br>• Business Plan<br>• Declaration of Interest (Form MBD 4)<br><br>Click the Submit button. | Step 16 | Confirmation pop-up displays “Are you sure you want to submit this application?” |

| Step 17 | Click on the “Yes, I am sure” button | Step 18 | Displays success message and autogenerates Application Reference Number. Reference Number notification sent to Applicant. Application Status = `Application Submitted, Pending Review` |



*(Note: System must have a Save and Continue function or Auto Save to enable the user to pick up from where they left.)*



### Alternative Flows

**Alternate Flow 1 (Alternative Flow at Step 15 - Missing Docs)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 15 | Doesn’t upload the documents | Step 16 | Error message displays to indicate mandatory document to be submitted |



**Alternate Flow 2 (Alternative Flow at Step 17 - Cancel)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 17 | Click the Cancel button | Step 18 | System cancels transaction |



---



## UC 06: Validate Proof of Payment

* **Use Case Number:** UC-06

* **Description:** The process of validating application fee payment.

* **Primary Actor:** Bookkeeper/Finance Officer

* **Secondary Actor(s):** Property Lease Management System

* **Pre-condition(s):** Must be successfully logged into the system.

* **Trigger:** Need to validate application fee payment.

* **Success End Condition:** Payment successfully validated.

* **Failed End Condition:** Payment not successfully validated.

* **mSCOA ID:** N/A | **Integration:** N/A



**Business Requirements & Rules:**

* **FR4.12:** Functionality for Back-office Officials to validate proof of payment uploaded by Applicant.

* **BR04:** Approval or Rejections must be conducted or governed by Workflow processes.



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Real Estate Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Validate Proof of Payment | Step 4 | Displays “Awaiting Payment Validation” Screen and list of applications with status `Application Submitted, Pending Review` |

| Step 5 | Selects the Application whose payment is to be validated by clicking on “View” | Step 6 | Opens Attached Documents Screen |

| Step 7 | Check the “Proof of Payment” document and select Approve to approve that the document is aligning and Click Submit | Step 8 | System presents a notification “Are you sure you want to submit the following document for approval?” |

| Step 9 | Click on “Yes, I am sure” | Step 10 | System closes notification. Application status = `Awaiting Risk Assessment Outcome` |



### Alternative Flows

**Alternative Flow 1 (Alternative at Step 7 - Rejected)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 7 | Check the document and select Rejected > Capture Reason and Click Submit | Step 8 | Confirmation page is displayed |

| Step 9 | Click on “Yes, I am sure” | Step 10 | Notification is sent to the Applicant with rejection based on reasons submitted on the captured Reason. Application status = `Application Submitted, Pending Review` |



---



## UC 07: Capture Risk Assessment Outcome

* **Use Case Number:** UC-07

* **Description:** The process for the Property Officer to capture Risk Assessment Outcome on the application.

* **Primary Actor:** Property Officer

* **Secondary Actor(s):** Property Lease Management System

* **Pre-condition(s):** Must be successfully logged into the system.

* **Trigger:** Lease Application Awaiting Risk Assessment Outcome.

* **Success End Condition:** Risk Assessment Outcome successfully captured.

* **Failed End Condition:** Risk Assessment Outcome not successfully captured.

* **Integration:** `IR08` (Credit Bureaus), `IR09` (Home Affairs), `IR10` (Deeds), `IR11` (SASSA), `IR12` (CIPC) - *(Not Ready for Testing)*.

* **Business Requirements:** `FR4.7` - Validate submitted details against the relevant systems.



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Real Estate Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Capture Risk Assessment Outcome | Step 4 | Opens the Risk Assessment Response Screen with All applications that are in `Awaiting Risk Assessment Outcome` status |

| Step 5 | Search or select an application from the list and tab Assess | Step 6 | Application details page presented and system accepts user action. Outside the system, the User accesses the various vetting agencies. |

| Step 7 | On the Risk Assessment Outcome section, Capture: Credit Bureau Result, Home Affairs Result, Deeds Result, SASSA Result, CIPC Result | Step 8 | Displays information |

| Step 9 | Capture Risk Assessment Outcome by selecting Recommended from the dropdown list | Step 10 | System accepts user selection |

| Step 11 | Capture Reason | Step 12 | Accepts user input |

| Step 13 | Browse and Upload supporting evidence from the vetting agencies | Step 14 | Accepts user input |

| Step 15 | Click on the Submit button | Step 16 | Message presented “Are you sure you want to submit the following recommendation?” |

| Step 17 | Click on “Yes, I am sure” | Step 18 | Success message displayed. Application status = `Verified` (Final outcome will be determined by Committee decision/feedback) |



### Alternative Flows

**Alternate Flow 1 (Alternative at Step 9 - Not Recommended)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 9 | User selects Not Recommended on Risk Assessment Outcome | Step 10 | System accepts user selection |

| Step 11 | Capture Reason (Mandatory) | Step 12 | Accepts user input |

| Step 13 | Click on the Submit button | Step 14 | Notify Applicant of the Risk Assessment Outcome based on reasons submitted on the captured Reason |



**Alternate Flow 2 (Alternative at Step 17 - Cancel)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 17 | User selects Cancel | Step 18 | Cancels transaction and retains the status to `Awaiting Risk Assessment Outcome`. |



---



## UC 08: Initiate Departmental Review

* **Use Case Number:** UC-08

* **Description:** The process to submit Lease Application for Departmental Comments.

* **Primary Actor:** Property Officer

* **Secondary Actor(s):** Property Lease Management System, Departmental Representatives

* **Pre-condition(s):** Application captured and validated, Required documents uploaded, Application fee payment confirmed, User logged in.

* **Trigger:** Lease application submitted and risk assessment verified.

* **Integration:** `IR01` (Identity and Access Management System)

* **Business Requirements:** `FR4.8` - Send workflow for review, comments, and approval.

* **Business Rules:** `BR02` - Applications must include ID, residency documents and other supporting documents.



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Real Estate Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Initiate Departmental Review | Step 4 | Displays list of applications with status = `Verified` |

| Step 5 | Selects application of interest | Step 6 | Accepts user action |

| Step 7 | Click on Initiate Review | Step 8 | System retrieves configured list of departments and officials/delegates |

| Step 9 | Click on Send for Review | Step 10 | Message presented “Are you sure you want to submit the application for departmental review?” |

| Step 11 | Click on “Yes, I am sure” | Step 12 | System routes application and send notification to all departments/delegates and HoD. Application status = `In Circulation for Evaluation` |



### Alternative Flows

**Alternate Flow 1 (Alternative at Step 7 - Missing Config)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 7 | Click on Initiate Review | Step 8 | No departments shown, “Please configure reviewing departments” |



**Alternate Flow 2 (Alternative at Step 11 - Cancel)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 11 | Click on Cancel | Step 12 | Cancels transaction |



---



## UC 09: Capture Departmental Reviews and Comments

* **Use Case Number:** UC-09

* **Description:** The process to enable departments to capture their review outcome.

* **Primary Actor:** Departmental Official/Representative

* **Secondary Actor(s):** Property Lease Management System, Departmental Representatives

* **Pre-condition(s):** Application assigned to department, User authenticated and logged in.

* **Trigger:** Application received in departmental queue.

* **Integration:** `IR01`

* **Business Requirements:** `FR4.9`, `FR5.1`

* **Business Rules:** `BR01`, `BR02`



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Real Estate Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Capture Departmental Reviews and Comments | Step 4 | Displays list of applications with status = `In Circulation for Evaluation` |

| Step 5 | Selects application of interest | Step 6 | Accepts user action and displays application details |

| Step 7 | Reviews application completeness as per pre-defined criteria > Select Review Outcome from dropdown: Supported | Step 8 | Accepts user selection |

| Step 9 | Capture Comments | Step 10 | Accepts user input |

| Step 11 | Browse and Upload Supporting Documents | Step 12 | Accepts user action |

| Step 13 | Click on Submit | Step 14 | Message presented “Are you sure you want to submit the following departmental review?” |

| Step 15 | Click on “Yes, I am sure” | Step 16 | Updates application status to the Review Outcome selected in Step 7 |



### Alternative Flows

**Alternate Flow 1 (Alternative at Step 7 - Supported with Conditions or Not Supported)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 7 | Select Review Outcome from dropdown: Supported with Conditions or Not Supported | Step 8 | Accepts user selection |

| Step 9 | Capture Comments (Mandatory) | Step 10 | Accepts user input |

| Step 11 | Browse and Upload Supporting Documents | Step 12 | Accepts user action |

| Step 15 | Click on Submit | Step 16 | Message presented “Are you sure...” |

| Step 15 | Click on “Yes, I am sure” | Step 16 | Updates application status to the Review Outcome selected with captured comments |



**Alternate Flow 2 (Alternative at Step 7 - Request Additional Information)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 7 | Select Review Outcome from dropdown: Request Additional Information | Step 8 | Accepts user selection |

| Step 9 | Capture Comments (Mandatory) | Step 10 | Accepts user input |

| Step 11 | Click on Submit | Step 12 | Message presented “Are you sure...” |

| Step 13 | Click on “Yes, I am sure” | Step 14 | Updates application status to the Review Outcome selected. Notification sent to Property Officer and Applicant with the captured comments |



**Alternate Flow 3 (Alternative at Step 15 - Cancel)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 11 | Click on Cancel | Step 12 | Cancels transaction |



---



## UC 10: Consolidate Departmental Feedback

* **Use Case Number:** UC-10

* **Description:** The process to consolidate departmental inputs.

* **Primary Actor:** Property Officer

* **Secondary Actor(s):** Property Lease Management System, Departmental Representatives, DPRE Evaluation Committee, DH, HoD

* **Pre-condition(s):** Department responses available, User logged in.

* **Trigger:** Departmental responses.

* **Integration:** `IR01`

* **Business Requirements:** `FR4.8` | **Business Rules:** `BR04`



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Real Estate Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Consolidate Departmental Feedback | Step 4 | Displays list of applications reviewed by departments |

| Step 5 | Selects application of interest | Step 6 | Accepts user action and displays departmental reviews |

| Step 7 | Reviews inputs and resolve conflicting inputs (if applicable) > Click on Generate Consolidated Report | Step 8 | System compiles: Summary of outcomes, List of conditions, Rejection flags |

| Step 9 | Click on Submit | Step 10 | Message presented “Are you sure you want to submit the application for Committee review?” |

| Step 11 | Click on “Yes, I am sure” | Step 12 | System routes application to DPRE Evaluation Committee Secretariat and send notification to HoD. Application status = `Pending Evaluation Committee Outcome` |



### Alternative Flows

**Alternate Flow 1 (Alternative at Step 7 - Escalate)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 7 | If any “Not Supported” are available, Compile the outcomes > Click on the Escalate button | Step 8 | Sends escalation notification to HoD for Decision or Rejection Recommendation. Application status = `Valuation Requested` |



**Alternate Flow 2 (Alternative at Step 11 - Cancel)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 11 | Click on Cancel | Step 12 | Cancels transaction |



---



## UC 11: Committee Review and Decision

* **Use Case Number:** UC-11

* **Description:** The process to Review Lease Application at Committee Level.

* **Primary Actor:** Committee Member/Secretariat

* **Secondary Actor(s):** Property Lease Management System, Property Officer, HoD

* **Pre-condition(s):** All departmental inputs captured, Item scheduled for Committee, User logged in.

* **Trigger:** Consolidated report submitted.

* **Integration:** `IR01`

* **Business Requirements:** `FR4.8`, `FR4.14`, `FR5.2`

* **Business Rules:** `BR01`, `BR04`



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Real Estate Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Committee Review and Decision | Step 4 | Displays list of applications with status = `Pending Evaluation Committee Outcome` |

| Step 5 | Selects application of interest | Step 6 | Accepts user action and displays application details |

| Step 7 | Capture Committee Reviews feedback: Application details, Departmental comments, Risk/financial implications | Step 8 | Displays information |

| Step 9 | Select Committee Decision from the dropdown list: Recommended, Recommended with Conditions, Not Recommended | Step 10 | Accepts user selection |

| Step 11 | Capture Committee Comments/ Resolution (Mandatory) | Step 12 | Displays information |

| Step 13 | Browse and Upload Committee Resolution Document (Mandatory) | Step 14 | Accepts user action |

| Step 15 | Click on Submit | Step 16 | Message presented “Are you sure you want to submit the application for final authorisation?” |

| Step 17 | Click on “Yes, I am sure” | Step 18 | Notification sent to HoD and Applicant. Application status = Evaluation Committee Outcome selected in Step 9 |



### Alternative Flows

**Alternate Flow 1 (Alternative at Step 9 - Deferred)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 9 | Select Committee Decision from the dropdown list: Deferred | Step 10 | Accepts user selection |

| Step 11 | Capture Deferral Reason | Step 12 | Displays information |

| Step 13 | Upload Committee Minutes & Resolution documents | Step 14 | Accepts user action |

| Step 15 | Click on Submit | Step 16 | Message presented “Are you sure...” |

| Step 17 | Click on “Yes, I am sure” | Step 18 | Notification sent to HoD and Applicant with captured Deferral Reason. Application status = `Pending Evaluation Committee Outcome` |



**Alternate Flow 2 (Alternative at Step 17 - Cancel)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 17 | Click on Cancel | Step 18 | Cancels transaction |



---



## UC 12: Application Final Authorisation

* **Use Case Number:** UC-12

* **Description:** The process to Provide Final Authorisation.

* **Primary Actor:** HoD

* **Secondary Actor(s):** Property Lease Management System, Property Officer, Applicant, DPRE Evaluation Committee members

* **Pre-condition(s):** Committee decision captured, Supporting documents available, Valuation Requests submitted, User logged in.

* **Trigger:** Committee recommendation submitted.

* **Integration:** `IR01`

* **Business Requirements:** `FR4.8`, `FR4.10`, `FR4.14`, `FR14.1`, `FR14.2`

* **Business Rules:** `BR01`, `BR04`



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Real Estate Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Application Final Authorisation | Step 4 | Displays list of applications in Worklist (Statuses = Valuation Requested; Evaluation Committee Outcome decisions; PTO status = Awaiting Approval) |

| Step 5 | Selects application of interest | Step 6 | Accepts user action and displays application details |

| Step 7 | Capture Committee Reviews feedback: Application details, All departmental comments, Committee decisions | Step 8 | Displays information |

| Step 9 | Select Final Outcome from the dropdown list: Approved, Approved with Conditions | Step 10 | Accepts user selection |

| Step 11 | Capture Final Comments (Mandatory) | Step 12 | Displays information |

| Step 13 | Sign on the Signature Box | Step 14 | Accepts user action |

| Step 15 | Click on Submit | Step 16 | Message presented “Are you sure...” |

| Step 17 | Click on “Yes, I am sure” | Step 18 | Notification sent to Property Officer, Applicant, DPRE Evaluation Committee members. Application status = `Application Concluded & Approved` |



### Alternative Flows

**Alternate Flow 1 (Alternative at Step 7 - Rejected)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 7 | Select Final Outcome from the dropdown list: Rejected | Step 8 | Accepts user selection |

| Step 9 | Capture Rejection Reason (Mandatory) | Step 10 | Displays information |

| Step 13 | Sign on the Signature Box | Step 14 | Accepts user action |

| Step 15 | Click on Submit | Step 16 | Message presented “Are you sure...” |

| Step 15 | Click on “Yes, I am sure” | Step 16 | Notification sent to Property Officer, Applicant, DPRE Evaluation Committee members with captured Rejection Reason. Application status = `Application Concluded & Rejected` |



**Alternate Flow 2 (Alternative at Step 15 - Cancel)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 15 | Click on Cancel | Step 16 | Cancels transaction |



---



## UC 13: Schedule Unit Inspection

* **Use Case Number:** UC-13

* **Description:** To schedule an inspection of a unit/property for occupancy, periodic review, handover, exit or maintenance assessment.

* **Primary Actor:** Property Officer; Applicant

* **Secondary Actor(s):** Property Lease Management System

* **Pre-condition(s):** Must be successfully logged into the system. The property/unit record exists and the inspection context is known.

* **Trigger:** A new lease, planned inspection cycle or inspection request requires an appointment.

* **Integration:** `IR13` (Microsoft Outlook)

* **Business Requirements:** `FR6.1`

* **Business Rules:** `BR07`



### Main Success Scenario: Pre-Occupancy Inspection Scheduling (Property Officer)

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Real Estate Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Schedule Unit Inspection | Step 4 | Opens Inspection Type Screen with options Pre-Occupation and Exit Inspection |

| Step 5 | Select Pre-Occupation Inspection | Step 6 | Accepts user input and displays list of vacant units matched with Applications |

| Step 7 | Select application by clicking on Schedule Inspection Slots under action | Step 8 | Application Details presented |

| Step 9 | Selects Day using Calendar Picker > Select Time by picking the hours > Click Schedule > Click Submit. (Process repeated for all units) | Step 10 | Updates the schedule. Send notification to Applicant. Lease status = `Awaiting Inspection` |



### Main Success Scenario 2: Applicant/Tenant Confirms Appointment

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Select Inbox | Step 2 | Displays items in Inbox |

| Step 3 | Select item with Lease Status = Awaiting Inspection | Step 4 | Accepts user selection and displays schedule details |

| Step 5 | Click on slot of choice | Step 6 | Accepts user action |

| Step 7 | Click Submit | Step 8 | Presents pop-up message “Are you sure...” |

| Step 9 | Click on “Yes, I am sure” | Step 10 | Success message displays |



### Alternative Flows

**Alternate Scenario (Alternative at Step 5 - Propose New Slot)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 5 | Click on Propose New Slot | Step 6 | Displays Calendar and Time Pickers (restricted to working hours) |

| Step 7 | Select Date and Time slots | Step 8 | Accepts user selection |

| Step 9 | Click on Submit | Step 10 | Presents pop-up message “Are you sure...” |

| Step 11 | Click on “Yes, I am sure” | Step 12 | Success message displays. Notification sent to Property Officer. Lease Status = `Awaiting Inspection` |



*(Note: The Property Officer will need to update the diary as per the proposed inspection appointment)*



---



## UC 14: Conduct Unit Inspection

* **Use Case Number:** UC-14

* **Description:** To perform the scheduled inspection and capture the physical condition of the unit.

* **Primary Actor:** Property Officer

* **Secondary Actor(s):** Property Lease Management System, Tenant, Maintenance Manager

* **Pre-condition(s):** An inspection has been scheduled and access to the checklist is available.

* **Trigger:** The inspection date/time occurs.

* **Integration:** `IR13` (Microsoft Outlook)

* **Business Requirements:** `FR6.2`, `FR6.3`

* **Business Rules:** `BR07`



### Main Success Scenario 1: Conduct Unit Inspection

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Real Estate Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Conduct Unit Inspection | Step 4 | Displays list of leases with status = `Awaiting Inspection` |

| Step 5 | Select Lease of interest > Click on Conduct Inspection under action | Step 6 | Displays details and unit inspection page |

| Step 7 | Download the Inspection Form | Step 8 | Documents successfully downloaded |

| Step 9 | Capture Structural Condition Observations (Plumbing, Electrical, Fixtures, Sanitation, Hazards, Wear and tear) | Step 10 | Displays information |

| Step 11 | Capture Inspection Comments | Step 12 | Displays information |

| Step 13 | Upload the completed Inspection Form and other Supporting documents (e.g. photos) | Step 14 | Documents successfully uploaded |

| Step 15 | Mark the inspection as Complete | Step 16 | Accepts user action |

| Step 17 | Click on Save | Step 18 | Presents notification “Are you sure...” |

| Step 19 | Click on “Yes, I am sure” | Step 20 | Success message displayed. Pop-up closes. Lease Status = `Awaiting Lease/User Agreement Conclusion` |



### Works Order Section (Logged during inspection)

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 21 | Capture/Confirm: Unit Number, Tasks, Issue Description, Priority, Due Date, Required materials, Safety instructions. Click the Add button. | Step 22 | Displays Information |

| Step 23 | Click on Generate Works Order | Step 24 | System generates a Work Order Number (WO: YYYY/ ####). Works Order status = `Logged` |



### Alternative Flows

**Alternative Flow (Alternative at Step 9 - No Access)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 9 | No access to the unit: the inspection is marked unsuccessful and rescheduled | Step 14 | Accepts user action |



---



## UC 15: Authorise and Assign Works Order Request

* **Use Case Number:** UC-15

* **Description:** The process to formalise maintenance work into executable work orders and assign them to appropriate technician route.

* **Primary Actor:** Property and Facilities Manager

* **Secondary Actor(s):** Property Lease Management System, DH

* **Pre-condition(s):** A maintenance request/task exists and technician/team resource information is available. User logged in.

* **Trigger:** Inspection is conducted and findings are captured.

* **Business Requirements:** `FR14.1`, `FR14.2`

* **Business Rules:** `BR04`, `BR09` (Vendors must be approved and active in SCM systems before assignment)



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Facilities Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Authorise and Assign Works Order Request | Step 4 | List of Work Orders with status = `Logged` display |

| Step 3 | Select Work Order Number of interest | Step 4 | Application details from the Inspection stage display, with details of work order request and supporting documents |

| Step 5 | Reviews the details and Click on Authorise > Click Approve > Capture Reason (Mandatory) | Step 6 | Success message display. Approval Date defaults on current system date |

| Step 7 | Capture Signature on the Signature box > Click Next | Step 8 | Message presented “Are you sure...” |

| Step 9 | Click on the “Yes, I am sure” button | Step 10 | Closes pop-up and displays Works Order Assignment Screen |



### Works Order Assignment Screen

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 11 | Click on Assign | Step 12 | Accepts user action and displays options |

| Step 13 | Select Internal or External from the options | Step 14 | Accepts user selection |

| Step 15 | Click Submit | Step 16 | System displays success message. Notification sent to Property Officer & Maintenance Manager. Works Order status = `Assigned` |



### Alternative Flows

**Alternate Flow 1 (Alternative at Step 5 - Reject)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 5 | Click on Authorize > Select Rejected > Capture Reason (Mandatory) | Step 6 | Authorisation action, comments accepted |

| Step 7 | Click Submit | Step 8 | Message presented “Are you sure...” |

| Step 9 | Click on the “Yes, I am sure” button | Step 10 | Notify Maintenance Manager and Property Officer of the work order request based on the captured Reason. Works Order status = `Logged` |



---



## UC 16: Create Maintenance Schedule

* **Use Case Number:** UC-16

* **Description:** To establish planned maintenance activities and track them through completion.

* **Primary Actor:** Maintenance Manager

* **Secondary Actor(s):** Property Lease Management System

* **Pre-condition(s):** Property/unit inventory and maintenance rules/checklists exist. User logged in.

* **Trigger:** A preventive maintenance planning cycle begins or the plan requires revision.

* **Integration:** `IR13` (Microsoft Outlook)

* **Business Requirements:** `FR6.6`



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Create Maintenance Schedule from the main menu | Step 2 | Displays units/assets that can be maintained or are due for maintenance |

| Step 5 | Defines/Confirms: Maintenance activity, Frequency, Due Dates, Estimated Duration, Dependencies | Step 6 | Accepts user input |

| Step 7 | Click on Create Maintenance Schedule | Step 8 | Creates Maintenance Schedule based on entries. Maintenance statuses applicable for tracking: `Planned`, `Due`, `In Progress`, `Completed`, `Overdue` |



---



## UC 17: Log and Track Ad Hoc Maintenance Requests

* **Use Case Number:** UC-17

* **Description:** To capture unplanned maintenance issues reported by tenants or staff and manage them to closure.

* **Primary Actor:** Property Officer; Tenant

* **Secondary Actor(s):** Property Lease Management System; Maintenance Manager; Technician; Property & Facilities Manager

* **Pre-condition(s):** A tenant/unit/property exists and a reporting channel is available. User logged in.

* **Trigger:** An unscheduled maintenance issue is reported.

* **Integration:** `IR13` (Microsoft Outlook)

* **Business Requirements:** `FR6.4`, `FR6.5` | **Business Rules:** `BR09`



### Main Success Scenario 1: Requestor (Property Officer, Tenant) logs issue

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Facilities Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Log and Track Ad Hoc Maintenance Requests | Step 4 | Displays Maintenance Request screen |

| Step 5 | Capture: Unit Number/Location, Issue Details, Date/Time Reported, Select Category, Select Urgency | Step 6 | Accepts user action |

| Step 7 | Browse and Upload Supporting Documents | Step 8 | |

| Step 9 | Click on Submit | Step 10 | Message presented “Are you sure...” |

| Step 11 | Click on the “Yes, I am sure” button | Step 12 | Success message displays. System generates Works Order Number (WO: YYYY/####). Notify Maintenance Manager and Requestor. Works Order status = `Logged` |



### Main Success Scenario 2: Property & Facilities Manager Review and Authorisation

*(See UC-15 Steps 1-16. Work Order moves from `Logged` to `Assigned`)*



### Main Success Scenario 3: Maintenance Manager Assigns and Tracks Internal Works Order

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Facilities Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects Log and Track Ad Hoc Maintenance Requests | Step 4 | List of Work Orders with status = `Assigned` display |

| Step 3 | Click on Assignment > Select Internally Assigned | Step 4 | Accepts user selection and displays work orders assigned internally |

| Step 5 | Select Works Order Number of interest | Step 6 | Displays details |

| Step 7 | Reviews and Validate details > Click on Assign Technician | Step 8 | Accepts user action displays list of Technicians |

| Step 9 | Select Technician (allow for multiple technician selection) | Step 10 | Accepts user selection |

| Step 11 | Click on Assign | Step 12 | Message presented “Are you sure...” |

| Step 13 | Click on the “Yes, I am sure” button | Step 14 | Notify Technician and Requestor of the work order assignment. Works Order status = `In Progress` |



**Alternate Flow 1 (Alternative at Step 3 - External):**

* **Step 3:** Click on Assignment > Select Externally Assigned.

* **System:** Accepts selection. User follows the current contractor procurement processes outside of the system.



**Alternate Flow 2 (Alternative at Step 9 - No Techs):**

* **Step 9:** No Technicians available. User follows the current contractor procurement processes outside of the system.



### Main Success Scenario 4: Technician Acknowledges Assignment

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Select Maintenance Work Orders > Select Allocation Requests | Step 2 | Displays Works Orders with status = `In Progress` |

| Step 3 | Select Works Order of interest | Step 4 | Accepts user selection and displays details |

| Step 5 | Click on the Allocation Request Decision > Select the Accept option | Step 6 | Accepts user selection |

| Step 7 | Click on the Submit button | Step 8 | Message presented “Are you sure...” |

| Step 9 | Click on the “Yes, I am sure” button | Step 10 | Notify Technician and Requestor. Work Order moved to Accepted Work Orders work queue. Works Order status = `In Progress, Pending Resolution` |



**Alternate Flow 1 (Alternative at Step 5 - Technician Rejects):**

* **Step 5-9:** Select the Reject option > Capture Rejection Reason (Mandatory) > Submit > Confirm.

* **System (Step 10-12):** Assignment is rejected. Notification sent to Maintenance Manager. Request sent to Maintenance Manager’s Allocation Requests queue. Works Order status remains = `In Progress`.



### Main Success Scenario 5: Technician Updates Works Order

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Select Maintenance Work Orders > Select Accepted Work Orders | Step 2 | Displays Works Orders with status = `In Progress, Pending Resolution` or `On Hold` |

| Step 3 | Select the Work Order of interest > Select the Action icon | Step 4 | Displays works order details |

| Step 5 | In the Task Section, Capture: Start Date, End Date, Activities, Task Comments. Select Task Complete | Step 6 | Accepts user action |

| Step 7 | Browse and Upload Supporting Documents (Job Sheet) | Step 8 | Accepts user action |

| Step 9 | Click on the Submit button | Step 10 | Message presented “Are you sure...” |

| Step 11 | Click on the “Yes, I am sure” button | Step 12 | Success message displayed. Notification sent to Maintenance Manager, Property Officer and Tenant. Works Order status = `Completed` |



**Alternate Flow 1 (Alternative at Step 5 - On Hold):**

* **Step 5-9:** Select Task On Hold > Capture Reason (Mandatory) > Submit > Confirm.

* **System (Step 10-12):** Works Order status = `On Hold` (Remains in Accepted Work Orders queue). Notification sent with Reason.



### Main Success Scenario 6: Maintenance Manager Closes Works Order

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Select Maintenance Work Orders > Selects Work Orders Pending Closure | Step 2 | List of Work Orders with status = `Completed` |

| Step 3 | Select Work Order of interest | Step 4 | Displays details |

| Step 5 | Review details > Click on Authorise > Select Approve | Step 6 | Accepts user selection |

| Step 7 | Capture Comments | Step 8 | Displays information |

| Step 9 | Capture Signature on the Signature box > Click Submit | Step 10 | Message presented “Are you sure...” |

| Step 11 | Click on the “Yes, I am sure” button | Step 12 | Success message displayed. Notification sent to Property Officer and Tenant. Works Order status = `Closed` |



**Alternate Flow 1 (Alternative at Step 5 - Reject Closure):**

* **Step 5-9:** Review details > Authorise > Select Reject > Capture Reason > Submit > Confirm.

* **System (Step 10):** Notification sent to Property Officer and Tenant. Works Order status = `In Progress, Pending Resolution`.



---



## UC 18: Request Permission to Occupy

* **Use Case Number:** UC-18

* **Description:** The process to enable the applicant or tenant to formally request early occupation of a retail space pending the finalisation of the lease agreement.

* **Primary Actor:** Applicant/Tenant

* **Secondary Actor(s):** Property Lease Management System, Property Officer

* **Pre-condition(s):** Lease application conditionally or committee-approved, Vetting and compliance checks completed, Unit allocated in PLM System, User logged in.

* **Trigger:** Tenant requires early access to premises.

* **Integration:** `IR01`

* **Business Requirements:** `FR4.13`

* **Business Rules:** `BR05` - Permission to Occupy is valid for a maximum of 12 months.



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Clicks on Applications on the Navigation panel | Step 2 | Lists available options on the dropdown list |

| Step 3 | Click on Lease Agreements | Step 4 | Displays list of conditionally or committee approved and unit allocated applications |

| Step 5 | Selects the relevant unit and Clicks Request to PTO (Permission to Occupy) | Step 6 | Opens PTO request form pre-populated with application details |

| Step 7 | Capture PTO Details: Purpose of Occupation, Requested Start Date, Required End Date | Step 8 | Accepts user input |

| Step 9 | Scroll down and Confirm Acceptance of Indemnity Terms and Conditions | Step 10 | Accepts user action |

| Step 11 | Upload Supporting Documents (Mandatory): Certificate of Insurance, Fit-out plans, Health & Safety Clearance | Step 12 | Accepts user action and stores documents against PTO request record |

| Step 13 | Click on Submit PTO | Step 14 | Message presented “Are you sure...” |

| Step 15 | Click on “Yes, I am sure” | Step 16 | System auto-generates PTO Reference Number. Send notification and route application to Property Officer. Application status = `Awaiting PTO Review` |



### Alternative Flows

* **Alternate Flow 1 (Alternative at Step 3 - No Apps):** No eligible applications listed. ‘Request PTO’ button is disabled with tooltip "Please note that you do not have applications with eligible status".

* **Alternate Flow 2 (Alternative at Step 7 - Invalid Dates):** System displays validation error if end date precedes start date or start date falls in the past.

* **Alternate Flow 3 (Alternative at Step 7 - Exceed Duration):** System displays an inline error message: "The requested PTO period cannot exceed the maximum allowable period of 12 months." Request not submitted.

* **Alternate Flow 4 (Alternative at Step 11 - Missing Docs):** System flags incomplete submission and prevents submission until all mandatory documents are uploaded.



---



## UC 19: Review Permission to Occupy

* **Use Case Number:** UC-19

* **Description:** The process to assesses the PTO request against risk, compliance and operational readiness criteria.

* **Primary Actor:** Property Officer

* **Secondary Actor(s):** Property Lease Management System, Tenant, Divisional Head, Head of Department

* **Pre-condition(s):** PTO request submitted successfully, PTO record in ‘Awaiting PTO Review’ status.

* **Trigger:** Property Officer receives notification of a new PTO request.

* **Integration:** `IR01`

* **Business Requirements:** `FR4.8`, `FR4.13`

* **Business Rules:** `BR04`, `BR05`



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Real Estate Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects PTO Reviews | Step 4 | Displays list of PTO requests in `Awaiting PTO Review` status, sorted by submission date |

| Step 5 | Selects PTO of interest | Step 6 | Accepts user selection and displays details |

| Step 5 | Review details, lease status and submitted documents | Step 6 | Displays full application history, vetting outcomes and uploaded documents |

| Step 7 | Validate completeness of supporting documents | Step 8 | System flags any missing mandatory documents |

| Step 9 | Click on Review Outcome > Select Recommended | Step 10 | Accepts user selection |

| Step 11 | Capture Reason | Step 12 | Displays information |

| Step 13 | Click on Submit PTO for Approval | Step 14 | Message presented “Are you sure...” |

| Step 15 | Click on “Yes, I am sure” | Step 16 | Success message displayed. Send notification to DH and HoD. Application status = `Awaiting Approval` |



### Alternative Flows

**Alternate Flow 1 (Alternative at Step 11 - Not Recommended)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 11 | Click on Review Outcome > Select Not Recommended > Capture Reason (Mandatory) | Step 12 | Incomplete documents: Property Officer requests additional information from Tenant |

| Step 15 | Click on Submit | Step 16 | Message presented “Are you sure...” |

| Step 15 | Click on “Yes, I am sure” | Step 16 | Success message displayed. Send notification to Tenant with captured Reason. Application status = `Awaiting PTO Review, Additional Information Required` |



---



## UC 20: Authorise Permission to Occupy

* **Use Case Number:** UC-20

* **Description:** The process to provide formal, documented approval or rejection of the PTO request.

* **Primary Actor:** Divisional Head, HoD

* **Secondary Actor(s):** Property Lease Management System, Tenant, Property Officer

* **Pre-condition(s):** Review completed, PTO in 'Awaiting Approval' status.

* **Trigger:** Delegated Authority receives notification of PTO recommendation pending decision.

* **Integration:** `IR01`

* **Business Requirements:** `FR4.8`, `FR4.13`

* **Business Rules:** `BR04`



### Main Success Scenario

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 1 | Selects Real Estate Queue from the main menu | Step 2 | Lists all available options on the dropdown list |

| Step 3 | Selects PTO Authorisations | Step 4 | Displays list of applications in Worklist (Statuses = Valuation Requested; Evaluation Committee Outcome decisions; PTO status = `Awaiting Approval`) |

| Step 5 | Selects PTO of interest with status = `Awaiting Approval` | Step 6 | Accepts user selection and displays details |

| Step 5 | Review details, reviews and submitted documents | Step 6 | Displays presentation of PTO record and uploaded documents |

| Step 7 | Select Approve from the PTO Decision from the dropdown list: Approve | Step 8 | System accepts user action |

| Step 9 | Capture signature on the Signature box > Click on Submit | Step 10 | Message presented “Are you sure...” |

| Step 11 | Click on “Yes, I am sure” | Step 12 | Success message displayed. Send notification to Property Officer, Tenant. Application status = `Approved`. Triggers PTO Certificate generation workflow |



### Alternative Flows

**Alternate Flow 1 (Alternative at Step 7 - Approve with Conditions or Reject)**

| Ref | Actor Action | Ref | System Response |

|---|---|---|---|

| Step 7 | Select: Approve with Conditions or Rejected | Step 8 | Accepts user selection and activates text field |

| Step 9 | Capture Reason (Mandatory) | Step 10 | Displays information |

| Step 11 | Capture signature on the Signature box > Click on Submit | Step 12 | Message presented “Are you sure...” |

| Step 13 | Click on “Yes, I am sure” | Step 14 | Success message displayed. Send notification to Property Officer and Tenant with captured Reason. Application status = Decision captured in Step 7 |

