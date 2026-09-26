IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_Vehicle', N'LoadCapacity') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_Vehicle]
    ADD [LoadCapacity] DECIMAL(5,1) NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_Vehicle', N'TagRfid') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_Vehicle]
    ADD [TagRfid] VARCHAR(100) NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNote', N'DriverName') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNote]
    ADD [DriverName] NVARCHAR(100) NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNote', N'TagRfid') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNote]
    ADD [TagRfid] VARCHAR(100) NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_Product', N'PackType') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_Product]
    ADD [PackType] NVARCHAR(50) NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_Product', N'QuantityPerPack') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_Product]
    ADD [QuantityPerPack] INT NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'PackType') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    ADD [PackType] NVARCHAR(50) NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'QuantityPerPack') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    ADD [QuantityPerPack] INT NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'DELIVERY_LOCATION') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    ADD [DELIVERY_LOCATION] NVARCHAR(40) NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'DELIVERY') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    ADD [DELIVERY] NVARCHAR(10) NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'PackingGroupNo') IS NULL
BEGIN
    IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'SortOrder') IS NOT NULL
        EXEC sp_rename N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail.SortOrder', N'PackingGroupNo', N'COLUMN';
    ELSE
        ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail] ADD [PackingGroupNo] INT NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'DELIVERY_Line') IS NULL
BEGIN
    IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'Line') IS NOT NULL
        EXEC sp_rename N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail.Line', N'DELIVERY_Line', N'COLUMN';
    ELSE
        ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail] ADD [DELIVERY_Line] NVARCHAR(10) NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'DELIVERY_Line') IS NOT NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    ALTER COLUMN [DELIVERY_Line] NVARCHAR(10) NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'Item_No') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    ADD [Item_No] INT NULL;
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'DeliveryTag') IS NOT NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    DROP COLUMN [DeliveryTag];
END;
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_DeliveryNoteDetail', N'RFIDTag') IS NOT NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    DROP COLUMN [RFIDTag];
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Customer_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Select] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Code,
        Name,
        Address,
        IsActive
    FROM [nhvpa3en_vpa01].[CDV_Customer]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
    ORDER BY Id DESC;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Customer_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Upsert]
    @Id INT = -1,
    @UserId INT = 0,
    @Code VARCHAR(50),
    @Name NVARCHAR(200),
    @Address NVARCHAR(300) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_Customer]
        (
            Code,
            Name,
            Address,
            IsActive
        )
        VALUES
        (
            @Code,
            @Name,
            @Address,
            ISNULL(@IsActive, 1)
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_Customer]
        SET
            Code = @Code,
            Name = @Name,
            Address = @Address,
            IsActive = ISNULL(@IsActive, IsActive)
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_Customer_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Customer_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [nhvpa3en_vpa01].[CDV_Customer]
    SET IsActive = 0
    WHERE Id = @Id;

    EXEC [nhvpa3en_vpa01].[CDV_Customer_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Product_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Select] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Code,
        Name,
        PackType,
        QuantityPerPack,
        IsActive
    FROM [nhvpa3en_vpa01].[CDV_Product]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
    ORDER BY Id DESC;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Product_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Upsert]
    @Id INT = -1,
    @UserId INT = 0,
    @Code VARCHAR(50),
    @Name NVARCHAR(300) = NULL,
    @PackType NVARCHAR(50) = NULL,
    @QuantityPerPack INT = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_Product]
        (
            Code,
            Name,
            PackType,
            QuantityPerPack,
            IsActive
        )
        VALUES
        (
            @Code,
            @Name,
            @PackType,
            @QuantityPerPack,
            ISNULL(@IsActive, 1)
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_Product]
        SET
            Code = @Code,
            Name = @Name,
            PackType = @PackType,
            QuantityPerPack = @QuantityPerPack,
            IsActive = ISNULL(@IsActive, IsActive)
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_Product_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Product_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [nhvpa3en_vpa01].[CDV_Product]
    SET IsActive = 0
    WHERE Id = @Id;

    EXEC [nhvpa3en_vpa01].[CDV_Product_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Product_BulkUpsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Product_BulkUpsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Product_BulkUpsert]
