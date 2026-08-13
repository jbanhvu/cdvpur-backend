SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOut]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_StockOut]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [StockOutNo] VARCHAR(30) NULL,
        [StockOutDate] DATETIME NULL,
        [StockOutTypeID] INT NULL,
        [DepartmentId] INT NULL,
        [RequestId] INT NULL,
        [Status] VARCHAR(20) NULL,
        [Note] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NULL CONSTRAINT [DF_CDV_StockOut_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CDV_StockOut] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_StockOut', N'StockOutTypeID') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_StockOut]
        ADD [StockOutTypeID] INT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_CDV_StockOut_StockOutNo'
      AND object_id = OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOut]')
)
BEGIN
    CREATE UNIQUE INDEX [UX_CDV_StockOut_StockOutNo]
    ON [nhvpa3en_vpa01].[CDV_StockOut] ([StockOutNo])
    WHERE [StockOutNo] IS NOT NULL;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOutDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_StockOutDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [StockOutId] INT NULL,
        [SupplierId] INT NULL,
        [MaterialId] INT NULL,
        [Qty] DECIMAL(18,2) NULL,
        [UnitPrice] DECIMAL(18,2) NULL,
        [Note] NVARCHAR(500) NULL,
        CONSTRAINT [PK_CDV_StockOutDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_StockOutDetail_StockOut]
            FOREIGN KEY ([StockOutId]) REFERENCES [nhvpa3en_vpa01].[CDV_StockOut] ([Id]),
        CONSTRAINT [FK_CDV_StockOutDetail_Material]
            FOREIGN KEY ([MaterialId]) REFERENCES [nhvpa3en_vpa01].[CDV_Material] ([Id])
    );
END
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_StockOutDetail', N'SupplierId') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_StockOutDetail]
        ADD [SupplierId] INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CDV_StockOutDetail_Supplier')
    AND OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Supplier]', N'U') IS NOT NULL
    AND NOT EXISTS
    (
        SELECT 1
        FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] d
        WHERE d.SupplierId IS NOT NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM [nhvpa3en_vpa01].[CDV_Supplier] s
              WHERE s.Id = d.SupplierId
          )
    )
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_StockOutDetail]
        ADD CONSTRAINT [FK_CDV_StockOutDetail_Supplier]
        FOREIGN KEY ([SupplierId]) REFERENCES [nhvpa3en_vpa01].[CDV_Supplier] ([Id]);
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOut_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockOut_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockOut_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT so.*
    FROM [nhvpa3en_vpa01].[CDV_StockOut] so
    WHERE @Id = 0 OR so.Id = @Id
    ORDER BY so.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOut_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockOut_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockOut_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @StockOutNo VARCHAR(30) = NULL,
    @StockOutDate DATETIME = NULL,
    @StockOutTypeID INT = NULL,
    @DepartmentId INT = NULL,
    @RequestId INT = NULL,
    @Status VARCHAR(20) = NULL,
    @Note NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF @StockOutNo IS NOT NULL AND EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_StockOut]
            WHERE StockOutNo = @StockOutNo
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'STOCK_OUT_NO_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_StockOut]
            (
                StockOutNo,
                StockOutDate,
                StockOutTypeID,
                DepartmentId,
                RequestId,
                Status,
                Note,
                CreatedBy
            )
            VALUES
            (
                '',
                @StockOutDate,
                @StockOutTypeID,
                @DepartmentId,
                @RequestId,
                @Status,
                @Note,
                @UserId
            );

            SET @Id = SCOPE_IDENTITY();

            UPDATE [nhvpa3en_vpa01].[CDV_StockOut]
            SET StockOutNo = 'PX-' + RIGHT('0000' + CAST(@Id AS VARCHAR(10)), 4)
            WHERE Id = @Id;
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_StockOut]
            SET
                StockOutNo = ISNULL(@StockOutNo, StockOutNo),
                StockOutDate = @StockOutDate,
                StockOutTypeID = @StockOutTypeID,
                DepartmentId = @DepartmentId,
                RequestId = @RequestId,
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOut_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockOut_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockOut_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_StockOutDetail]
        WHERE StockOutId = @Id;

        DELETE FROM [nhvpa3en_vpa01].[CDV_StockOut]
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
        s.Code AS SupplierCode,
        s.Name AS SupplierName,
        m.Code AS MaterialCode,
        m.Name AS MaterialName
    FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] d
    LEFT JOIN [nhvpa3en_vpa01].[CDV_StockOut] so ON so.Id = d.StockOutId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Supplier] s ON s.Id = d.SupplierId
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
        s.Code AS SupplierCode,
        s.Name AS SupplierName,
        m.Code AS MaterialCode,
        m.Name AS MaterialName
    FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] d
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Supplier] s ON s.Id = d.SupplierId
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
    @SupplierId INT = NULL,
    @MaterialId INT = NULL,
    @Qty DECIMAL(18,2) = NULL,
    @UnitPrice DECIMAL(18,2) = NULL,
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

        IF @SupplierId IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Supplier] WHERE Id = @SupplierId)
        BEGIN
            SELECT @Id AS ID, -4 AS ErrCode, 'SUPPLIER_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_StockOutDetail]
            (
                StockOutId,
                SupplierId,
                MaterialId,
                Qty,
                UnitPrice,
                Note
            )
            VALUES
            (
                @StockOutId,
                @SupplierId,
                @MaterialId,
                @Qty,
                @UnitPrice,
                @Note
            );

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_StockOutDetail]
            SET
                StockOutId = @StockOutId,
                SupplierId = @SupplierId,
                MaterialId = @MaterialId,
                Qty = @Qty,
                UnitPrice = @UnitPrice,
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
            SupplierId INT NULL,
            MaterialId INT NULL,
            Qty DECIMAL(18,2) NULL,
            UnitPrice DECIMAL(18,2) NULL,
            Note NVARCHAR(500) NULL
        );

        INSERT INTO @Details
        (
            Id,
            SupplierId,
            MaterialId,
            Qty,
            UnitPrice,
            Note
        )
        SELECT
            Id,
            SupplierId,
            MaterialId,
            Qty,
            UnitPrice,
            Note
        FROM OPENJSON(@DetailsJson)
        WITH
        (
            Id INT '$.id',
            SupplierId INT '$.supplierId',
            MaterialId INT '$.materialId',
            Qty DECIMAL(18,2) '$.qty',
            UnitPrice DECIMAL(18,2) '$.unitPrice',
            Note NVARCHAR(500) '$.note'
        );

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
            WHERE d.SupplierId IS NOT NULL
              AND NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Supplier] s WHERE s.Id = d.SupplierId)
        )
        BEGIN
            SELECT @StockOutId AS ID, -4 AS ErrCode, 'SUPPLIER_NOT_FOUND' AS ErrMsg;
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
            SupplierId = source.SupplierId,
            MaterialId = source.MaterialId,
            Qty = source.Qty,
            UnitPrice = source.UnitPrice,
            Note = source.Note
        FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] target
        JOIN @Details source ON source.Id = target.Id
        WHERE target.StockOutId = @StockOutId;

        INSERT INTO [nhvpa3en_vpa01].[CDV_StockOutDetail]
        (
            StockOutId,
            SupplierId,
            MaterialId,
            Qty,
            UnitPrice,
            Note
        )
        SELECT
            @StockOutId,
            source.SupplierId,
            source.MaterialId,
            source.Qty,
            source.UnitPrice,
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOutDetail_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_StockOutDetail]
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
