SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Manufacturer]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_Manufacturer]
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
        [IsActive] BIT NOT NULL CONSTRAINT [DF_CDV_Manufacturer_IsActive] DEFAULT ((1)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_CDV_Manufacturer_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CDV_Manufacturer] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_CDV_Manufacturer_Code'
      AND object_id = OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Manufacturer]')
)
BEGIN
    CREATE UNIQUE INDEX [UX_CDV_Manufacturer_Code]
    ON [nhvpa3en_vpa01].[CDV_Manufacturer] ([Code])
    WHERE [Code] IS NOT NULL;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Manufacturer_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Manufacturer_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Manufacturer_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_Manufacturer]
    WHERE @Id = 0 OR Id = @Id
    ORDER BY Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Manufacturer_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Manufacturer_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Manufacturer_Upsert]
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
            FROM [nhvpa3en_vpa01].[CDV_Manufacturer]
            WHERE Code = @Code
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'MANUFACTURER_CODE_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_Manufacturer]
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
            UPDATE [nhvpa3en_vpa01].[CDV_Manufacturer]
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Manufacturer_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Manufacturer_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Manufacturer_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockIn]', N'U') IS NOT NULL
           AND EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_StockIn]
            WHERE Manufacturerid = @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'MANUFACTURER_IS_IN_USE' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOutDetail]', N'U') IS NOT NULL
           AND EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_StockOutDetail]
            WHERE Manufacturerid = @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'MANUFACTURER_IS_IN_USE' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DELETE FROM [nhvpa3en_vpa01].[CDV_Manufacturer]
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
