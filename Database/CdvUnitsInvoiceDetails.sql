SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Unit]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_Unit]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Code] VARCHAR(50) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_CDV_Unit_IsActive] DEFAULT ((1)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_CDV_Unit_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CDV_Unit] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_Unit_Code] UNIQUE ([Code])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_InvoiceDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_InvoiceDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [InvoiceId] INT NOT NULL,
        [MaterialId] INT NOT NULL,
        [UnitId] INT NOT NULL,
        [Quantity] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_InvoiceDetail_Quantity] DEFAULT ((0)),
        [UnitPrice] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_InvoiceDetail_UnitPrice] DEFAULT ((0)),
        [TaxAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_InvoiceDetail_TaxAmount] DEFAULT ((0)),
        [DiscountAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_InvoiceDetail_DiscountAmount] DEFAULT ((0)),
        [TotalAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_InvoiceDetail_TotalAmount] DEFAULT ((0)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_CDV_InvoiceDetail_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CDV_InvoiceDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_InvoiceDetail_Invoice]
            FOREIGN KEY ([InvoiceId]) REFERENCES [nhvpa3en_vpa01].[CDV_Invoice] ([Id]),
        CONSTRAINT [FK_CDV_InvoiceDetail_Material]
            FOREIGN KEY ([MaterialId]) REFERENCES [nhvpa3en_vpa01].[CDV_Material] ([Id]),
        CONSTRAINT [FK_CDV_InvoiceDetail_Unit]
            FOREIGN KEY ([UnitId]) REFERENCES [nhvpa3en_vpa01].[CDV_Unit] ([Id])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Unit_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Unit_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Unit_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_Unit]
    WHERE @Id = 0 OR Id = @Id
    ORDER BY Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Unit_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Unit_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Unit_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @Code VARCHAR(50),
    @Name NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @IsActive BIT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_Unit]
            WHERE Code = @Code
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'UNIT_CODE_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_Unit]
            (
                Code,
                Name,
                Description,
                IsActive,
                CreatedBy
            )
            VALUES
            (
                @Code,
                @Name,
                @Description,
                ISNULL(@IsActive, 1),
                @UserId
            );

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_Unit]
            SET
                Code = @Code,
                Name = @Name,
                Description = @Description,
                IsActive = ISNULL(@IsActive, IsActive),
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Unit_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Unit_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Unit_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_InvoiceDetail]
            WHERE UnitId = @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'UNIT_IS_IN_USE' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DELETE FROM [nhvpa3en_vpa01].[CDV_Unit]
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_InvoiceDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_InvoiceDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_InvoiceDetail_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.*,
        i.InvoiceNo,
        m.Code AS MaterialCode,
        m.Name AS MaterialName,
        u.Code AS UnitCode,
        u.Name AS UnitName
    FROM [nhvpa3en_vpa01].[CDV_InvoiceDetail] d
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Invoice] i ON i.Id = d.InvoiceId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] m ON m.Id = d.MaterialId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Unit] u ON u.Id = d.UnitId
    WHERE @Id = 0 OR d.Id = @Id
    ORDER BY d.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_InvoiceDetail_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_InvoiceDetail_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_InvoiceDetail_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @InvoiceId INT,
    @MaterialId INT,
    @UnitId INT,
    @Quantity DECIMAL(18,2),
    @UnitPrice DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2) = NULL,
    @DiscountAmount DECIMAL(18,2) = NULL,
    @TotalAmount DECIMAL(18,2)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Invoice] WHERE Id = @InvoiceId)
        BEGIN
            SELECT @Id AS ID, -2 AS ErrCode, 'INVOICE_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Material] WHERE Id = @MaterialId)
        BEGIN
            SELECT @Id AS ID, -3 AS ErrCode, 'MATERIAL_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Unit] WHERE Id = @UnitId)
        BEGIN
            SELECT @Id AS ID, -4 AS ErrCode, 'UNIT_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_InvoiceDetail]
            (
                InvoiceId,
                MaterialId,
                UnitId,
                Quantity,
                UnitPrice,
                TaxAmount,
                DiscountAmount,
                TotalAmount,
                CreatedBy
            )
            VALUES
            (
                @InvoiceId,
                @MaterialId,
                @UnitId,
                @Quantity,
                @UnitPrice,
                ISNULL(@TaxAmount, 0),
                ISNULL(@DiscountAmount, 0),
                @TotalAmount,
                @UserId
            );

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_InvoiceDetail]
            SET
                InvoiceId = @InvoiceId,
                MaterialId = @MaterialId,
                UnitId = @UnitId,
                Quantity = @Quantity,
                UnitPrice = @UnitPrice,
                TaxAmount = ISNULL(@TaxAmount, 0),
                DiscountAmount = ISNULL(@DiscountAmount, 0),
                TotalAmount = @TotalAmount,
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_InvoiceDetail_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_InvoiceDetail_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_InvoiceDetail_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_InvoiceDetail]
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