(
    @UserId INT = 0,
    @ProductsJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF ISJSON(@ProductsJson) <> 1
        BEGIN
            SELECT 0 AS InsertedCount, 0 AS UpdatedCount, -1 AS ErrCode, 'PRODUCTS_JSON_INVALID' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        DECLARE @Products TABLE
        (
            Code VARCHAR(50) NOT NULL,
            Name NVARCHAR(300) NULL,
            PackType NVARCHAR(50) NULL,
            QuantityPerPack INT NULL,
            IsActive BIT NULL
        );

        INSERT INTO @Products
        (
            Code,
            Name,
            PackType,
            QuantityPerPack,
            IsActive
        )
        SELECT
            LTRIM(RTRIM(COALESCE(
                JSON_VALUE([value], '$.code'),
                JSON_VALUE([value], '$.Code')
            ))),
            COALESCE(
                JSON_VALUE([value], '$.name'),
                JSON_VALUE([value], '$.Name')
            ),
            COALESCE(
                JSON_VALUE([value], '$.packType'),
                JSON_VALUE([value], '$.PackType')
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.quantityPerPack')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.QuantityPerPack'))
            ),
            CASE
                WHEN LOWER(COALESCE(JSON_VALUE([value], '$.isActive'), JSON_VALUE([value], '$.IsActive'))) = 'true' THEN 1
                WHEN LOWER(COALESCE(JSON_VALUE([value], '$.isActive'), JSON_VALUE([value], '$.IsActive'))) = 'false' THEN 0
                ELSE COALESCE(
                    TRY_CONVERT(BIT, JSON_VALUE([value], '$.isActive')),
                    TRY_CONVERT(BIT, JSON_VALUE([value], '$.IsActive'))
                )
            END
        FROM OPENJSON(@ProductsJson);

        IF EXISTS (SELECT 1 FROM @Products WHERE Code IS NULL OR Code = '')
        BEGIN
            SELECT 0 AS InsertedCount, 0 AS UpdatedCount, -2 AS ErrCode, 'PRODUCT_CODE_REQUIRED' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM @Products
            GROUP BY Code
            HAVING COUNT(*) > 1
        )
        BEGIN
            SELECT 0 AS InsertedCount, 0 AS UpdatedCount, -3 AS ErrCode, 'DUPLICATE_PRODUCT_CODE_IN_JSON' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        DECLARE @Updated TABLE (Id INT);
        DECLARE @Inserted TABLE (Id INT);

        UPDATE target
        SET
            Name = source.Name,
            PackType = source.PackType,
            QuantityPerPack = source.QuantityPerPack,
            IsActive = ISNULL(source.IsActive, target.IsActive)
        OUTPUT INSERTED.Id INTO @Updated
        FROM [nhvpa3en_vpa01].[CDV_Product] target
        INNER JOIN @Products source ON source.Code = target.Code;

        INSERT INTO [nhvpa3en_vpa01].[CDV_Product]
        (
            Code,
            Name,
            PackType,
            QuantityPerPack,
            IsActive
        )
        OUTPUT INSERTED.Id INTO @Inserted
        SELECT
            source.Code,
            source.Name,
            source.PackType,
            source.QuantityPerPack,
            ISNULL(source.IsActive, 1)
        FROM @Products source
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_Product] target
            WHERE target.Code = source.Code
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
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Vehicle_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Select] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        VehicleNo,
        DefaultDriverName,
        LoadCapacity,
        TagRfid,
        IsActive
    FROM [nhvpa3en_vpa01].[CDV_Vehicle]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
    ORDER BY Id DESC;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Vehicle_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Upsert]
    @Id INT = -1,
    @UserId INT = 0,
    @VehicleNo VARCHAR(50),
    @DefaultDriverName NVARCHAR(100) = NULL,
    @LoadCapacity DECIMAL(5,1) = NULL,
    @TagRfid VARCHAR(100) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_Vehicle]
        (
            VehicleNo,
            DefaultDriverName,
            LoadCapacity,
            TagRfid,
            IsActive
        )
        VALUES
        (
            @VehicleNo,
            @DefaultDriverName,
            @LoadCapacity,
            @TagRfid,
            ISNULL(@IsActive, 1)
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_Vehicle]
        SET
            VehicleNo = @VehicleNo,
            DefaultDriverName = @DefaultDriverName,
            LoadCapacity = @LoadCapacity,
            TagRfid = @TagRfid,
            IsActive = ISNULL(@IsActive, IsActive)
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_Vehicle_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Vehicle_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [nhvpa3en_vpa01].[CDV_Vehicle]
    SET IsActive = 0
    WHERE Id = @Id;

    EXEC [nhvpa3en_vpa01].[CDV_Vehicle_Select] @Id = @Id;
