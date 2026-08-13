SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Departmant]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_Departmant]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Code] VARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_CDV_Departmant_IsActive] DEFAULT ((1)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_CDV_Departmant_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CDV_Departmant] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_Departmant_Code] UNIQUE ([Code])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CDV_StockOut_Departmant')
    AND OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOut]', N'U') IS NOT NULL
    AND NOT EXISTS
    (
        SELECT 1
        FROM [nhvpa3en_vpa01].[CDV_StockOut] so
        WHERE so.DepartmentId IS NOT NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM [nhvpa3en_vpa01].[CDV_Departmant] d
              WHERE d.Id = so.DepartmentId
          )
    )
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_StockOut]
        ADD CONSTRAINT [FK_CDV_StockOut_Departmant]
        FOREIGN KEY ([DepartmentId]) REFERENCES [nhvpa3en_vpa01].[CDV_Departmant] ([Id]);
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Departmant_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Departmant_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Departmant_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_Departmant]
    WHERE @Id = 0 OR Id = @Id
    ORDER BY Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Departmant_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Departmant_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Departmant_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @Code VARCHAR(50),
    @Name NVARCHAR(200),
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
            FROM [nhvpa3en_vpa01].[CDV_Departmant]
            WHERE Code = @Code
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'DEPARTMANT_CODE_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_Departmant]
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
            UPDATE [nhvpa3en_vpa01].[CDV_Departmant]
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Departmant_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Departmant_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Departmant_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOut]', N'U') IS NOT NULL
           AND EXISTS
           (
               SELECT 1
               FROM [nhvpa3en_vpa01].[CDV_StockOut]
               WHERE DepartmentId = @Id
           )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'DEPARTMANT_IS_IN_USE' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DELETE FROM [nhvpa3en_vpa01].[CDV_Departmant]
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
