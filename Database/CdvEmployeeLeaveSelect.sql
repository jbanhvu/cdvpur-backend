SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[CDV_EmployeeLeave_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[CDV_EmployeeLeave_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[CDV_EmployeeLeave_Select]
(
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @ApprovalStatus NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_EmployeeLeave]
    WHERE
        (@FromDate IS NULL OR ToDate >= @FromDate)
        AND (@ToDate IS NULL OR FromDate <= @ToDate)
        AND
        (
            @ApprovalStatus IS NULL
            OR ApprovalStatus = @ApprovalStatus
        )
    ORDER BY FromDate DESC, EmployeeName;
END
GO
