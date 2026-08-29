SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockIn]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_StockIn]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [StockInNo] VARCHAR(30) NOT NULL,
        [StockInDate] DATETIME NOT NULL,
        [SupplierId] INT NULL,
        [Manufacturerid] INT NULL,
        [PurchaseOrderId] INT NULL,
        [Status] VARCHAR(20) NOT NULL,
        [Note] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NULL CONSTRAINT [DF_CDV_StockIn_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CDV_StockIn] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_StockIn_StockInNo] UNIQUE ([StockInNo])
    );
END
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_StockIn', N'Manufacturerid') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_StockIn]
        ADD [Manufacturerid] INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CDV_StockIn_Supplier')
    AND OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Supplier]', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_StockIn]
        ADD CONSTRAINT [FK_CDV_StockIn_Supplier]
        FOREIGN KEY ([SupplierId]) REFERENCES [nhvpa3en_vpa01].[CDV_Supplier] ([Id]);
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockInDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_StockInDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [StockInId] INT NOT NULL,
        [Manufacturerid] INT NULL,
        [MaterialId] INT NOT NULL,
        [Qty] DECIMAL(18,2) NOT NULL,
        [UnitPrice] DECIMAL(18,2) NULL,
        [LotNo] NVARCHAR(100) NULL,
        [ExpiredDate] DATE NULL,
        [Note] NVARCHAR(500) NULL,
        CONSTRAINT [PK_CDV_StockInDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_StockInDetail_StockIn]
            FOREIGN KEY ([StockInId]) REFERENCES [nhvpa3en_vpa01].[CDV_StockIn] ([Id]),
        CONSTRAINT [FK_CDV_StockInDetail_Manufacturer]
            FOREIGN KEY ([Manufacturerid]) REFERENCES [nhvpa3en_vpa01].[CDV_Manufacturer] ([Id]),
        CONSTRAINT [FK_CDV_StockInDetail_Material]
            FOREIGN KEY ([MaterialId]) REFERENCES [nhvpa3en_vpa01].[CDV_Material] ([Id])
    );
END
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_StockInDetail', N'Manufacturerid') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_StockInDetail]
        ADD [Manufacturerid] INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CDV_StockInDetail_Manufacturer')
