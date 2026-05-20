USE AIFeedbackDB;
GO

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