SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Supplier]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_Supplier]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Code] VARCHAR(50) NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Phone] VARCHAR(50) NULL,
        [Email] VARCHAR(100) NULL,
        [Address] NVARCHAR(500) NULL,
        [TaxCode] VARCHAR(50) NULL,
        [ContactPerson] NVARCHAR(200) NULL,
        [BankAccount] VARCHAR(100) NULL,
        [BankName] NVARCHAR(200) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_CDV_Supplier_IsActive] DEFAULT ((1)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_CDV_Supplier_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CDV_Supplier] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_CDV_Supplier_Code'
      AND object_id = OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Supplier]')
)
BEGIN
    CREATE UNIQUE INDEX [UX_CDV_Supplier_Code]
    ON [nhvpa3en_vpa01].[CDV_Supplier] ([Code])
    WHERE [Code] IS NOT NULL;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Invoice]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_Invoice]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [InvoiceNo] VARCHAR(50) NOT NULL,
        [SupplierId] INT NOT NULL,
        [InvoiceDate] DATE NOT NULL,
        [TotalAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_Invoice_TotalAmount] DEFAULT ((0)),
        [TaxAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_Invoice_TaxAmount] DEFAULT ((0)),
        [DiscountAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_Invoice_DiscountAmount] DEFAULT ((0)),
        [FinalAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_Invoice_FinalAmount] DEFAULT ((0)),
        [Status] VARCHAR(50) NOT NULL CONSTRAINT [DF_CDV_Invoice_Status] DEFAULT ('DRAFT'),
        [Note] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_CDV_Invoice_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CDV_Invoice] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_Invoice_InvoiceNo] UNIQUE ([InvoiceNo]),
        CONSTRAINT [FK_CDV_Invoice_Supplier]
            FOREIGN KEY ([SupplierId]) REFERENCES [nhvpa3en_vpa01].[CDV_Supplier] ([Id])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Supplier_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Supplier_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Supplier_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_Supplier]
    WHERE @Id = 0 OR Id = @Id
    ORDER BY Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Supplier_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Supplier_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Supplier_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @Name NVARCHAR(200),
    @Code VARCHAR(50) = NULL,
    @Phone VARCHAR(50) = NULL,
    @Email VARCHAR(100) = NULL,
    @Address NVARCHAR(500) = NULL,
    @TaxCode VARCHAR(50) = NULL,
    @ContactPerson NVARCHAR(200) = NULL,
    @BankAccount VARCHAR(100) = NULL,
    @BankName NVARCHAR(200) = NULL,
    @IsActive BIT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF @Code IS NOT NULL AND EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_Supplier]
            WHERE Code = @Code
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'SUPPLIER_CODE_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_Supplier]
            (
                Code,
                Name,
                Phone,
                Email,
                Address,
                TaxCode,
                ContactPerson,
                BankAccount,
                BankName,
                IsActive,
                CreatedBy
            )
            VALUES
            (
                @Code,
                @Name,
                @Phone,
                @Email,
                @Address,
                @TaxCode,
                @ContactPerson,
                @BankAccount,
                @BankName,
                ISNULL(@IsActive, 1),
                @UserId
            );

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_Supplier]
            SET
                Code = @Code,
                Name = @Name,
                Phone = @Phone,
                Email = @Email,
                Address = @Address,
                TaxCode = @TaxCode,
                ContactPerson = @ContactPerson,
                BankAccount = @BankAccount,
                BankName = @BankName,
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Supplier_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Supplier_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Supplier_Delete]
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
            FROM [nhvpa3en_vpa01].[CDV_Invoice]
            WHERE SupplierId = @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'SUPPLIER_IS_IN_USE' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DELETE FROM [nhvpa3en_vpa01].[CDV_Supplier]
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Invoice_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Invoice_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Invoice_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        i.*,
        s.Code AS SupplierCode,
        s.Name AS SupplierName
    FROM [nhvpa3en_vpa01].[CDV_Invoice] i
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Supplier] s ON s.Id = i.SupplierId
    WHERE @Id = 0 OR i.Id = @Id
    ORDER BY i.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Invoice_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Invoice_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Invoice_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @InvoiceNo VARCHAR(50),
    @SupplierId INT,
    @InvoiceDate DATE,
    @TotalAmount DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2) = NULL,
    @DiscountAmount DECIMAL(18,2) = NULL,
    @FinalAmount DECIMAL(18,2),
    @Status VARCHAR(50) = NULL,
    @Note NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_Supplier]
            WHERE Id = @SupplierId
        )
        BEGIN
            SELECT @Id AS ID, -2 AS ErrCode, 'SUPPLIER_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_Invoice]
            WHERE InvoiceNo = @InvoiceNo
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'INVOICE_NO_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_Invoice]
            (
                InvoiceNo,
                SupplierId,
                InvoiceDate,
                TotalAmount,
                TaxAmount,
                DiscountAmount,
                FinalAmount,
                Status,
                Note,
                CreatedBy
            )
            VALUES
            (
                @InvoiceNo,
                @SupplierId,
                @InvoiceDate,
                @TotalAmount,
                ISNULL(@TaxAmount, 0),
                ISNULL(@DiscountAmount, 0),
                @FinalAmount,
                ISNULL(@Status, 'DRAFT'),
                @Note,
                @UserId
            );

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_Invoice]
            SET
                InvoiceNo = @InvoiceNo,
                SupplierId = @SupplierId,
                InvoiceDate = @InvoiceDate,
                TotalAmount = @TotalAmount,
                TaxAmount = ISNULL(@TaxAmount, 0),
                DiscountAmount = ISNULL(@DiscountAmount, 0),
                FinalAmount = @FinalAmount,
                Status = ISNULL(@Status, Status),
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Invoice_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Invoice_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Invoice_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_Invoice]
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