BEGIN
    IF NOT EXISTS
    (
        SELECT 1
        FROM [nhvpa3en_vpa01].[CDV_StockInDetail] d
        WHERE d.Manufacturerid IS NOT NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM [nhvpa3en_vpa01].[CDV_Manufacturer] manufacturer
              WHERE manufacturer.Id = d.Manufacturerid
          )
    )
    BEGIN
        ALTER TABLE [nhvpa3en_vpa01].[CDV_StockInDetail]
            ADD CONSTRAINT [FK_CDV_StockInDetail_Manufacturer]
            FOREIGN KEY ([Manufacturerid]) REFERENCES [nhvpa3en_vpa01].[CDV_Manufacturer] ([Id]);
    END
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockIn_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockIn_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockIn_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        si.*,
        s.Code AS SupplierCode,
        s.Name AS SupplierName
    FROM [nhvpa3en_vpa01].[CDV_StockIn] si
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Supplier] s ON s.Id = si.SupplierId
    WHERE @Id = 0 OR si.Id = @Id
    ORDER BY si.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockIn_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockIn_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockIn_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @StockInNo VARCHAR(30) = NULL,
    @StockInDate DATETIME,
    @SupplierId INT = NULL,
    @Manufacturerid INT = NULL,
    @PurchaseOrderId INT = NULL,
    @Status VARCHAR(20),
    @Note NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF @SupplierId IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Supplier] WHERE Id = @SupplierId)
        BEGIN
            SELECT @Id AS ID, -2 AS ErrCode, 'SUPPLIER_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF @StockInNo IS NOT NULL AND EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_StockIn]
            WHERE StockInNo = @StockInNo
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'STOCK_IN_NO_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_StockIn]
            (
                StockInNo,
                StockInDate,
                SupplierId,
                Manufacturerid,
                PurchaseOrderId,
                Status,
                Note,
                CreatedBy
            )
            VALUES
            (
                '',
                @StockInDate,
                @SupplierId,
                @Manufacturerid,
                @PurchaseOrderId,
                @Status,
                @Note,
                @UserId
            );

            SET @Id = SCOPE_IDENTITY();

            UPDATE [nhvpa3en_vpa01].[CDV_StockIn]
            SET StockInNo = 'PN-' + RIGHT('0000' + CAST(@Id AS VARCHAR(10)), 4)
            WHERE Id = @Id;
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_StockIn]
            SET
                StockInNo = ISNULL(@StockInNo, StockInNo),
                StockInDate = @StockInDate,
                SupplierId = @SupplierId,
                Manufacturerid = @Manufacturerid,
                PurchaseOrderId = @PurchaseOrderId,
                Status = @Status,
                Note = @Note,
                UpdatedAt = GETDATE(),
                UpdatedBy = @UserId
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockIn_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockIn_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockIn_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_StockInDetail]
        WHERE StockInId = @Id;

        DELETE FROM [nhvpa3en_vpa01].[CDV_StockIn]
        WHERE Id = @Id;

        COMMIT TRAN;

        SELECT @Id AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @Id AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockInDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.*,
        si.StockInNo,
        manufacturer.Code AS ManufacturerCode,
        manufacturer.Name AS ManufacturerName,
        m.Code AS MaterialCode,
        m.Name AS MaterialName
    FROM [nhvpa3en_vpa01].[CDV_StockInDetail] d
    LEFT JOIN [nhvpa3en_vpa01].[CDV_StockIn] si ON si.Id = d.StockInId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Manufacturer] manufacturer ON manufacturer.Id = d.Manufacturerid
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] m ON m.Id = d.MaterialId
    WHERE @Id = 0 OR d.Id = @Id
    ORDER BY d.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockInDetail_SelectByStockInId]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_SelectByStockInId] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_SelectByStockInId]
