SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleOut]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_RecycleOut]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RecycleOutNo] VARCHAR(30) NOT NULL,
        [SupplierId] INT NOT NULL,
        [ExportDate] DATETIME NOT NULL,
        [Status] VARCHAR(20) NOT NULL,
        [Note] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NULL CONSTRAINT [DF_CDV_RecycleOut_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CDV_RecycleOut] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_RecycleOut_RecycleOutNo] UNIQUE ([RecycleOutNo]),
        CONSTRAINT [FK_CDV_RecycleOut_Supplier]
            FOREIGN KEY ([SupplierId]) REFERENCES [nhvpa3en_vpa01].[CDV_Supplier] ([Id])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleOutDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_RecycleOutDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RecycleOutId] INT NOT NULL,
        [MaterialId] INT NOT NULL,
        [Qty] DECIMAL(18,2) NOT NULL,
        [UnitPrice] DECIMAL(18,2) NULL,
        [Note] NVARCHAR(500) NULL,
        CONSTRAINT [PK_CDV_RecycleOutDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_RecycleOutDetail_RecycleOut]
            FOREIGN KEY ([RecycleOutId]) REFERENCES [nhvpa3en_vpa01].[CDV_RecycleOut] ([Id]),
        CONSTRAINT [FK_CDV_RecycleOutDetail_Material]
            FOREIGN KEY ([MaterialId]) REFERENCES [nhvpa3en_vpa01].[CDV_Material] ([Id])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleOut_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOut_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOut_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ro.*,
        s.Code AS SupplierCode,
        s.Name AS SupplierName
    FROM [nhvpa3en_vpa01].[CDV_RecycleOut] ro
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Supplier] s ON s.Id = ro.SupplierId
    WHERE @Id = 0 OR ro.Id = @Id
    ORDER BY ro.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleOut_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOut_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOut_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @RecycleOutNo VARCHAR(30) = NULL,
    @SupplierId INT,
    @ExportDate DATETIME,
    @Status VARCHAR(20),
    @Note NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Supplier] WHERE Id = @SupplierId)
        BEGIN
            SELECT @Id AS ID, -2 AS ErrCode, 'SUPPLIER_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF @RecycleOutNo IS NOT NULL AND EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_RecycleOut]
            WHERE RecycleOutNo = @RecycleOutNo
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'RECYCLE_OUT_NO_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_RecycleOut]
            (
                RecycleOutNo,
                SupplierId,
                ExportDate,
                Status,
                Note,
                CreatedBy
            )
            VALUES
            (
                '',
                @SupplierId,
                @ExportDate,
                @Status,
                @Note,
                @UserId
            );

            SET @Id = SCOPE_IDENTITY();

            UPDATE [nhvpa3en_vpa01].[CDV_RecycleOut]
            SET RecycleOutNo = 'XTC-' + RIGHT('0000' + CAST(@Id AS VARCHAR(10)), 4)
            WHERE Id = @Id;
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_RecycleOut]
            SET
                RecycleOutNo = ISNULL(@RecycleOutNo, RecycleOutNo),
                SupplierId = @SupplierId,
                ExportDate = @ExportDate,
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleOut_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOut_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOut_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_RecycleOutDetail]
        WHERE RecycleOutId = @Id;

        DELETE FROM [nhvpa3en_vpa01].[CDV_RecycleOut]
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleOutDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOutDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOutDetail_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.*,
        ro.RecycleOutNo,
        m.Code AS MaterialCode,
        m.Name AS MaterialName
    FROM [nhvpa3en_vpa01].[CDV_RecycleOutDetail] d
    LEFT JOIN [nhvpa3en_vpa01].[CDV_RecycleOut] ro ON ro.Id = d.RecycleOutId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] m ON m.Id = d.MaterialId
    WHERE @Id = 0 OR d.Id = @Id
    ORDER BY d.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleOutDetail_SelectByRecycleOutId]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOutDetail_SelectByRecycleOutId] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOutDetail_SelectByRecycleOutId]
