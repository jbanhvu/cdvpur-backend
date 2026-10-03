IF COL_LENGTH(N'dbo.CDV_Machine', N'DefaultStageName') IS NULL
BEGIN
    ALTER TABLE [dbo].[CDV_Machine]
    ADD [DefaultStageName] NVARCHAR(100) NULL;
END
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
END
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
    @DefaultStageName NVARCHAR(100) = NULL,
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
            DefaultStageName,
            IsActive,
            Note,
            CreatedBy
        )
        VALUES
        (
            @MachineCode,
            @MachineName,
            @MachineGroup,
            @DefaultStageName,
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
            DefaultStageName = @DefaultStageName,
            IsActive = ISNULL(@IsActive, IsActive),
            Note = @Note,
            UpdatedBy = @UserId,
            UpdatedAt = SYSDATETIME()
        WHERE MachineId = @MachineId;

        IF @@ROWCOUNT = 0
            THROW 50003, N'Machine not found.', 1;
    END;

    EXEC [dbo].[CDV_Machine_Select] @MachineId = @MachineId;
END
GO
