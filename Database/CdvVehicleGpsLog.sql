IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_VehicleGPSLog]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_VehicleGPSLog]
    (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [VehicleId] INT NOT NULL,
        [Latitude] DECIMAL(18,10) NULL,
        [Longitude] DECIMAL(18,10) NULL,
        [Address] NVARCHAR(500) NULL,
        [Speed] DECIMAL(10,2) NULL,
        [Angle] INT NULL,
        [IsEngineOn] BIT NULL,
        [IsParking] BIT NULL,
        [IsGpsActive] BIT NULL,
        [IsGsmLost] BIT NULL,
        [GPSTime] DATETIME NOT NULL,
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_CDV_VehicleGPSLog_CreatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_CDV_VehicleGPSLog] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CDV_VehicleGPSLog_Vehicle')
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_VehicleGPSLog]
        ADD CONSTRAINT [FK_CDV_VehicleGPSLog_Vehicle]
        FOREIGN KEY ([VehicleId]) REFERENCES [nhvpa3en_vpa01].[CDV_Vehicle] ([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_VehicleGPSLog]') AND name = N'UX_CDV_VehicleGPSLog_Vehicle_GPSTime')
BEGIN
    CREATE UNIQUE INDEX [UX_CDV_VehicleGPSLog_Vehicle_GPSTime]
    ON [nhvpa3en_vpa01].[CDV_VehicleGPSLog] ([VehicleId], [GPSTime]);
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_sp_VehicleGPSLog_Sync]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_sp_VehicleGPSLog_Sync] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_sp_VehicleGPSLog_Sync]
(
    @GpsJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF ISJSON(@GpsJson) <> 1
    BEGIN
        SELECT 0 AS InsertedCount, 0 AS IgnoredCount, -1 AS ErrCode, 'GPS_JSON_INVALID' AS ErrMsg;
        RETURN;
    END;

    DECLARE @Input TABLE
    (
        VehicleNo VARCHAR(50) NULL,
        NormalizedVehicleNo VARCHAR(50) NULL,
        Latitude DECIMAL(18,10) NULL,
        Longitude DECIMAL(18,10) NULL,
        Address NVARCHAR(500) NULL,
        Speed DECIMAL(10,2) NULL,
        Angle INT NULL,
        IsEngineOn BIT NULL,
        IsParking BIT NULL,
        IsGpsActive BIT NULL,
        IsGsmLost BIT NULL,
        GPSTime DATETIME NULL
    );

    INSERT INTO @Input
    SELECT
        NULLIF(LTRIM(RTRIM(COALESCE(JSON_VALUE([value], '$.VehicleNo'), JSON_VALUE([value], '$.vehicleNo'), JSON_VALUE([value], '$.Bs'), JSON_VALUE([value], '$.bs')))), ''),
        UPPER(REPLACE(REPLACE(REPLACE(REPLACE(NULLIF(LTRIM(RTRIM(COALESCE(JSON_VALUE([value], '$.VehicleNo'), JSON_VALUE([value], '$.vehicleNo'), JSON_VALUE([value], '$.Bs'), JSON_VALUE([value], '$.bs')))), ''), '-', ''), '.', ''), ' ', ''), '_', '')),
        TRY_CONVERT(DECIMAL(18,10), COALESCE(JSON_VALUE([value], '$.Latitude'), JSON_VALUE([value], '$.latitude'))),
        TRY_CONVERT(DECIMAL(18,10), COALESCE(JSON_VALUE([value], '$.Longitude'), JSON_VALUE([value], '$.longitude'))),
        COALESCE(JSON_VALUE([value], '$.Address'), JSON_VALUE([value], '$.address')),
        TRY_CONVERT(DECIMAL(10,2), COALESCE(JSON_VALUE([value], '$.Speed'), JSON_VALUE([value], '$.speed'))),
        TRY_CONVERT(INT, COALESCE(JSON_VALUE([value], '$.Angle'), JSON_VALUE([value], '$.angle'))),
        TRY_CONVERT(BIT, COALESCE(JSON_VALUE([value], '$.IsEngineOn'), JSON_VALUE([value], '$.isEngineOn'))),
        TRY_CONVERT(BIT, COALESCE(JSON_VALUE([value], '$.IsParking'), JSON_VALUE([value], '$.isParking'))),
        TRY_CONVERT(BIT, COALESCE(JSON_VALUE([value], '$.IsGpsActive'), JSON_VALUE([value], '$.isGpsActive'))),
        TRY_CONVERT(BIT, COALESCE(JSON_VALUE([value], '$.IsGsmLost'), JSON_VALUE([value], '$.isGsmLost'))),
        TRY_CONVERT(DATETIME, COALESCE(JSON_VALUE([value], '$.GPSTime'), JSON_VALUE([value], '$.gpsTime')))
    FROM OPENJSON(@GpsJson);

    DECLARE @CandidateCount INT =
    (
        SELECT COUNT(*)
        FROM @Input i
        INNER JOIN [nhvpa3en_vpa01].[CDV_Vehicle] v
            ON UPPER(REPLACE(REPLACE(REPLACE(REPLACE(v.VehicleNo, '-', ''), '.', ''), ' ', ''), '_', '')) = i.NormalizedVehicleNo
        WHERE i.NormalizedVehicleNo IS NOT NULL
          AND i.GPSTime IS NOT NULL
          AND ISNULL(v.IsActive, 1) = 1
    );

    ;WITH Matched AS
    (
        SELECT
            v.Id AS VehicleId,
            i.Latitude,
            i.Longitude,
            i.Address,
            i.Speed,
            i.Angle,
            i.IsEngineOn,
            i.IsParking,
            i.IsGpsActive,
            i.IsGsmLost,
            i.GPSTime
        FROM @Input i
        INNER JOIN [nhvpa3en_vpa01].[CDV_Vehicle] v
            ON UPPER(REPLACE(REPLACE(REPLACE(REPLACE(v.VehicleNo, '-', ''), '.', ''), ' ', ''), '_', '')) = i.NormalizedVehicleNo
        WHERE i.NormalizedVehicleNo IS NOT NULL
          AND i.GPSTime IS NOT NULL
          AND ISNULL(v.IsActive, 1) = 1
    )
    INSERT INTO [nhvpa3en_vpa01].[CDV_VehicleGPSLog]
    (
        VehicleId, Latitude, Longitude, Address, Speed, Angle,
        IsEngineOn, IsParking, IsGpsActive, IsGsmLost, GPSTime, CreatedAt
    )
    SELECT
        m.VehicleId, m.Latitude, m.Longitude, m.Address, m.Speed, m.Angle,
        m.IsEngineOn, m.IsParking, m.IsGpsActive, m.IsGsmLost, m.GPSTime, GETDATE()
    FROM Matched m
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM [nhvpa3en_vpa01].[CDV_VehicleGPSLog] existing
        WHERE existing.VehicleId = m.VehicleId
          AND existing.GPSTime = m.GPSTime
    );

    DECLARE @InsertedCount INT = @@ROWCOUNT;

    SELECT
        @InsertedCount AS InsertedCount,
        @CandidateCount - @InsertedCount AS IgnoredCount,
        0 AS ErrCode,
        'SUCCESS' AS ErrMsg;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_sp_VehicleGPSLog_GetLatest]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_sp_VehicleGPSLog_GetLatest] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_sp_VehicleGPSLog_GetLatest]
