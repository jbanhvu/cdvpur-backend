IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_MoldRepairLog_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairLog_Select] AS BEGIN SET NOCOUNT ON; END');
GO
IF OBJECT_ID(N'[nhvpa3en_vpa01].[CK_CDV_MoldRepairLog_Dates]', N'C') IS NOT NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_MoldRepairLog] DROP CONSTRAINT [CK_CDV_MoldRepairLog_Dates];
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_MoldRepairLog]', 'StartDate') IS NOT NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_MoldRepairLog] ALTER COLUMN [StartDate] DATETIME NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_MoldRepairLog]', 'EndDate') IS NOT NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_MoldRepairLog] ALTER COLUMN [EndDate] DATETIME NULL;
GO
IF OBJECT_ID(N'[nhvpa3en_vpa01].[CK_CDV_MoldRepairLog_Dates]', N'C') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_MoldRepairLog]
    ADD CONSTRAINT [CK_CDV_MoldRepairLog_Dates]
    CHECK ([StartDate] IS NULL OR [EndDate] IS NULL OR [EndDate] >= [StartDate]);
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
END;
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
            StartDate = @StartDate,
            EndDate = @EndDate,
            Result = @Result,
            Status = ISNULL(@Status, Status),
            UpdatedAt = SYSDATETIME(),
            UpdatedBy = @UpdatedBy
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_MoldRepairLog_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_MoldRepairLog_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairLog_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairLog_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [nhvpa3en_vpa01].[CDV_MoldRepairImage]
    WHERE MoldRepairLogId = @Id;

    DELETE FROM [nhvpa3en_vpa01].[CDV_MoldRepairLog]
    WHERE Id = @Id;

    SELECT @Id AS Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_MoldRepairImage_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairImage_Select] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairImage_Select]
    @Id INT = -1,
    @MoldRepairLogId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        MoldRepairLogId,
        ImageType,
        ImageUrl,
        CreatedAt
    FROM [nhvpa3en_vpa01].[CDV_MoldRepairImage]
    WHERE (ISNULL(@Id, -1) <= 0 OR Id = @Id)
      AND (@MoldRepairLogId IS NULL OR MoldRepairLogId = @MoldRepairLogId)
    ORDER BY MoldRepairLogId DESC, Id DESC;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_MoldRepairImage_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairImage_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairImage_Upsert]
    @Id INT = -1,
    @MoldRepairLogId INT,
    @ImageType VARCHAR(10),
    @ImageUrl NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_MoldRepairImage]
        (
            MoldRepairLogId,
            ImageType,
            ImageUrl,
            CreatedAt
        )
        VALUES
        (
            @MoldRepairLogId,
            @ImageType,
            @ImageUrl,
            SYSDATETIME()
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_MoldRepairImage]
        SET
            MoldRepairLogId = @MoldRepairLogId,
            ImageType = @ImageType,
            ImageUrl = @ImageUrl
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_MoldRepairImage_Select] @Id = @Id, @MoldRepairLogId = NULL;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_MoldRepairImage_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairImage_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_MoldRepairImage_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [nhvpa3en_vpa01].[CDV_MoldRepairImage]
    WHERE Id = @Id;

    SELECT @Id AS Id;
END;
GO
