SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_NaverWorksApprovalRaw_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_NaverWorksApprovalRaw_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_NaverWorksApprovalRaw_Upsert]
(
    @ApprovalDocumentId BIGINT,
    @DocumentFormId NVARCHAR(100) = NULL,
    @DocumentNumber NVARCHAR(100) = NULL,
    @UserId NVARCHAR(100) = NULL,
    @UserName NVARCHAR(200) = NULL,
    @OrgUnitId NVARCHAR(100) = NULL,
    @OrgUnitName NVARCHAR(200) = NULL,
    @Title NVARCHAR(500) = NULL,
    @Status NVARCHAR(50) = NULL,
    @CreatedTime DATETIMEOFFSET = NULL,
    @CompletedTime DATETIMEOFFSET = NULL,
    @RawJson NVARCHAR(MAX) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM [nhvpa3en_vpa01].[CDV_NaverWorksApprovalRaw]
        WHERE ApprovalDocumentId = @ApprovalDocumentId
    )
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_NaverWorksApprovalRaw]
        SET
            DocumentFormId = @DocumentFormId,
            DocumentNumber = @DocumentNumber,
            UserId = @UserId,
            UserName = @UserName,
            OrgUnitId = @OrgUnitId,
            OrgUnitName = @OrgUnitName,
            Title = @Title,
            Status = @Status,
            CreatedTime = @CreatedTime,
            CompletedTime = @CompletedTime,
            RawJson = @RawJson,
            LastSyncTime = GETDATE()
        WHERE ApprovalDocumentId = @ApprovalDocumentId;
    END
    ELSE
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_NaverWorksApprovalRaw]
        (
            ApprovalDocumentId,
            DocumentFormId,
            DocumentNumber,
            UserId,
            UserName,
            OrgUnitId,
            OrgUnitName,
            Title,
            Status,
            CreatedTime,
            CompletedTime,
            RawJson,
            LastSyncTime
        )
        VALUES
        (
            @ApprovalDocumentId,
            @DocumentFormId,
            @DocumentNumber,
            @UserId,
            @UserName,
            @OrgUnitId,
            @OrgUnitName,
            @Title,
            @Status,
            @CreatedTime,
            @CompletedTime,
            @RawJson,
            GETDATE()
        );
    END
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_EmployeeLeave_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_EmployeeLeave_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_EmployeeLeave_Upsert]
(
    @ApprovalDocumentId BIGINT,
    @DocumentNumber NVARCHAR(100) = NULL,
    @DocumentFormId NVARCHAR(100) = NULL,
    @NaverUserId NVARCHAR(100) = NULL,
    @EmployeeId NVARCHAR(50) = NULL,
    @EmployeeName NVARCHAR(200) = NULL,
    @OrgUnitId NVARCHAR(100) = NULL,
    @DepartmentName NVARCHAR(200) = NULL,
    @LeaveType NVARCHAR(100) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @LeaveDays DECIMAL(18,2) = NULL,
    @TimeUsed NVARCHAR(100) = NULL,
    @DeductHours DECIMAL(18,2) = NULL,
    @Reason NVARCHAR(1000) = NULL,
    @ApprovalStatus NVARCHAR(50) = NULL,
    @CreatedTime DATETIMEOFFSET = NULL,
    @CompletedTime DATETIMEOFFSET = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM [nhvpa3en_vpa01].[CDV_EmployeeLeave]
        WHERE ApprovalDocumentId = @ApprovalDocumentId
    )
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_EmployeeLeave]
        SET
            DocumentNumber = @DocumentNumber,
            DocumentFormId = @DocumentFormId,
            NaverUserId = @NaverUserId,
            EmployeeId = @EmployeeId,
            EmployeeName = @EmployeeName,
            OrgUnitId = @OrgUnitId,
            DepartmentName = @DepartmentName,
            LeaveType = @LeaveType,
            FromDate = @FromDate,
            ToDate = @ToDate,
            LeaveDays = @LeaveDays,
            TimeUsed = @TimeUsed,
            DeductHours = @DeductHours,
            Reason = @Reason,
            ApprovalStatus = @ApprovalStatus,
            CreatedTime = @CreatedTime,
            CompletedTime = @CompletedTime,
            LastSyncTime = GETDATE()
        WHERE ApprovalDocumentId = @ApprovalDocumentId;
    END
    ELSE
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_EmployeeLeave]
        (
            ApprovalDocumentId,
            DocumentNumber,
            DocumentFormId,
            NaverUserId,
            EmployeeId,
            EmployeeName,
            OrgUnitId,
            DepartmentName,
            LeaveType,
            FromDate,
            ToDate,
            LeaveDays,
            TimeUsed,
            DeductHours,
            Reason,
            ApprovalStatus,
            CreatedTime,
            CompletedTime,
            LastSyncTime
        )
        VALUES
        (
            @ApprovalDocumentId,
            @DocumentNumber,
            @DocumentFormId,
            @NaverUserId,
            @EmployeeId,
            @EmployeeName,
            @OrgUnitId,
            @DepartmentName,
            @LeaveType,
            @FromDate,
            @ToDate,
            @LeaveDays,
            @TimeUsed,
            @DeductHours,
            @Reason,
            @ApprovalStatus,
            @CreatedTime,
            @CompletedTime,
            GETDATE()
        );
    END
END
GO
