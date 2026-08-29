SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF COL_LENGTH('dbo.CDV_MachineOperationLog', 'InputBy') IS NOT NULL
    EXEC sp_rename 'dbo.CDV_MachineOperationLog.InputBy', 'CreatedBy', 'COLUMN';

IF COL_LENGTH('dbo.CDV_MachineOperationLog', 'InputAt') IS NOT NULL
    EXEC sp_rename 'dbo.CDV_MachineOperationLog.InputAt', 'CreatedAt', 'COLUMN';
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CDV_MachineOperationLog]') AND name = N'IX_CDV_Operation_Machine_Time')
    DROP INDEX [IX_CDV_Operation_Machine_Time] ON [dbo].[CDV_MachineOperationLog];
GO

DECLARE @CreatedAtDefaultConstraint SYSNAME;

SELECT @CreatedAtDefaultConstraint = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[CDV_MachineOperationLog]')
  AND c.name = N'CreatedAt';

IF @CreatedAtDefaultConstraint IS NOT NULL
    EXEC(N'ALTER TABLE [dbo].[CDV_MachineOperationLog] DROP CONSTRAINT [' + @CreatedAtDefaultConstraint + N']');
GO

IF COL_LENGTH('dbo.CDV_MachineOperationLog', 'CreatedBy') IS NOT NULL
BEGIN
    UPDATE [dbo].[CDV_MachineOperationLog]
    SET CreatedBy = NULL
    WHERE TRY_CONVERT(INT, CreatedBy) IS NULL;

    ALTER TABLE [dbo].[CDV_MachineOperationLog] ALTER COLUMN CreatedBy INT NULL;
END;

IF COL_LENGTH('dbo.CDV_MachineOperationLog', 'CreatedAt') IS NOT NULL
    ALTER TABLE [dbo].[CDV_MachineOperationLog] ALTER COLUMN CreatedAt DATETIME NULL;

IF COL_LENGTH('dbo.CDV_MachineOperationLog', 'UpdatedBy') IS NOT NULL
BEGIN
    UPDATE [dbo].[CDV_MachineOperationLog]
    SET UpdatedBy = NULL
    WHERE TRY_CONVERT(INT, UpdatedBy) IS NULL;

    ALTER TABLE [dbo].[CDV_MachineOperationLog] ALTER COLUMN UpdatedBy INT NULL;
END;

IF COL_LENGTH('dbo.CDV_MachineOperationLog', 'UpdatedAt') IS NOT NULL
    ALTER TABLE [dbo].[CDV_MachineOperationLog] ALTER COLUMN UpdatedAt DATETIME NULL;
GO

IF COL_LENGTH('dbo.CDV_MachineOperationLog', 'CreatedAt') IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1
       FROM sys.default_constraints dc
       JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
       WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[CDV_MachineOperationLog]')
         AND c.name = N'CreatedAt'
   )
    ALTER TABLE [dbo].[CDV_MachineOperationLog]
    ADD CONSTRAINT [DF_CDV_Operation_CreatedAt] DEFAULT (GETDATE()) FOR [CreatedAt];

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CDV_MachineOperationLog]') AND name = N'IX_CDV_Operation_Machine_Time')
    CREATE INDEX [IX_CDV_Operation_Machine_Time]
    ON [dbo].[CDV_MachineOperationLog]([MachineId], [StartTime], [EndTime])
    INCLUDE([StatusCode], [MoldId], [CreatedBy]);
GO

