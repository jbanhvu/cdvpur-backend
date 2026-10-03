IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_MoldRepairLog]', 'StageName') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_MoldRepairLog] ADD [StageName] NVARCHAR(100) NULL;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_MoldRepairLog_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairLog_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairLog_Select]
    @Id INT = -1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.Id,
        l.MoldId,
        m.MoldCode,
        m.MoldName,
        l.Issue,
        l.Solution,
        l.Vendor,
        l.RepairPersonIds,
        l.StageName,
        l.StartDate,
        l.EndDate,
        l.Result,
        l.Status,
        l.CreatedAt,
        l.CreatedBy,
        l.UpdatedAt,
        l.UpdatedBy
    FROM [nhvpa3en_vpa01].[CDV_MoldRepairLog] l
    LEFT JOIN [dbo].[CDV_Mold] m ON m.MoldId = l.MoldId
    WHERE (ISNULL(@Id, -1) <= 0 OR l.Id = @Id)
    ORDER BY l.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_MoldRepairLog_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairLog_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairLog_Upsert]
    @Id INT = -1,
    @MoldId INT,
    @Issue NVARCHAR(MAX) = NULL,
    @Solution NVARCHAR(MAX) = NULL,
    @Vendor NVARCHAR(200) = NULL,
    @RepairPersonIds NVARCHAR(500) = NULL,
    @StageName NVARCHAR(100) = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @Result NVARCHAR(MAX) = NULL,
    @Status VARCHAR(30) = NULL,
    @CreatedBy INT = NULL,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_MoldRepairLog]
        (
            MoldId,
            Issue,
            Solution,
            Vendor,
            RepairPersonIds,
            StageName,
            StartDate,
            EndDate,
            Result,
            Status,
            CreatedAt,
            CreatedBy
        )
        VALUES
        (
            @MoldId,
            @Issue,
            @Solution,
            @Vendor,
            @RepairPersonIds,
            @StageName,
            @StartDate,
            @EndDate,
            @Result,
            ISNULL(@Status, 'PENDING'),
            SYSDATETIME(),
            @CreatedBy
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_MoldRepairLog]
        SET
            MoldId = @MoldId,
            Issue = @Issue,
            Solution = @Solution,
            Vendor = @Vendor,
            RepairPersonIds = @RepairPersonIds,
            StageName = @StageName,
            StartDate = @StartDate,
            EndDate = @EndDate,
            Result = @Result,
            Status = ISNULL(@Status, Status),
            UpdatedAt = SYSDATETIME(),
            UpdatedBy = @UpdatedBy
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_MoldRepairLog_Select] @Id = @Id;
END
GO
