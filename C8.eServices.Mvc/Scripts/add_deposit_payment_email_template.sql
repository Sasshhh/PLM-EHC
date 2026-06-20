-- =============================================
-- Script to add Deposit Payment Email Template
-- Date: 2026-03-14
-- Description: Adds email template for sending banking details after unit acceptance
-- =============================================

-- Check if email template already exists
IF NOT EXISTS (SELECT 1 FROM EmailContentTypes WHERE [Key] = 'plm_unit_accepted_deposit_payment_details')
BEGIN
    INSERT INTO EmailContentTypes 
    (
        [Name], 
        [Description], 
        [Key], 
        [IsActive], 
        [IsDeleted], 
        [IsLocked],
        [CreatedDateTime],
        [ModifiedDateTime],
        [DepartmentId]
    )
    VALUES 
    (
        'Unit Accepted - Deposit Payment Details',
        'You have successfully accepted the unit offer. Please make payment for the deposit using the following banking details: Bank Name: Standard Bank, Account Name: Ekurhuleni Metropolitan Municipality, Account Number: 001844075, Branch Code: 011545, Reference: {0}. After making payment, please login to the system and upload your proof of payment.',
        'plm_unit_accepted_deposit_payment_details',
        1, -- IsActive
        0, -- IsDeleted
        0, -- IsLocked
        GETDATE(), -- CreatedDateTime
        GETDATE(), -- ModifiedDateTime
        NULL -- DepartmentId
    )

    PRINT 'Email template for deposit payment details added successfully.'
END
ELSE
BEGIN
    PRINT 'Email template for deposit payment details already exists.'
END
GO
