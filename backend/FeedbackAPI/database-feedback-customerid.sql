USE AIFeedbackDB;
GO

/*
    Run this once before using the updated API.
    Feedback now identifies the user only by Feedback.CustomerId.
    Customer display values are read by joining Feedback to Customers.
*/

/* 1. Make sure the foreign-key column exists. */
IF COL_LENGTH('dbo.Feedback', 'CustomerId') IS NULL
BEGIN
    ALTER TABLE dbo.Feedback ADD CustomerId INT NULL;
END
GO

/* 2. Backfill existing feedback rows from the old duplicated email/name fields. */
IF COL_LENGTH('dbo.Feedback', 'CustomerEmail') IS NOT NULL
BEGIN
    UPDATE f
    SET f.CustomerId = c.CustomerId
    FROM dbo.Feedback f
    INNER JOIN dbo.Customers c
        ON c.Email = f.CustomerEmail
    WHERE f.CustomerId IS NULL;
END
GO

/* 3. Check rows that still cannot be mapped before making CustomerId mandatory. */
SELECT f.FeedbackId, f.CustomerId, f.FeedbackText
FROM dbo.Feedback f
WHERE f.CustomerId IS NULL;
GO

/*
    If the previous SELECT returns rows, either:
    - create matching rows in Customers and rerun step 2, or
    - delete/move those orphan feedback rows.
    Then run the remaining statements.
*/

/* 4. Enforce CustomerId and add the foreign key. */
ALTER TABLE dbo.Feedback ALTER COLUMN CustomerId INT NOT NULL;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Feedback_Customers_CustomerId'
)
BEGIN
    ALTER TABLE dbo.Feedback
    ADD CONSTRAINT FK_Feedback_Customers_CustomerId
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId);
END
GO

/* 5. Stop storing duplicated customer details in Feedback. */
IF COL_LENGTH('dbo.Feedback', 'CustomerName') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Feedback DROP COLUMN CustomerName;
END
GO

IF COL_LENGTH('dbo.Feedback', 'CustomerEmail') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Feedback DROP COLUMN CustomerEmail;
END
GO

/* 6. Stored procedure for the admin feedback page. */
CREATE OR ALTER PROCEDURE dbo.sp_GetFeedbackWithCustomer
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        f.FeedbackId,
        f.CustomerId,
        c.FullName AS CustomerName,
        c.Email AS CustomerEmail,
        f.FeedbackText,
        f.Summary,
        f.Sentiment,
        f.IssueCategory,
        f.RecommendedAction,
        f.SubmittedAt
    FROM dbo.Feedback f
    INNER JOIN dbo.Customers c
        ON c.CustomerId = f.CustomerId
    ORDER BY f.FeedbackId DESC;
END
GO

/* 7. Stored procedure for customer feedback submission. */
CREATE OR ALTER PROCEDURE dbo.sp_SubmitFeedback
    @CustomerId INT,
    @FeedbackText NVARCHAR(MAX),
    @Summary NVARCHAR(MAX),
    @Sentiment NVARCHAR(50),
    @IssueCategory NVARCHAR(100),
    @RecommendedAction NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Customers
        WHERE CustomerId = @CustomerId
    )
    BEGIN
        THROW 50001, 'CustomerId does not exist.', 1;
    END

    INSERT INTO dbo.Feedback
    (
        CustomerId,
        FeedbackText,
        Summary,
        Sentiment,
        IssueCategory,
        RecommendedAction
    )
    OUTPUT INSERTED.FeedbackId
    VALUES
    (
        @CustomerId,
        @FeedbackText,
        @Summary,
        @Sentiment,
        @IssueCategory,
        @RecommendedAction
    );
END
GO