(
    @RecycleOutId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.*,
        m.Code AS MaterialCode,
        m.Name AS MaterialName
    FROM [nhvpa3en_vpa01].[CDV_RecycleOutDetail] d
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] m ON m.Id = d.MaterialId
    WHERE d.RecycleOutId = @RecycleOutId
    ORDER BY d.Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleOutDetail_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOutDetail_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOutDetail_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @RecycleOutId INT,
    @MaterialId INT,
    @Qty DECIMAL(18,2),
    @UnitPrice DECIMAL(18,2) = NULL,
    @Note NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_RecycleOut] WHERE Id = @RecycleOutId)
        BEGIN
            SELECT @Id AS ID, -2 AS ErrCode, 'RECYCLE_OUT_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Material] WHERE Id = @MaterialId)
        BEGIN
            SELECT @Id AS ID, -3 AS ErrCode, 'MATERIAL_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_RecycleOutDetail]
            (
                RecycleOutId,
                MaterialId,
                Qty,
                UnitPrice,
                Note
            )
            VALUES
            (
                @RecycleOutId,
                @MaterialId,
                @Qty,
                @UnitPrice,
                @Note
            );

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_RecycleOutDetail]
            SET
                RecycleOutId = @RecycleOutId,
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleOutDetail_BulkSave]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOutDetail_BulkSave] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOutDetail_BulkSave]
(
    @RecycleOutId INT,
    @UserId INT = 0,
    @DetailsJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_RecycleOut] WHERE Id = @RecycleOutId)
        BEGIN
            SELECT @RecycleOutId AS ID, -2 AS ErrCode, 'RECYCLE_OUT_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISJSON(@DetailsJson) <> 1
        BEGIN
            SELECT @RecycleOutId AS ID, -1 AS ErrCode, 'DETAILS_JSON_INVALID' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DECLARE @Details TABLE
        (
            Id INT NULL,
            MaterialId INT NOT NULL,
            Qty DECIMAL(18,2) NOT NULL,
            UnitPrice DECIMAL(18,2) NULL,
            Note NVARCHAR(500) NULL
        );

        INSERT INTO @Details
        (
            Id,
            MaterialId,
            Qty,
            UnitPrice,
            Note
        )
        SELECT
            Id,
            MaterialId,
            Qty,
            UnitPrice,
            Note
        FROM OPENJSON(@DetailsJson)
        WITH
        (
            Id INT '$.id',
            MaterialId INT '$.materialId',
            Qty DECIMAL(18,2) '$.qty',
            UnitPrice DECIMAL(18,2) '$.unitPrice',
            Note NVARCHAR(500) '$.note'
        );

        IF EXISTS (SELECT 1 FROM @Details WHERE MaterialId IS NULL OR Qty IS NULL)
        BEGIN
            SELECT @RecycleOutId AS ID, -3 AS ErrCode, 'DETAIL_MATERIAL_QTY_REQUIRED' AS ErrMsg;
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
            SELECT @RecycleOutId AS ID, -4 AS ErrCode, 'MATERIAL_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DELETE target
        FROM [nhvpa3en_vpa01].[CDV_RecycleOutDetail] target
        WHERE target.RecycleOutId = @RecycleOutId
          AND NOT EXISTS
          (
              SELECT 1
              FROM @Details source
              WHERE source.Id IS NOT NULL
                AND source.Id = target.Id
          );

        UPDATE target
        SET
            MaterialId = source.MaterialId,
            Qty = source.Qty,
            UnitPrice = source.UnitPrice,
            Note = source.Note
        FROM [nhvpa3en_vpa01].[CDV_RecycleOutDetail] target
        JOIN @Details source ON source.Id = target.Id
        WHERE target.RecycleOutId = @RecycleOutId;

        INSERT INTO [nhvpa3en_vpa01].[CDV_RecycleOutDetail]
        (
            RecycleOutId,
            MaterialId,
            Qty,
            UnitPrice,
            Note
        )
        SELECT
            @RecycleOutId,
            source.MaterialId,
            source.Qty,
            source.UnitPrice,
            source.Note
        FROM @Details source
        WHERE ISNULL(source.Id, -1) = -1
           OR NOT EXISTS
           (
               SELECT 1
               FROM [nhvpa3en_vpa01].[CDV_RecycleOutDetail] target
               WHERE target.Id = source.Id
                 AND target.RecycleOutId = @RecycleOutId
           );

        COMMIT TRAN;

        SELECT @RecycleOutId AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @RecycleOutId AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleOutDetail_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOutDetail_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleOutDetail_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_RecycleOutDetail]
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