END;
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
            LoadCapacity DECIMAL(5,1) NULL,
            TagRfid VARCHAR(100) NULL,
            IsActive BIT NULL
        );

        INSERT INTO @Vehicles
        (
            VehicleNo,
            DefaultDriverName,
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
            LoadCapacity,
            TagRfid,
            IsActive
        )
        OUTPUT INSERTED.Id INTO @Inserted
        SELECT
            source.VehicleNo,
            source.DefaultDriverName,
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
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNote_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Select] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        dn.Id,
        dn.DeliveryNo,
        dn.DeliveryDate,
        dn.CustomerId,
        c.Code AS CustomerCode,
        c.Name AS CustomerName,
        dn.VehicleId,
        v.VehicleNo,
        v.DefaultDriverName,
        v.LoadCapacity,
        v.TagRfid AS VehicleTagRfid,
        dn.DriverName,
        dn.TagRfid,
        dn.ReceiverName,
        dn.TotalQuantity,
        dn.Remark,
        dn.CreatedAt,
        dn.CreatedBy,
        dn.UpdatedAt,
        dn.UpdatedBy
    FROM [nhvpa3en_vpa01].[CDV_DeliveryNote] dn
    INNER JOIN [nhvpa3en_vpa01].[CDV_Customer] c ON c.Id = dn.CustomerId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Vehicle] v ON v.Id = dn.VehicleId
    WHERE (@Id IS NULL OR @Id = 0 OR dn.Id = @Id)
    ORDER BY dn.DeliveryDate DESC, dn.Id DESC;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNote_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Upsert]
    @Id INT = -1,
    @DeliveryNo VARCHAR(50),
    @DeliveryDate DATE,
    @CustomerId INT,
    @VehicleId INT = NULL,
    @DriverName NVARCHAR(100) = NULL,
    @TagRfid VARCHAR(100) = NULL,
    @ReceiverName NVARCHAR(100) = NULL,
    @TotalQuantity INT = NULL,
    @Remark NVARCHAR(500) = NULL,
    @CreatedBy INT = NULL,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_DeliveryNote]
        (
            DeliveryNo,
            DeliveryDate,
            CustomerId,
            VehicleId,
            DriverName,
            TagRfid,
            ReceiverName,
            TotalQuantity,
            Remark,
            CreatedAt,
            CreatedBy
        )
        VALUES
        (
            @DeliveryNo,
            @DeliveryDate,
            @CustomerId,
            @VehicleId,
            @DriverName,
            @TagRfid,
            @ReceiverName,
            @TotalQuantity,
            @Remark,
            GETDATE(),
            @CreatedBy
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_DeliveryNote]
        SET
            DeliveryNo = @DeliveryNo,
            DeliveryDate = @DeliveryDate,
            CustomerId = @CustomerId,
            VehicleId = @VehicleId,
            DriverName = @DriverName,
            TagRfid = @TagRfid,
            ReceiverName = @ReceiverName,
            TotalQuantity = @TotalQuantity,
            Remark = @Remark,
            UpdatedAt = GETDATE(),
            UpdatedBy = @UpdatedBy
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_DeliveryNote_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNote_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    WHERE DeliveryNoteId = @Id;

    DELETE FROM [nhvpa3en_vpa01].[CDV_DeliveryNote]
    WHERE Id = @Id;

    SELECT @Id AS Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Select]
    @Id INT = 0,
    @DeliveryNoteId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.Id,
        d.DeliveryNoteId,
        d.Seq,
        d.PackingGroupNo,
        d.DELIVERY_Line,
        d.Item_No,
        d.ProductId,
        p.Code AS ProductCode,
        p.Name AS ProductName,
        p.PackType AS ProductPackType,
        p.QuantityPerPack AS ProductQuantityPerPack,
        d.PackType,
        d.QuantityPerPack,
        d.Quantity,
        d.DeliveryTime,
        d.TrolleyBox,
        d.DELIVERY,
        d.DELIVERY_LOCATION,
        d.Remark
    FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail] d
    INNER JOIN [nhvpa3en_vpa01].[CDV_Product] p ON p.Id = d.ProductId
    WHERE (@Id IS NULL OR @Id = 0 OR d.Id = @Id)
      AND (@DeliveryNoteId IS NULL OR d.DeliveryNoteId = @DeliveryNoteId)
    ORDER BY d.DeliveryNoteId DESC, ISNULL(d.PackingGroupNo, ISNULL(d.Seq, d.Id)), d.Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Upsert]
    @Id INT = -1,
    @DeliveryNoteId INT,
    @Seq INT = NULL,
    @PackingGroupNo INT = NULL,
    @DELIVERY_Line NVARCHAR(10) = NULL,
    @Item_No INT = NULL,
    @ProductId INT,
    @Quantity INT,
    @PackType NVARCHAR(50) = NULL,
    @QuantityPerPack INT = NULL,
    @DeliveryTime VARCHAR(20) = NULL,
    @TrolleyBox VARCHAR(100) = NULL,
    @DELIVERY NVARCHAR(10) = NULL,
    @DELIVERY_LOCATION NVARCHAR(40) = NULL,
    @Remark NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
        (
            DeliveryNoteId,
            Seq,
            PackingGroupNo,
            DELIVERY_Line,
            Item_No,
            ProductId,
            Quantity,
            PackType,
            QuantityPerPack,
            DeliveryTime,
            TrolleyBox,
            DELIVERY,
            DELIVERY_LOCATION,
            Remark
        )
        VALUES
        (
            @DeliveryNoteId,
            @Seq,
            @PackingGroupNo,
            @DELIVERY_Line,
            @Item_No,
            @ProductId,
            @Quantity,
            @PackType,
            @QuantityPerPack,
            @DeliveryTime,
            @TrolleyBox,
            @DELIVERY,
            @DELIVERY_LOCATION,
            @Remark
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
        SET
            DeliveryNoteId = @DeliveryNoteId,
            Seq = @Seq,
            PackingGroupNo = @PackingGroupNo,
            DELIVERY_Line = @DELIVERY_Line,
            Item_No = @Item_No,
            ProductId = @ProductId,
            Quantity = @Quantity,
            PackType = @PackType,
            QuantityPerPack = @QuantityPerPack,
            DeliveryTime = @DeliveryTime,
            TrolleyBox = @TrolleyBox,
            DELIVERY = @DELIVERY,
            DELIVERY_LOCATION = @DELIVERY_LOCATION,
            Remark = @Remark
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Select] @Id = @Id, @DeliveryNoteId = NULL;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    WHERE Id = @Id;

    SELECT @Id AS Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_BulkSave]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_BulkSave] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_BulkSave]