IF OBJECT_ID(N'[dbo].[CDV_Machine_Save]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[CDV_Machine_Save];
IF OBJECT_ID(N'[dbo].[CDV_Machine_GetList]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[CDV_Machine_GetList];
IF OBJECT_ID(N'[dbo].[CDV_Mold_Save]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[CDV_Mold_Save];
IF OBJECT_ID(N'[dbo].[CDV_Mold_GetList]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[CDV_Mold_GetList];
IF OBJECT_ID(N'[dbo].[CDV_MachineStatus_GetList]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[CDV_MachineStatus_GetList];
IF OBJECT_ID(N'[dbo].[CDV_MachineOperation_Save]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[CDV_MachineOperation_Save];
IF OBJECT_ID(N'[dbo].[CDV_MachineOperation_GetById]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[CDV_MachineOperation_GetById];
IF OBJECT_ID(N'[dbo].[CDV_MachineOperation_GetList]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[CDV_MachineOperation_GetList];
GO

IF OBJECT_ID(N'[dbo].[CDV_Machine_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[CDV_Machine_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[CDV_Machine_Select]
(
    @MachineId INT = NULL,
    @Keyword NVARCHAR(100) = NULL,
    @IsActive BIT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT m.*
    FROM [dbo].[CDV_Machine] m
    WHERE (@MachineId IS NULL OR m.MachineId = @MachineId)
      AND (@IsActive IS NULL OR m.IsActive = @IsActive)
      AND
      (
          @Keyword IS NULL
          OR m.MachineCode LIKE '%' + @Keyword + '%'
          OR m.MachineName LIKE N'%' + @Keyword + N'%'
      )
    ORDER BY m.MachineCode;
END;
GO

IF OBJECT_ID(N'[dbo].[CDV_Machine_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[CDV_Machine_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[CDV_Machine_Upsert]
(
    @MachineId INT = NULL,
    @MachineCode VARCHAR(30),
    @MachineName NVARCHAR(100),
    @MachineGroup NVARCHAR(100) = NULL,
    @IsActive BIT = 1,
    @Note NVARCHAR(500) = NULL,
    @UserId VARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @MachineCode = UPPER(LTRIM(RTRIM(@MachineCode)));
    SET @UserId = ISNULL(NULLIF(LTRIM(RTRIM(@UserId)), ''), '0');

    IF NULLIF(@MachineCode, '') IS NULL OR NULLIF(LTRIM(RTRIM(@MachineName)), N'') IS NULL
        THROW 50001, N'Machine code and machine name are required.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM [dbo].[CDV_Machine]
        WHERE MachineCode = @MachineCode
          AND MachineId <> ISNULL(@MachineId, 0)
    )
        THROW 50002, N'Machine code already exists.', 1;

    IF ISNULL(@MachineId, 0) = 0
    BEGIN
        INSERT INTO [dbo].[CDV_Machine]
        (
            MachineCode,
            MachineName,
            MachineGroup,
            IsActive,
            Note,
            CreatedBy
        )
        VALUES
        (
            @MachineCode,
            @MachineName,
            @MachineGroup,
            ISNULL(@IsActive, 1),
            @Note,
            @UserId
        );

        SET @MachineId = CONVERT(INT, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE [dbo].[CDV_Machine]
        SET
            MachineCode = @MachineCode,
            MachineName = @MachineName,
            MachineGroup = @MachineGroup,
            IsActive = ISNULL(@IsActive, IsActive),
            Note = @Note,
            UpdatedBy = @UserId,
            UpdatedAt = SYSDATETIME()
        WHERE MachineId = @MachineId;

        IF @@ROWCOUNT = 0
            THROW 50003, N'Machine not found.', 1;
    END;

    EXEC [dbo].[CDV_Machine_Select] @MachineId = @MachineId;
END;
GO

IF OBJECT_ID(N'[dbo].[CDV_Machine_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[CDV_Machine_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[CDV_Machine_Delete]
(
    @MachineId INT,
    @UserId VARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @UserId = ISNULL(NULLIF(LTRIM(RTRIM(@UserId)), ''), '0');

    UPDATE [dbo].[CDV_Machine]
    SET IsActive = 0,
        UpdatedBy = @UserId,
        UpdatedAt = SYSDATETIME()
    WHERE MachineId = @MachineId;

    IF @@ROWCOUNT = 0
        THROW 50004, N'Machine not found.', 1;

    SELECT @MachineId AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
END;
GO

IF OBJECT_ID(N'[dbo].[CDV_Mold_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[CDV_Mold_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[CDV_Mold_Select]
(
    @MoldId INT = NULL,
    @Keyword NVARCHAR(150) = NULL,
    @IsActive BIT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT mo.*
    FROM [dbo].[CDV_Mold] mo
    WHERE (@MoldId IS NULL OR mo.MoldId = @MoldId)
      AND (@IsActive IS NULL OR mo.IsActive = @IsActive)
      AND
      (
          @Keyword IS NULL
          OR mo.MoldCode LIKE '%' + @Keyword + '%'
          OR mo.MoldName LIKE N'%' + @Keyword + N'%'
          OR mo.ProductCode LIKE '%' + @Keyword + '%'
          OR mo.ProductName LIKE N'%' + @Keyword + N'%'
      )
    ORDER BY mo.MoldCode;
END;
GO

IF OBJECT_ID(N'[dbo].[CDV_Mold_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[CDV_Mold_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[CDV_Mold_Upsert]
(
    @MoldId INT = NULL,
    @MoldCode VARCHAR(50),
    @MoldName NVARCHAR(150),
    @ProductCode VARCHAR(50) = NULL,
    @ProductName NVARCHAR(150) = NULL,
    @IsActive BIT = 1,
    @Note NVARCHAR(500) = NULL,
    @UserId VARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @MoldCode = UPPER(LTRIM(RTRIM(@MoldCode)));
    SET @UserId = ISNULL(NULLIF(LTRIM(RTRIM(@UserId)), ''), '0');

    IF NULLIF(@MoldCode, '') IS NULL OR NULLIF(LTRIM(RTRIM(@MoldName)), N'') IS NULL
        THROW 50011, N'Mold code and mold name are required.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM [dbo].[CDV_Mold]
        WHERE MoldCode = @MoldCode
          AND MoldId <> ISNULL(@MoldId, 0)
    )
        THROW 50012, N'Mold code already exists.', 1;

    IF ISNULL(@MoldId, 0) = 0
    BEGIN
        INSERT INTO [dbo].[CDV_Mold]
        (
            MoldCode,
            MoldName,
            ProductCode,
            ProductName,
            IsActive,
            Note,
            CreatedBy
        )
        VALUES
        (
            @MoldCode,
            @MoldName,
            @ProductCode,
            @ProductName,
            ISNULL(@IsActive, 1),
            @Note,
            @UserId
        );

        SET @MoldId = CONVERT(INT, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE [dbo].[CDV_Mold]
        SET
            MoldCode = @MoldCode,
            MoldName = @MoldName,
            ProductCode = @ProductCode,
            ProductName = @ProductName,
            IsActive = ISNULL(@IsActive, IsActive),
            Note = @Note,
            UpdatedBy = @UserId,
            UpdatedAt = SYSDATETIME()
        WHERE MoldId = @MoldId;

        IF @@ROWCOUNT = 0
            THROW 50013, N'Mold not found.', 1;
    END;

    EXEC [dbo].[CDV_Mold_Select] @MoldId = @MoldId;
END;
GO

IF OBJECT_ID(N'[dbo].[CDV_Mold_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[CDV_Mold_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[CDV_Mold_Delete]
(
    @MoldId INT,
    @UserId VARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @UserId = ISNULL(NULLIF(LTRIM(RTRIM(@UserId)), ''), '0');

    UPDATE [dbo].[CDV_Mold]
    SET IsActive = 0,
        UpdatedBy = @UserId,
        UpdatedAt = SYSDATETIME()
    WHERE MoldId = @MoldId;

    IF @@ROWCOUNT = 0
        THROW 50014, N'Mold not found.', 1;

    SELECT @MoldId AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
END;
GO

IF OBJECT_ID(N'[dbo].[CDV_MachineStatus_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[CDV_MachineStatus_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[CDV_MachineStatus_Select]
(
    @IsActive BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [dbo].[CDV_MachineStatus]
    WHERE @IsActive IS NULL OR IsActive = @IsActive
    ORDER BY DisplayOrder, StatusCode;
END;
GO

IF OBJECT_ID(N'[dbo].[CDV_MachineOperation_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[CDV_MachineOperation_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[CDV_MachineOperation_Select]
(
    @OperationLogId BIGINT = NULL,
    @DateFrom DATE = NULL,
    @DateTo DATE = NULL,
    @MachineId INT = NULL,
    @StatusCode VARCHAR(30) = NULL,
    @MoldId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @OperationLogId IS NULL
    BEGIN
        IF @DateFrom IS NULL OR @DateTo IS NULL
            THROW 50031, N'DateFrom and DateTo are required.', 1;

        IF @DateTo < @DateFrom
            THROW 50032, N'Date range is invalid.', 1;
    END;

    SELECT
        l.OperationLogId,
        l.MachineId,
        m.MachineCode,
        m.MachineName,
        l.StartTime,
        l.EndTime,
        l.StatusCode,
        s.StatusName,
        s.ColorHex,
        l.MoldId,
        mo.MoldCode,
        mo.MoldName,
        mo.ProductCode,
        mo.ProductName,
        l.Note,
        l.CreatedBy,
        l.CreatedAt,
        l.UpdatedBy,
        l.UpdatedAt
    FROM [dbo].[CDV_MachineOperationLog] l
    JOIN [dbo].[CDV_Machine] m ON m.MachineId = l.MachineId
    JOIN [dbo].[CDV_MachineStatus] s ON s.StatusCode = l.StatusCode
    LEFT JOIN [dbo].[CDV_Mold] mo ON mo.MoldId = l.MoldId
    WHERE (@OperationLogId IS NULL OR l.OperationLogId = @OperationLogId)
      AND
      (
          @OperationLogId IS NOT NULL
          OR
          (
              l.StartTime < DATEADD(DAY, 1, CONVERT(DATETIME2, @DateTo))
              AND l.EndTime > CONVERT(DATETIME2, @DateFrom)
              AND (@MachineId IS NULL OR l.MachineId = @MachineId)
              AND (@StatusCode IS NULL OR l.StatusCode = @StatusCode)
              AND (@MoldId IS NULL OR l.MoldId = @MoldId)
          )
      )
    ORDER BY m.MachineCode, l.StartTime;
END;
GO

IF OBJECT_ID(N'[dbo].[CDV_MachineOperation_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[CDV_MachineOperation_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[CDV_MachineOperation_Upsert]
(
    @OperationLogId BIGINT = NULL,
    @MachineId INT,
    @StartTime DATETIME2(0),
    @EndTime DATETIME2(0),
    @StatusCode VARCHAR(30),
    @MoldId INT = NULL,
    @Note NVARCHAR(1000) = NULL,
    @CreatedBy INT = NULL,
    @CreatedAt DATETIME = NULL,
    @UpdatedBy INT = NULL,
    @UpdatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @EndTime <= @StartTime
        THROW 50021, N'End time must be greater than start time.', 1;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[CDV_Machine] WHERE MachineId = @MachineId AND IsActive = 1)
        THROW 50022, N'Machine does not exist or is inactive.', 1;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[CDV_MachineStatus] WHERE StatusCode = @StatusCode AND IsActive = 1)
        THROW 50023, N'Machine status is invalid.', 1;

    IF EXISTS (SELECT 1 FROM [dbo].[CDV_MachineStatus] WHERE StatusCode = @StatusCode AND RequiresMold = 1) AND @MoldId IS NULL
        THROW 50024, N'Mold is required for this status.', 1;

    IF @MoldId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[CDV_Mold] WHERE MoldId = @MoldId AND IsActive = 1)
        THROW 50025, N'Mold does not exist or is inactive.', 1;

    BEGIN TRAN;

    IF EXISTS
    (
        SELECT 1
        FROM [dbo].[CDV_MachineOperationLog] WITH (UPDLOCK, HOLDLOCK)
        WHERE MachineId = @MachineId
          AND OperationLogId <> ISNULL(@OperationLogId, 0)
          AND StartTime < @EndTime
          AND EndTime > @StartTime
    )
    BEGIN
        ROLLBACK;
        THROW 50026, N'Time range overlaps another operation log for this machine.', 1;
    END;

    IF ISNULL(@OperationLogId, 0) = 0
    BEGIN
        INSERT INTO [dbo].[CDV_MachineOperationLog]
        (
            MachineId,
            StartTime,
            EndTime,
            StatusCode,
            MoldId,
            Note,
            CreatedBy,
            CreatedAt,
            UpdatedBy,
            UpdatedAt
        )
        VALUES
        (
            @MachineId,
            @StartTime,
            @EndTime,
            @StatusCode,
            @MoldId,
            @Note,
            NULLIF(@CreatedBy, 0),
            ISNULL(@CreatedAt, GETDATE()),
            NULLIF(@UpdatedBy, 0),
            @UpdatedAt
        );

        SET @OperationLogId = CONVERT(BIGINT, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE [dbo].[CDV_MachineOperationLog]
        SET
            MachineId = @MachineId,
            StartTime = @StartTime,
            EndTime = @EndTime,
            StatusCode = @StatusCode,
            MoldId = @MoldId,
            Note = @Note,
            CreatedBy = COALESCE(NULLIF(@CreatedBy, 0), CreatedBy),
            CreatedAt = COALESCE(@CreatedAt, CreatedAt),
            UpdatedBy = NULLIF(@UpdatedBy, 0),
            UpdatedAt = ISNULL(@UpdatedAt, GETDATE())
        WHERE OperationLogId = @OperationLogId;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK;
            THROW 50027, N'Operation log not found.', 1;
        END;
    END;

    COMMIT;

    EXEC [dbo].[CDV_MachineOperation_Select] @OperationLogId = @OperationLogId;
END;
GO

IF OBJECT_ID(N'[dbo].[CDV_MachineOperation_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [dbo].[CDV_MachineOperation_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [dbo].[CDV_MachineOperation_Delete]
(
    @OperationLogId BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[CDV_MachineOperationLog]
    WHERE OperationLogId = @OperationLogId;

    IF @@ROWCOUNT = 0
        THROW 50033, N'Operation log not found.', 1;

    SELECT @OperationLogId AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
END;
GO