(
    @StockInId INT
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
    FROM [nhvpa3en_vpa01].[CDV_StockInDetail] d
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Manufacturer] manufacturer ON manufacturer.Id = d.Manufacturerid
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] m ON m.Id = d.MaterialId
    WHERE d.StockInId = @StockInId
    ORDER BY d.Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockInDetail_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @StockInId INT,
    @Manufacturerid INT = NULL,
    @MaterialId INT,
    @Qty DECIMAL(18,2),
    @UnitPrice DECIMAL(18,2) = NULL,
    @LotNo NVARCHAR(100) = NULL,
    @ExpiredDate DATE = NULL,
    @Note NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_StockIn] WHERE Id = @StockInId)
        BEGIN
            SELECT @Id AS ID, -2 AS ErrCode, 'STOCK_IN_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Material] WHERE Id = @MaterialId)
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
            INSERT INTO [nhvpa3en_vpa01].[CDV_StockInDetail]
            (
                StockInId,
                Manufacturerid,
                MaterialId,
                Qty,
                UnitPrice,
                LotNo,
                ExpiredDate,
                Note
            )
            VALUES
            (
                @StockInId,
                @Manufacturerid,
                @MaterialId,
                @Qty,
                @UnitPrice,
                @LotNo,
                @ExpiredDate,
                @Note
            );

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_StockInDetail]
            SET
                StockInId = @StockInId,
                Manufacturerid = @Manufacturerid,
                MaterialId = @MaterialId,
                Qty = @Qty,
                UnitPrice = @UnitPrice,
                LotNo = @LotNo,
                ExpiredDate = @ExpiredDate,
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockInDetail_BulkSave]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_BulkSave] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_BulkSave]
(
    @StockInId INT,
    @UserId INT = 0,
    @DetailsJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_StockIn] WHERE Id = @StockInId)
        BEGIN
            SELECT @StockInId AS ID, -2 AS ErrCode, 'STOCK_IN_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISJSON(@DetailsJson) <> 1
        BEGIN
            SELECT @StockInId AS ID, -1 AS ErrCode, 'DETAILS_JSON_INVALID' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DECLARE @Details TABLE
        (
            Id INT NULL,
            Manufacturerid INT NULL,
            MaterialId INT NULL,
            Qty DECIMAL(18,2) NULL,
            UnitPrice DECIMAL(18,2) NULL,
            LotNo NVARCHAR(100) NULL,
            ExpiredDate DATE NULL,
            Note NVARCHAR(500) NULL
        );

        INSERT INTO @Details
        (
            Id,
            Manufacturerid,
            MaterialId,
            Qty,
            UnitPrice,
            LotNo,
            ExpiredDate,
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
                TRY_CONVERT(INT, JSON_VALUE([value], '$.manufacturerID')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.Manufacturerid')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.ManufacturerId')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.ManufacturerID'))
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
                TRY_CONVERT(DECIMAL(18,2), JSON_VALUE([value], '$.unitPrice')),
                TRY_CONVERT(DECIMAL(18,2), JSON_VALUE([value], '$.UnitPrice'))
            ),
            COALESCE(
                JSON_VALUE([value], '$.lotNo'),
                JSON_VALUE([value], '$.LotNo')
            ),
            COALESCE(
                TRY_CONVERT(DATE, JSON_VALUE([value], '$.expiredDate')),
                TRY_CONVERT(DATE, JSON_VALUE([value], '$.ExpiredDate'))
            ),
            COALESCE(
                JSON_VALUE([value], '$.note'),
                JSON_VALUE([value], '$.Note')
            )
        FROM OPENJSON(@DetailsJson);

        IF EXISTS (SELECT 1 FROM @Details WHERE MaterialId IS NULL OR Qty IS NULL)
        BEGIN
            SELECT @StockInId AS ID, -3 AS ErrCode, 'DETAIL_MATERIAL_QTY_REQUIRED' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF EXISTS
        (
            SELECT 1
            FROM @Details d
            WHERE NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Material] m WHERE m.Id = d.MaterialId)
        )
        BEGIN
            SELECT @StockInId AS ID, -4 AS ErrCode, 'MATERIAL_NOT_FOUND' AS ErrMsg;
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
            SELECT @StockInId AS ID, -5 AS ErrCode, 'MANUFACTURER_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DELETE target
        FROM [nhvpa3en_vpa01].[CDV_StockInDetail] target
        WHERE target.StockInId = @StockInId
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
            UnitPrice = source.UnitPrice,
            LotNo = source.LotNo,
            ExpiredDate = source.ExpiredDate,
            Note = source.Note
        FROM [nhvpa3en_vpa01].[CDV_StockInDetail] target
        JOIN @Details source ON source.Id = target.Id
        WHERE target.StockInId = @StockInId;

        INSERT INTO [nhvpa3en_vpa01].[CDV_StockInDetail]
        (
            StockInId,
            Manufacturerid,
            MaterialId,
            Qty,
            UnitPrice,
            LotNo,
            ExpiredDate,
            Note
        )
        SELECT
            @StockInId,
            source.Manufacturerid,
            source.MaterialId,
            source.Qty,
            source.UnitPrice,
            source.LotNo,
            source.ExpiredDate,
            source.Note
        FROM @Details source
        WHERE ISNULL(source.Id, -1) = -1
           OR NOT EXISTS
           (
               SELECT 1
               FROM [nhvpa3en_vpa01].[CDV_StockInDetail] target
               WHERE target.Id = source.Id
                 AND target.StockInId = @StockInId
           );

        COMMIT TRAN;

        SELECT @StockInId AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @StockInId AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockInDetail_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_StockInDetail]
        WHERE Id = @Id;

        COMMIT TRAN;

        SELECT @Id AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @Id AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO
