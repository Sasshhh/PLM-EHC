USE [Siyakhokha]
GO
/****** Object:  StoredProcedure [dbo].[pr_SCRIPT_AcbDataScrubPrep]    Script Date: 2019/10/28 20:22:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jayan Kistasami
-- Create date: 2019-12-21
-- Description:	Structures data for ACB file generation.
-- =============================================
ALTER PROCEDURE [dbo].[pr_SCRIPT_AcbDataScrubPrep] 
	-- Add the parameters for the stored procedure here
	@StrikeDay INT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DECLARE @TranCount INT;
    SET @TranCount = @@trancount;
    BEGIN TRY
        IF (@trancount = 0)
            BEGIN TRANSACTION;
        ELSE
            SAVE TRANSACTION pr_SCRIPT_AcbDataScrubPrep;			

			-- Logical script starts here.
			DECLARE @Year INT;				-- Year to work with.
			DECLARE @Month INT;				-- Month to work with.
			DECLARE @DupId INT;				-- Duplicate status id.
			DECLARE @PenId INT;				-- Pending status id.
			DECLARE @Stamp DATETIME;		-- Modified stamp for duplicates.
			DECLARE @BatchStamp DATETIME;	-- Modified stamp for individual batches.
			DECLARE @BatchNumber BIGINT;	-- Generated batch no.

			SET @Month = (SELECT MONTH(GETDATE()));
			SET @Year = (SELECT YEAR(GETDATE()));
			SET @Stamp = GETDATE();
			SET @BatchStamp = GETDATE();
			SET @PenId = (SELECT Id FROM [Status] WHERE [Key] LIKE 's_debit_order_pending')

			-- Creates a new debit order status called duplicate.
			IF ((SELECT COUNT(*) FROM [Status] WHERE [Key] LIKE 's_debit_order_duplicate') = 0)
			BEGIN
				INSERT INTO [Status] (
					Name, [Description], [Key], StatusTypeId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId,
					CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime) 
				VALUES (
					'Duplicate', 'Duplicate orders.', 's_debit_order_duplicate', 
					(SELECT TOP 1 Id FROM StatusTypes WHERE [Key] LIKE 'st_debit_order'),
					1, 0, 0, 1, GETDATE(), 1, GETDATE() );
			END;

			SET @DupId = (SELECT TOP 1 Id FROM [Status] WHERE [Key] LIKE 's_debit_order_duplicate');

			-- Flags all duplicates (and soft deletes) for non-recurring per strike day, month and year.
			-- Check the debit order status duplicates also.
			WITH cte AS (
				SELECT 
					[BankAccountId] ,[AccountId] ,[Amount] ,[StartDateTime]
					,[IsRecurring] ,[StatusId] ,[ModifiedBySystemUserId]
					,[ModifiedDateTime] ,[IsBatch] ,[IsDeleted]
					,ROW_NUMBER() OVER (
						PARTITION BY 
							[BankAccountId] ,[AccountId] ,[Amount]
							,[StartDateTime] ,[IsRecurring] ,[StatusId] ,[IsBatch]
						ORDER BY 
							[BankAccountId] ,[AccountId] ,[Amount] 
							,[StartDateTime] ,[IsRecurring] ,[StatusId] ,[IsBatch]
					) RowNumber
				FROM 
					DebitOrders do
				WHERE
					do.IsActive = 1
					AND do.IsDeleted = 0
					AND do.IsRecurring = 0
					AND do.StrikeDay = @StrikeDay
					AND MONTH(do.StartDateTime) = @Month
					AND YEAR(do.StartDateTime) = @Year
			)
			UPDATE cte SET StatusId = @DupId, IsDeleted = 1, ModifiedBySystemUserId = 1, ModifiedDateTime = @Stamp WHERE RowNumber > 1;

			-- Check sequence number turnover.
			IF ((
				SELECT CAST(ModifiedDateTime AS DATE) 
				FROM AppSettings 
				WHERE [Key] LIKE 'debit_order_daily_sequence_counter') 
					< CAST(GETDATE() AS DATE))
			BEGIN
				UPDATE AppSettings SET 
					Value = '0001', ModifiedDateTime = GETDATE() 
				WHERE [Key] LIKE 'debit_order_daily_sequence_counter';
			END
			ELSE
			BEGIN
				UPDATE AppSettings SET 
					Value = RIGHT('0000' + CAST((CAST(Value AS INT) + 1) AS VARCHAR(4)), 4), ModifiedDateTime = GETDATE() 
				WHERE [Key] LIKE 'debit_order_daily_sequence_counter';
			END;

			-- Build batch number.
			SELECT @BatchNumber = 
				CAST(FORMAT(GETDATE(), 'yyyyMMdd') + 
					(SELECT Value FROM AppSettings WHERE [Key] LIKE 'debit_order_daily_sequence_counter') + '0' +
						RIGHT('00' + CAST((CAST(@Strikeday AS INT)) AS VARCHAR(2)), 2) AS BIGINT)
			PRINT 'Batch No.: ' + CAST(@BatchNumber AS VARCHAR);

			-- Create a debit order status for those that don't have one.
			INSERT INTO DebitOrderStatus (
				DebitOrderId, StatusId, BatchNumber, BatchReference, IsActive, IsDeleted, CreatedBySystemUserId,
				CreatedDateTime, ModifiedBySystemUserId, ModifiedDateTime ) (
			SELECT 
				do.Id, @PenId, @BatchNumber, 'Individual Debit Order', 1, 0, 1, GETDATE(), 1, @BatchStamp
			FROM 
				DebitOrders do
				LEFT JOIN DebitOrderStatus dos ON dos.DebitOrderId = do.Id
			WHERE 
				do.IsActive = 1 AND do.IsDeleted = 0 AND 
				((do.StatusId = @PenId AND do.IsRecurring = 0 AND CAST(do.CreatedDateTime AS DATE) = CAST(do.StartDateTime AS DATE)) 
				OR (do.IsRecurring = 1)) AND do.StrikeDay = @StrikeDay AND dos.DebitOrderId IS NULL
				AND CAST(do.StartDateTime AS DATE) <= CAST(GETDATE() AS DATE));

			-- Update the batch number for all individual entries as a single batch.
			UPDATE dos SET 
				dos.BatchNumber = @BatchNumber,
				dos.ModifiedDateTime = @BatchStamp
			FROM 
				DebitOrders do
				INNER JOIN DebitOrderStatus dos ON dos.DebitOrderId = do.Id
			WHERE 
				do.IsActive = 1 AND do.IsDeleted = 0 AND dos.StatusId = @PenId
				AND do.StrikeDay = @StrikeDay AND do.Amount > 0;

	lbexit:
        IF @TranCount = 0
            COMMIT;
    END TRY
    BEGIN CATCH
        DECLARE @Error INT, @Message VARCHAR(4000), @XState INT;
        SELECT @Error = ERROR_NUMBER(), @Message = ERROR_MESSAGE(), @XState = XACT_STATE();
        if @XState = -1
            ROLLBACK;
        if @XState = 1 and @TranCount = 0
            ROLLBACK;
        if @XState = 1 and @TranCount > 0
            ROLLBACK TRANSACTION pr_SCRIPT_AcbDataScrubPrep;

        RAISERROR ('pr_SCRIPT_AcbDataScrubPrep: %d: %s', 16, 1, @Error, @Message) ;
    END CATCH
END
