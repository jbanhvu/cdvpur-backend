SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleIn]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_RecycleIn]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RecycleInNo] VARCHAR(30) NOT NULL,
        [RecycleOutId] INT NOT NULL,
        [SupplierId] INT NOT NULL,
        [ImportDate] DATETIME NOT NULL,
        [Status] VARCHAR(20) NULL,
        [Note] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NULL CONSTRAINT [DF_CDV_RecycleIn_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        CONSTRAINT [PK_CDV_RecycleIn] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_RecycleIn_RecycleInNo] UNIQUE ([RecycleInNo]),
        CONSTRAINT [FK_CDV_RecycleIn_RecycleOut]
            FOREIGN KEY ([RecycleOutId]) REFERENCES [nhvpa3en_vpa01].[CDV_RecycleOut] ([Id]),
        CONSTRAINT [FK_CDV_RecycleIn_Supplier]
            FOREIGN KEY ([SupplierId]) REFERENCES [nhvpa3en_vpa01].[CDV_Supplier] ([Id])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleInDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_RecycleInDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RecycleInId] INT NOT NULL,
        [MaterialId] INT NOT NULL,
        [Qty] DECIMAL(18,2) NOT NULL,
        [UnitPrice] DECIMAL(18,2) NULL,
        [Note] NVARCHAR(500) NULL,
        CONSTRAINT [PK_CDV_RecycleInDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_RecycleInDetail_RecycleIn]
            FOREIGN KEY ([RecycleInId]) REFERENCES [nhvpa3en_vpa01].[CDV_RecycleIn] ([Id]),
        CONSTRAINT [FK_CDV_RecycleInDetail_Material]
            FOREIGN KEY ([MaterialId]) REFERENCES [nhvpa3en_vpa01].[CDV_Material] ([Id])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleIn_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleIn_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleIn_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ri.*,
        ro.RecycleOutNo,
        s.Code AS SupplierCode,
        s.Name AS SupplierName
    FROM [nhvpa3en_vpa01].[CDV_RecycleIn] ri
    LEFT JOIN [nhvpa3en_vpa01].[CDV_RecycleOut] ro ON ro.Id = ri.RecycleOutId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Supplier] s ON s.Id = ri.SupplierId
    WHERE @Id = 0 OR ri.Id = @Id
    ORDER BY ri.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleIn_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleIn_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleIn_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @RecycleInNo VARCHAR(30) = NULL,
    @RecycleOutId INT,
    @SupplierId INT,
    @ImportDate DATETIME,
    @Status VARCHAR(20) = NULL,
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

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Supplier] WHERE Id = @SupplierId)
        BEGIN
            SELECT @Id AS ID, -3 AS ErrCode, 'SUPPLIER_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF @RecycleInNo IS NOT NULL AND EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_RecycleIn]
            WHERE RecycleInNo = @RecycleInNo
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'RECYCLE_IN_NO_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_RecycleIn]
            (
                RecycleInNo,
                RecycleOutId,
                SupplierId,
                ImportDate,
                Status,
                Note,
                CreatedBy
            )
            VALUES
            (
                '',
                @RecycleOutId,
                @SupplierId,
                @ImportDate,
                @Status,
                @Note,
                @UserId
            );

            SET @Id = SCOPE_IDENTITY();

            UPDATE [nhvpa3en_vpa01].[CDV_RecycleIn]
            SET RecycleInNo = 'NTC-' + RIGHT('0000' + CAST(@Id AS VARCHAR(10)), 4)
            WHERE Id = @Id;
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_RecycleIn]
            SET
                RecycleInNo = ISNULL(@RecycleInNo, RecycleInNo),
                RecycleOutId = @RecycleOutId,
                SupplierId = @SupplierId,
                ImportDate = @ImportDate,
                Status = @Status,
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleIn_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleIn_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleIn_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_RecycleInDetail]
        WHERE RecycleInId = @Id;

        DELETE FROM [nhvpa3en_vpa01].[CDV_RecycleIn]
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleInDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleInDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleInDetail_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.*,
        ri.RecycleInNo,
        m.Code AS MaterialCode,
        m.Name AS MaterialName
    FROM [nhvpa3en_vpa01].[CDV_RecycleInDetail] d
    LEFT JOIN [nhvpa3en_vpa01].[CDV_RecycleIn] ri ON ri.Id = d.RecycleInId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] m ON m.Id = d.MaterialId
    WHERE @Id = 0 OR d.Id = @Id
    ORDER BY d.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleInDetail_SelectByRecycleInId]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleInDetail_SelectByRecycleInId] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleInDetail_SelectByRecycleInId]
(
    @RecycleInId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.*,
        m.Code AS MaterialCode,
        m.Name AS MaterialName
    FROM [nhvpa3en_vpa01].[CDV_RecycleInDetail] d
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] m ON m.Id = d.MaterialId
    WHERE d.RecycleInId = @RecycleInId
    ORDER BY d.Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleInDetail_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleInDetail_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleInDetail_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @RecycleInId INT,
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

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_RecycleIn] WHERE Id = @RecycleInId)
        BEGIN
            SELECT @Id AS ID, -2 AS ErrCode, 'RECYCLE_IN_NOT_FOUND' AS ErrMsg;
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
            INSERT INTO [nhvpa3en_vpa01].[CDV_RecycleInDetail]
            (
                RecycleInId,
                MaterialId,
                Qty,
                UnitPrice,
                Note
            )
            VALUES
            (
                @RecycleInId,
                @MaterialId,
                @Qty,
                @UnitPrice,
                @Note
            );

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_RecycleInDetail]
            SET
                RecycleInId = @RecycleInId,
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleInDetail_BulkSave]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleInDetail_BulkSave] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleInDetail_BulkSave]
(
    @RecycleInId INT,
    @UserId INT = 0,
    @DetailsJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_RecycleIn] WHERE Id = @RecycleInId)
        BEGIN
            SELECT @RecycleInId AS ID, -2 AS ErrCode, 'RECYCLE_IN_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISJSON(@DetailsJson) <> 1
        BEGIN
            SELECT @RecycleInId AS ID, -1 AS ErrCode, 'DETAILS_JSON_INVALID' AS ErrMsg;
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
            SELECT @RecycleInId AS ID, -3 AS ErrCode, 'DETAIL_MATERIAL_QTY_REQUIRED' AS ErrMsg;
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
            SELECT @RecycleInId AS ID, -4 AS ErrCode, 'MATERIAL_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DELETE target
        FROM [nhvpa3en_vpa01].[CDV_RecycleInDetail] target
        WHERE target.RecycleInId = @RecycleInId
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
        FROM [nhvpa3en_vpa01].[CDV_RecycleInDetail] target
        JOIN @Details source ON source.Id = target.Id
        WHERE target.RecycleInId = @RecycleInId;

        INSERT INTO [nhvpa3en_vpa01].[CDV_RecycleInDetail]
        (
            RecycleInId,
            MaterialId,
            Qty,
            UnitPrice,
            Note
        )
        SELECT
            @RecycleInId,
            source.MaterialId,
            source.Qty,
            source.UnitPrice,
            source.Note
        FROM @Details source
        WHERE ISNULL(source.Id, -1) = -1
           OR NOT EXISTS
           (
               SELECT 1
               FROM [nhvpa3en_vpa01].[CDV_RecycleInDetail] target
               WHERE target.Id = source.Id
                 AND target.RecycleInId = @RecycleInId
           );

        COMMIT TRAN;

        SELECT @RecycleInId AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @RecycleInId AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RecycleInDetail_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleInDetail_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_RecycleInDetail_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_RecycleInDetail]
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
