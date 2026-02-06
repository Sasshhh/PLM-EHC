-- =============================================
-- TENANT TRAINING AND EXAMINATION SYSTEM
-- SQL INSERT SCRIPT
-- Generated: 2025-01-15
-- =============================================

USE [CRMPLMDEV_2025]
GO

-- =============================================
-- INSERT TRAINING SLIDES (24 SLIDES)
-- =============================================

SET IDENTITY_INSERT [dbo].[TrainingSlides] ON
GO

-- Note: Adjust image paths to match your Content/Training Slides folder
-- Images should be named: slide-01.jpg through slide-24.jpg

INSERT INTO [dbo].[TrainingSlides] (Id, SlideNumber, Title, Content, ImagePath, DisplayOrder, IsActive, IsDeleted, IsLocked, CreatedDateTime, ModifiedDateTime)
VALUES
(1, 1, 'PRE-TENANCY TRAINING PROGRAMME', 'Welcome to the Pre-Tenancy Training Programme - 5 JULY 2025', '/Content/Training Slides/slide-01.jpg', 1, 1, 0, 0, GETDATE(), GETDATE()),
(2, 2, 'THE SOCIAL HOUSING PROGRAMME', 'Overview of the Social Housing Programme', '/Content/Training Slides/slide-02.jpg', 2, 1, 0, 0, GETDATE(), GETDATE()),
(3, 3, 'WHAT IS SOCIAL HOUSING?', 'Urban rental housing programme with focus on integration, inclusivity and restructuring of urban fabric. Densified rental housing (3-4 storey walk ups or high-rise buildings). Rental in perpetuity - NO OWNERSHIP. EHC owns the units.', '/Content/Training Slides/slide-03.jpg', 3, 1, 0, 0, GETDATE(), GETDATE()),
(4, 4, 'THE SOCIAL HOUSING MODEL', 'Spatial integration, Social housing grant, Financial model, Products and services', '/Content/Training Slides/slide-04.jpg', 4, 1, 0, 0, GETDATE(), GETDATE()),
(5, 5, 'SOCIAL HOUSING QUALIFYING CRITERIA', 'Income: R1,850 to R22,000 per month. Citizenship: South African. Property ownership: Must not own or have owned property. Household types: Single with financial dependents, nuclear families. Affordability: Must have sufficient net disposable income.', '/Content/Training Slides/slide-05.jpg', 5, 1, 0, 0, GETDATE(), GETDATE()),
(6, 6, 'WHAT IS A SOCIAL HOUSING ASSOCIATION', 'EHC is a Social Housing institution providing rental accommodation to households earning R1,850-R22,000. Regulated by Social Housing Regulatory Authority. Owns and manages SH stock and related tenancies.', '/Content/Training Slides/slide-06.jpg', 6, 1, 0, 0, GETDATE(), GETDATE()),
(7, 7, 'ROLES AND RESPONSIBILITIES', 'Overview of EHC and tenant roles: Social housing, Social rights & responsibilities, Financial rights & responsibilities, Repairs & maintenance, Tenant participation', '/Content/Training Slides/slide-07.jpg', 7, 1, 0, 0, GETDATE(), GETDATE()),
(8, 8, 'SOCIAL RIGHTS AND RESPONSIBILITIES', 'Understanding nuisance, responsibilities, and problem-solving', '/Content/Training Slides/slide-08.jpg', 8, 1, 0, 0, GETDATE(), GETDATE()),
(9, 9, 'USE OF THE UNIT AND SUBLETTING', 'Units designed for certain number of people. Must personally occupy units. Residential purposes only - no business activities. Exceptional subletting circumstances only with EHC approval.', '/Content/Training Slides/slide-09.jpg', 9, 1, 0, 0, GETDATE(), GETDATE()),
(10, 10, 'FINANCIAL RIGHTS AND RESPONSIBILITIES', 'EHC and tenant financial responsibilities overview', '/Content/Training Slides/slide-10.jpg', 10, 1, 0, 0, GETDATE(), GETDATE()),
(11, 11, 'FINANCIAL RIGHTS AND RESPONSIBILITIES - EHC', 'Setting rentals and deposits. Sending monthly rent accounts. Collecting rent. Lawful eviction for non-payment.', '/Content/Training Slides/slide-11.jpg', 11, 1, 0, 0, GETDATE(), GETDATE()),
(12, 12, 'FINANCIAL RIGHTS AND RESPONSIBILITIES - TENANT', 'Pay deposit before signing lease. Sign preferred debit order. Pay rental by 1st of each month. Make arrangements for late payment. Cannot withhold rental. Lodge complaints to EHC or rental tribunal.', '/Content/Training Slides/slide-12.jpg', 12, 1, 0, 0, GETDATE(), GETDATE()),
(13, 13, 'RENTAL BREAKDOWN - EXAMPLE', 'Direct property costs: Rates, taxes, insurance, cleaning, gardening, maintenance. Overheads: Office, staff. Facilities: Security, common areas, refuse removal.', '/Content/Training Slides/slide-13.jpg', 13, 1, 0, 0, GETDATE(), GETDATE()),
(14, 14, 'RENTAL PAYMENTS', 'Rentals due on 1st of month. Payment by preferred debit order. Non-payment leads to eviction. No winner in eviction.', '/Content/Training Slides/slide-14.jpg', 14, 1, 0, 0, GETDATE(), GETDATE()),
(15, 15, 'RENT INCREASE - EXAMPLE', 'Increases due 1st of July. Linked to inflation rate. Example calculation provided.', '/Content/Training Slides/slide-15.jpg', 15, 1, 0, 0, GETDATE(), GETDATE()),
(16, 16, 'MAINTENANCE RIGHTS AND RESPONSIBILITIES', 'Maintenance overview', '/Content/Training Slides/slide-16.jpg', 16, 1, 0, 0, GETDATE(), GETDATE()),
(17, 17, 'MAINTENANCE, ALTERATIONS, REPAIRS', 'Shared responsibility: EHC (outside) and tenants (inside). Always contact Complex supervisor first. Leave home as found when moved in.', '/Content/Training Slides/slide-17.jpg', 17, 1, 0, 0, GETDATE(), GETDATE()),
(18, 18, 'INSURANCE', 'EHC insures property including units. Tenants insure household contents. Example: geyser bursts and damages lounge suite.', '/Content/Training Slides/slide-18.jpg', 18, 1, 0, 0, GETDATE(), GETDATE()),
(19, 19, 'TENANT PARTICIPATION & COMMUNITY DEVELOPMENT', 'Overview of tenant participation', '/Content/Training Slides/slide-19.jpg', 19, 1, 0, 0, GETDATE(), GETDATE()),
(20, 20, 'TENANT PARTICIPATION/COMMUNITY DEVELOPMENT', 'What is participation? Why should tenants participate? How can tenants participate?', '/Content/Training Slides/slide-20.jpg', 20, 1, 0, 0, GETDATE(), GETDATE()),
(21, 21, 'SUMMARY SLIDE 1', 'Key points summary', '/Content/Training Slides/slide-21.jpg', 21, 1, 0, 0, GETDATE(), GETDATE()),
(22, 22, 'SUMMARY SLIDE 2', 'Additional key points', '/Content/Training Slides/slide-22.jpg', 22, 1, 0, 0, GETDATE(), GETDATE()),
(23, 23, 'SUMMARY SLIDE 3', 'Final summary points', '/Content/Training Slides/slide-23.jpg', 23, 1, 0, 0, GETDATE(), GETDATE()),
(24, 24, 'CONCLUSION', 'Thank you for completing the training. Please proceed to the examination.', '/Content/Training Slides/slide-24.jpg', 24, 1, 0, 0, GETDATE(), GETDATE())

