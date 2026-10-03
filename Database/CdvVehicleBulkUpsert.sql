IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_Vehicle', N'LoadCapacity') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_Vehicle]
    ADD [LoadCapacity] DECIMAL(5,1) NULL;
END
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_Vehicle', N'TagRfid') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_Vehicle]
    ADD [TagRfid] VARCHAR(100) NULL;
END
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_Vehicle', N'DefaultDriverPhone') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_Vehicle]
    ADD [DefaultDriverPhone] VARCHAR(20) NULL;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Vehicle_BulkUpsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_BulkUpsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_BulkUpsert]
(
    @UserId INT = 0,
    @VehiclesJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF ISJSON(@VehiclesJson) <> 1
        BEGIN
            SELECT 0 AS InsertedCount, 0 AS UpdatedCount, -1 AS ErrCode, 'VEHICLES_JSON_INVALID' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        DECLARE @Vehicles TABLE
        (
            VehicleNo VARCHAR(50) NOT NULL,
            DefaultDriverName NVARCHAR(100) NULL,
            DefaultDriverPhone VARCHAR(20) NULL,
            LoadCapacity DECIMAL(5,1) NULL,
            TagRfid VARCHAR(100) NULL,
            IsActive BIT NULL
        );

        INSERT INTO @Vehicles
        (
            VehicleNo,
            DefaultDriverName,
            DefaultDriverPhone,
            LoadCapacity,
            TagRfid,
            IsActive
        )
        SELECT
            LTRIM(RTRIM(COALESCE(
                JSON_VALUE([value], '$.vehicleNo'),
                JSON_VALUE([value], '$.VehicleNo')
            ))),
            COALESCE(
                JSON_VALUE([value], '$.defaultDriverName'),
                JSON_VALUE([value], '$.DefaultDriverName')
            ),
            COALESCE(
                JSON_VALUE([value], '$.defaultDriverPhone'),
                JSON_VALUE([value], '$.DefaultDriverPhone')
            ),
            COALESCE(
                TRY_CONVERT(DECIMAL(5,1), JSON_VALUE([value], '$.loadCapacity')),
                TRY_CONVERT(DECIMAL(5,1), JSON_VALUE([value], '$.LoadCapacity'))
            ),
            COALESCE(
                JSON_VALUE([value], '$.tagRfid'),
                JSON_VALUE([value], '$.TagRfid'),
                JSON_VALUE([value], '$.tagRFID'),
                JSON_VALUE([value], '$.TagRFID')
            ),
            CASE
                WHEN LOWER(COALESCE(JSON_VALUE([value], '$.isActive'), JSON_VALUE([value], '$.IsActive'))) = 'true' THEN 1
                WHEN LOWER(COALESCE(JSON_VALUE([value], '$.isActive'), JSON_VALUE([value], '$.IsActive'))) = 'false' THEN 0
                ELSE COALESCE(
                    TRY_CONVERT(BIT, JSON_VALUE([value], '$.isActive')),
                    TRY_CONVERT(BIT, JSON_VALUE([value], '$.IsActive'))
                )
            END
        FROM OPENJSON(@VehiclesJson);

        IF EXISTS (SELECT 1 FROM @Vehicles WHERE VehicleNo IS NULL OR VehicleNo = '')
        BEGIN
            SELECT 0 AS InsertedCount, 0 AS UpdatedCount, -2 AS ErrCode, 'VEHICLE_NO_REQUIRED' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM @Vehicles
            GROUP BY VehicleNo
            HAVING COUNT(*) > 1
        )
        BEGIN
            SELECT 0 AS InsertedCount, 0 AS UpdatedCount, -3 AS ErrCode, 'DUPLICATE_VEHICLE_NO_IN_JSON' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        DECLARE @Updated TABLE (Id INT);
        DECLARE @Inserted TABLE (Id INT);

        UPDATE target
        SET
            DefaultDriverName = source.DefaultDriverName,
            DefaultDriverPhone = source.DefaultDriverPhone,
            LoadCapacity = source.LoadCapacity,
            TagRfid = source.TagRfid,
            IsActive = ISNULL(source.IsActive, target.IsActive)
        OUTPUT INSERTED.Id INTO @Updated
        FROM [nhvpa3en_vpa01].[CDV_Vehicle] target
        INNER JOIN @Vehicles source ON source.VehicleNo = target.VehicleNo;

        INSERT INTO [nhvpa3en_vpa01].[CDV_Vehicle]
        (
            VehicleNo,
            DefaultDriverName,
            DefaultDriverPhone,
            LoadCapacity,
            TagRfid,
            IsActive
        )
        OUTPUT INSERTED.Id INTO @Inserted
        SELECT
            source.VehicleNo,
            source.DefaultDriverName,
            source.DefaultDriverPhone,
            source.LoadCapacity,
            source.TagRfid,
            ISNULL(source.IsActive, 1)
        FROM @Vehicles source
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_Vehicle] target
            WHERE target.VehicleNo = source.VehicleNo
        );

        COMMIT TRAN;

        SELECT
            (SELECT COUNT(*) FROM @Inserted) AS InsertedCount,
            (SELECT COUNT(*) FROM @Updated) AS UpdatedCount,
            0 AS ErrCode,
            'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT 0 AS InsertedCount, 0 AS UpdatedCount, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO
