IF COL_LENGTH(N'dbo.CDV_MachineOperationLog', N'StageName') IS NULL
BEGIN
    ALTER TABLE [dbo].[CDV_MachineOperationLog]
    ADD [StageName] NVARCHAR(100) NULL;
END
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
        l.StageName,
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
END
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
    @StageName NVARCHAR(100) = NULL,
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
            StageName,
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
            @StageName,
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
            StageName = @StageName,
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
END
GO