SET IDENTITY_INSERT [dbo].[TrainingSlides] OFF
GO

-- =============================================
-- INSERT EXAMINATION QUESTIONS (15 QUESTIONS + 1 EXAMPLE)
-- =============================================

SET IDENTITY_INSERT [dbo].[ExaminationQuestions] ON
GO

INSERT INTO [dbo].[ExaminationQuestions] (Id, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, QuestionOrder, IsExampleQuestion, IsActive, IsDeleted, IsLocked, CreatedDateTime, ModifiedDateTime)
VALUES
-- EXAMPLE QUESTION (Pre-circled for demonstration)
(1, 'The sun always rises in:', 'The afternoon', 'The morning', 'The evening', NULL, 'B', 0, 1, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 1
(2, 'Who owns your unit?', 'The tenant', 'EHC', 'The tenant and EHC jointly', NULL, 'B', 1, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 2
(3, 'The social housing programme includes the following tenure options:', 'Rental forever', 'Rent to buy', 'Rent to own', 'Instalment sale', 'A', 2, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 3
(4, 'If noisy neighbours are disturbing you, it is best to:', 'Call the police first', 'Call Complex Supervisor or a staff member at EHC first', 'Discuss the situation with your neighbour', NULL, 'B', 3, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 4
(5, 'You can be evicted because of:', 'Rental arrears', 'Nuisance', 'Both of the above', NULL, 'C', 4, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 5
(6, 'The deposit of 1 month''s rental must be paid:', 'Before you sign the lease agreement', 'After the 1st month of rental', 'EHC does not charge a deposit', NULL, 'A', 5, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 6
(7, 'I pay rental to cover:', 'Insurance of the property, security', 'The maintenance of the buildings', 'Lighting, cleaning, gardening of communal areas', 'All of the above', 'D', 6, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 7
(8, 'The wind blows the roof off and your bed is damaged as a result, who is responsible for replacing your bed?', 'EHC', 'The tenant', 'Both EHC and the tenant', 'None of the above', 'B', 7, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 8
(9, 'When your unit needs a light bulb to be replaced:', 'EHC is responsible for the outside of your home and you are responsible for the inside', 'EHC is responsible for both the inside and outside of your home', 'You are responsible for both the inside and the outside of your home', NULL, 'A', 8, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 9
(10, 'The rental will:', 'Increase every year on the 1st of October', 'Increase every year on the 1st of July', 'Always stay the same', NULL, 'B', 9, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 10
(11, 'The rent must be paid:', 'Before the 15th of each month', 'Before the 1st of each month', 'When it suits you', NULL, 'B', 10, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 11
(12, 'The preferred way in which rent must be paid is:', 'In cash at the EHC reception', 'Via direct payroll deduction', 'By preferred debit order', NULL, 'C', 11, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 12
(13, 'If you want to end the lease agreement:', 'You give EHC 2 months'' notice', 'You give EHC 1 month'' notice', 'You can leave whenever you like', NULL, 'B', 12, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 13
(14, 'When you leave your home and you have met all your obligations:', 'You will lose your deposit', 'Your deposit will be refunded', 'Your deposit will be used for maintenance', NULL, 'B', 13, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 14
(15, 'If you want to do alterations to your home:', 'You ask a friend to do it for you', 'You ask a contractor to do it for you', 'You ask EHC for permission in writing first', NULL, 'C', 14, 0, 1, 0, 0, GETDATE(), GETDATE()),

-- QUESTION 15
(16, 'You cannot afford the rent anymore and you decide to take in a person who will pay you rent:', 'You are allowed to relet/sublet your unit', 'You are required to apply to EHC to sublet', 'You can sublet on condition that the joint income does not exceed R22,000', 'You can sublet if your household does not exceed 4 people', 'B', 15, 0, 1, 0, 0, GETDATE(), GETDATE())

SET IDENTITY_INSERT [dbo].[ExaminationQuestions] OFF
GO

PRINT 'Training slides and examination questions inserted successfully!'
PRINT 'Total Slides: 24'
PRINT 'Total Questions: 15 + 1 Example'
PRINT ''
PRINT 'CORRECT ANSWERS SUMMARY:'
PRINT 'Example: B (The morning)'
PRINT 'Q1: B (EHC)'
PRINT 'Q2: A (Rental forever)'
PRINT 'Q3: B (Call Complex Supervisor)'
PRINT 'Q4: C (Both of the above)'
PRINT 'Q5: A (Before you sign the lease)'
PRINT 'Q6: D (All of the above)'
PRINT 'Q7: B (The tenant)'
PRINT 'Q8: A (EHC outside, tenant inside)'
PRINT 'Q9: B (1st of July)'
PRINT 'Q10: B (1st of each month)'
PRINT 'Q11: C (By preferred debit order)'
PRINT 'Q12: B (1 month notice)'
PRINT 'Q13: B (Deposit refunded)'
PRINT 'Q14: C (Ask EHC permission first)'
PRINT 'Q15: B (Apply to EHC to sublet)'
GO