(
    @VehicleId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Latest AS
    (
        SELECT
            l.*,
            ROW_NUMBER() OVER (PARTITION BY l.VehicleId ORDER BY l.GPSTime DESC, l.Id DESC) AS rn
        FROM [nhvpa3en_vpa01].[CDV_VehicleGPSLog] l
        WHERE @VehicleId IS NULL OR l.VehicleId = @VehicleId
    )
    SELECT
        l.Id, l.VehicleId, v.VehicleNo, l.Latitude, l.Longitude, l.Address,
        l.Speed, l.Angle, l.IsEngineOn, l.IsParking, l.IsGpsActive,
        l.IsGsmLost, l.GPSTime, l.CreatedAt
    FROM Latest l
    INNER JOIN [nhvpa3en_vpa01].[CDV_Vehicle] v ON v.Id = l.VehicleId
    WHERE l.rn = 1
    ORDER BY v.VehicleNo;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_sp_VehicleGPSLog_GetHistory]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_sp_VehicleGPSLog_GetHistory] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_sp_VehicleGPSLog_GetHistory]
(
    @VehicleId INT,
    @FromTime DATETIME,
    @ToTime DATETIME
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.Id, l.VehicleId, v.VehicleNo, l.Latitude, l.Longitude, l.Address,
        l.Speed, l.Angle, l.IsEngineOn, l.IsParking, l.IsGpsActive,
        l.IsGsmLost, l.GPSTime, l.CreatedAt
    FROM [nhvpa3en_vpa01].[CDV_VehicleGPSLog] l
    INNER JOIN [nhvpa3en_vpa01].[CDV_Vehicle] v ON v.Id = l.VehicleId
    WHERE l.VehicleId = @VehicleId
      AND l.GPSTime >= @FromTime
      AND l.GPSTime <= @ToTime
    ORDER BY l.GPSTime DESC, l.Id DESC;
END
GO