(
    @DeliveryNoteId INT,
    @UserId INT = 0,
    @DetailsJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_DeliveryNote] WHERE Id = @DeliveryNoteId)
        BEGIN
            SELECT @DeliveryNoteId AS ID, -2 AS ErrCode, 'DELIVERY_NOTE_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        IF ISJSON(@DetailsJson) <> 1
        BEGIN
            SELECT @DeliveryNoteId AS ID, -1 AS ErrCode, 'DETAILS_JSON_INVALID' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        DECLARE @Details TABLE
        (
            Id INT NULL,
            Seq INT NULL,
            PackingGroupNo INT NULL,
            DELIVERY_Line NVARCHAR(10) NULL,
            Item_No INT NULL,
            ProductId INT NULL,
            Quantity INT NULL,
            PackType NVARCHAR(50) NULL,
            QuantityPerPack INT NULL,
            DeliveryTime VARCHAR(20) NULL,
            TrolleyBox VARCHAR(100) NULL,
            DELIVERY NVARCHAR(10) NULL,
            DELIVERY_LOCATION NVARCHAR(40) NULL,
            Remark NVARCHAR(300) NULL
        );

        INSERT INTO @Details
        (
            Id,
            Seq,
            PackingGroupNo,
            DELIVERY_Line,
            Item_No,
            ProductId,
            Quantity,
            PackType,
            QuantityPerPack,
            DeliveryTime,
            TrolleyBox,
            DELIVERY,
            DELIVERY_LOCATION,
            Remark
        )
        SELECT
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.id')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.Id'))
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.seq')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.Seq'))
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.packingGroupNo')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.PackingGroupNo')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.sortOrder')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.SortOrder'))
            ),
            COALESCE(
                JSON_VALUE([value], '$.delivery_Line'),
                JSON_VALUE([value], '$.deliveryLine'),
                JSON_VALUE([value], '$.DELIVERY_Line'),
                JSON_VALUE([value], '$.DELIVERY_LINE'),
                JSON_VALUE([value], '$.line'),
                JSON_VALUE([value], '$.Line')
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.item_No')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.itemNo')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.Item_No')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.ITEM_NO')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.ItemNo'))
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.productId')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.ProductId'))
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.quantity')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.Quantity'))
            ),
            COALESCE(
                JSON_VALUE([value], '$.packType'),
                JSON_VALUE([value], '$.PackType')
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.quantityPerPack')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.QuantityPerPack'))
            ),
            COALESCE(
                JSON_VALUE([value], '$.deliveryTime'),
                JSON_VALUE([value], '$.DeliveryTime')
            ),
            COALESCE(
                JSON_VALUE([value], '$.trolleyBox'),
                JSON_VALUE([value], '$.TrolleyBox')
            ),
            COALESCE(
                JSON_VALUE([value], '$.delivery'),
                JSON_VALUE([value], '$.Delivery'),
                JSON_VALUE([value], '$.DELIVERY')
            ),
            COALESCE(
                JSON_VALUE([value], '$.deliveryLocation'),
                JSON_VALUE([value], '$.DeliveryLocation'),
                JSON_VALUE([value], '$.DELIVERY_LOCATION')
            ),
            COALESCE(
                JSON_VALUE([value], '$.remark'),
                JSON_VALUE([value], '$.Remark')
            )
        FROM OPENJSON(@DetailsJson);

        IF EXISTS (SELECT 1 FROM @Details WHERE ProductId IS NULL OR Quantity IS NULL)
        BEGIN
            SELECT @DeliveryNoteId AS ID, -3 AS ErrCode, 'DETAIL_PRODUCT_QUANTITY_REQUIRED' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM @Details d
            WHERE NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Product] p WHERE p.Id = d.ProductId)
        )
        BEGIN
            SELECT @DeliveryNoteId AS ID, -4 AS ErrCode, 'PRODUCT_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        DELETE target
        FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail] target
        WHERE target.DeliveryNoteId = @DeliveryNoteId
          AND NOT EXISTS
          (
              SELECT 1
              FROM @Details source
              WHERE source.Id IS NOT NULL
                AND source.Id = target.Id
          );

        UPDATE target
        SET
            Seq = source.Seq,
            PackingGroupNo = source.PackingGroupNo,
            DELIVERY_Line = source.DELIVERY_Line,
            Item_No = source.Item_No,
            ProductId = source.ProductId,
            Quantity = source.Quantity,
            PackType = source.PackType,
            QuantityPerPack = source.QuantityPerPack,
            DeliveryTime = source.DeliveryTime,
            TrolleyBox = source.TrolleyBox,
            DELIVERY = source.DELIVERY,
            DELIVERY_LOCATION = source.DELIVERY_LOCATION,
            Remark = source.Remark
        FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail] target
        JOIN @Details source ON source.Id = target.Id
        WHERE target.DeliveryNoteId = @DeliveryNoteId;

        INSERT INTO [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
        (
            DeliveryNoteId,
            Seq,
            PackingGroupNo,
            DELIVERY_Line,
            Item_No,
            ProductId,
            Quantity,
            PackType,
            QuantityPerPack,
            DeliveryTime,
            TrolleyBox,
            DELIVERY,
            DELIVERY_LOCATION,
            Remark
        )
        SELECT
            @DeliveryNoteId,
            source.Seq,
            source.PackingGroupNo,
            source.DELIVERY_Line,
            source.Item_No,
            source.ProductId,
            source.Quantity,
            source.PackType,
            source.QuantityPerPack,
            source.DeliveryTime,
            source.TrolleyBox,
            source.DELIVERY,
            source.DELIVERY_LOCATION,
            source.Remark
        FROM @Details source
        WHERE ISNULL(source.Id, -1) = -1
           OR NOT EXISTS
           (
               SELECT 1
               FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail] target
               WHERE target.Id = source.Id
                 AND target.DeliveryNoteId = @DeliveryNoteId
           );

        UPDATE [nhvpa3en_vpa01].[CDV_DeliveryNote]
        SET
            TotalQuantity =
            (
                SELECT SUM(Quantity)
                FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
                WHERE DeliveryNoteId = @DeliveryNoteId
            ),
            UpdatedAt = GETDATE(),
            UpdatedBy = @UserId
        WHERE Id = @DeliveryNoteId;

        COMMIT TRAN;

        SELECT @DeliveryNoteId AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @DeliveryNoteId AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END;
GO
