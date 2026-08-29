SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_StockOutDetail', N'Manufacturerid') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_StockOutDetail]
        ADD [Manufacturerid] INT NULL;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOutDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.*,
        so.StockOutNo,
        manufacturer.Code AS ManufacturerCode,
        manufacturer.Name AS ManufacturerName,
        m.Code AS MaterialCode,
        m.Name AS MaterialName
    FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] d
    LEFT JOIN [nhvpa3en_vpa01].[CDV_StockOut] so ON so.Id = d.StockOutId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Manufacturer] manufacturer ON manufacturer.Id = d.Manufacturerid
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] m ON m.Id = d.MaterialId
    WHERE @Id = 0 OR d.Id = @Id
    ORDER BY d.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOutDetail_SelectByStockOutId]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_SelectByStockOutId] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_SelectByStockOutId]
(
    @StockOutId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.*,
        manufacturer.Code AS ManufacturerCode,
        manufacturer.Name AS ManufacturerName,
        m.Code AS MaterialCode,
        m.Name AS MaterialName
    FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] d
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Manufacturer] manufacturer ON manufacturer.Id = d.Manufacturerid
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] m ON m.Id = d.MaterialId
    WHERE d.StockOutId = @StockOutId
    ORDER BY d.Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOutDetail_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @StockOutId INT = NULL,
    @Manufacturerid INT = NULL,
    @MaterialId INT = NULL,
    @Qty DECIMAL(18,2) = NULL,
    @Note NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF @StockOutId IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_StockOut] WHERE Id = @StockOutId)
        BEGIN
            SELECT @Id AS ID, -2 AS ErrCode, 'STOCK_OUT_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF @MaterialId IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Material] WHERE Id = @MaterialId)
        BEGIN
            SELECT @Id AS ID, -3 AS ErrCode, 'MATERIAL_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF @Manufacturerid IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Manufacturer] WHERE Id = @Manufacturerid)
        BEGIN
            SELECT @Id AS ID, -4 AS ErrCode, 'MANUFACTURER_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_StockOutDetail]
            (
                StockOutId,
                Manufacturerid,
                MaterialId,
                Qty,
                Note
            )
            VALUES
            (
                @StockOutId,
                @Manufacturerid,
                @MaterialId,
                @Qty,
                @Note
            );

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_StockOutDetail]
            SET
                StockOutId = @StockOutId,
                Manufacturerid = @Manufacturerid,
                MaterialId = @MaterialId,
                Qty = @Qty,
                Note = @Note
            WHERE Id = @Id;
        END

        COMMIT TRAN;

        SELECT @Id AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @Id AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOutDetail_BulkSave]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_BulkSave] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_BulkSave]
(
    @StockOutId INT,
    @UserId INT = 0,
    @DetailsJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_StockOut] WHERE Id = @StockOutId)
        BEGIN
            SELECT @StockOutId AS ID, -2 AS ErrCode, 'STOCK_OUT_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISJSON(@DetailsJson) <> 1
        BEGIN
            SELECT @StockOutId AS ID, -1 AS ErrCode, 'DETAILS_JSON_INVALID' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DECLARE @Details TABLE
        (
            Id INT NULL,
            Manufacturerid INT NULL,
            MaterialId INT NULL,
            Qty DECIMAL(18,2) NULL,
            Note NVARCHAR(500) NULL
        );

        INSERT INTO @Details
        (
            Id,
            Manufacturerid,
            MaterialId,
            Qty,
            Note
        )
        SELECT
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.id')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.Id'))
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.manufacturerid')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.manufacturerId')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.Manufacturerid')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.ManufacturerId'))
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.materialId')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.MaterialId'))
            ),
            COALESCE(
                TRY_CONVERT(DECIMAL(18,2), JSON_VALUE([value], '$.qty')),
                TRY_CONVERT(DECIMAL(18,2), JSON_VALUE([value], '$.Qty'))
            ),
            COALESCE(
                JSON_VALUE([value], '$.note'),
                JSON_VALUE([value], '$.Note')
            )
        FROM OPENJSON(@DetailsJson);

        IF EXISTS
        (
            SELECT 1
            FROM @Details d
            WHERE d.MaterialId IS NOT NULL
              AND NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Material] m WHERE m.Id = d.MaterialId)
        )
        BEGIN
            SELECT @StockOutId AS ID, -3 AS ErrCode, 'MATERIAL_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF EXISTS
        (
            SELECT 1
            FROM @Details d
            WHERE d.Manufacturerid IS NOT NULL
              AND NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Manufacturer] manufacturer WHERE manufacturer.Id = d.Manufacturerid)
        )
        BEGIN
            SELECT @StockOutId AS ID, -4 AS ErrCode, 'MANUFACTURER_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DELETE target
        FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] target
        WHERE target.StockOutId = @StockOutId
          AND NOT EXISTS
          (
              SELECT 1
              FROM @Details source
              WHERE source.Id IS NOT NULL
                AND source.Id = target.Id
          );

        UPDATE target
        SET
            Manufacturerid = source.Manufacturerid,
            MaterialId = source.MaterialId,
            Qty = source.Qty,
            Note = source.Note
        FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] target
        JOIN @Details source ON source.Id = target.Id
        WHERE target.StockOutId = @StockOutId;

        INSERT INTO [nhvpa3en_vpa01].[CDV_StockOutDetail]
        (
            StockOutId,
            Manufacturerid,
            MaterialId,
            Qty,
            Note
        )
        SELECT
            @StockOutId,
            source.Manufacturerid,
            source.MaterialId,
            source.Qty,
            source.Note
        FROM @Details source
        WHERE ISNULL(source.Id, -1) = -1
           OR NOT EXISTS
           (
               SELECT 1
               FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] target
               WHERE target.Id = source.Id
                 AND target.StockOutId = @StockOutId
           );

        COMMIT TRAN;

        SELECT @StockOutId AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @StockOutId AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO
