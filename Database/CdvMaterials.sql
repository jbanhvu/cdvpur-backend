SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_MaterialType]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_MaterialType]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Code] VARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_CDV_MaterialType_IsActive] DEFAULT ((1)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_CDV_MaterialType_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CDV_MaterialType] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_MaterialType_Code] UNIQUE ([Code])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Material]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_Material]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [MaterialTypeId] INT NOT NULL,
        [Code] VARCHAR(50) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Unit] NVARCHAR(50) NULL,
        [Specification] NVARCHAR(500) NULL,
        [Description] NVARCHAR(500) NULL,
        [DefaultPrice] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_Material_DefaultPrice] DEFAULT ((0)),
        [MinimumStock] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_Material_MinimumStock] DEFAULT ((0)),
        [CurrentStock] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_Material_CurrentStock] DEFAULT ((0)),
        [IsActive] BIT NOT NULL CONSTRAINT [DF_CDV_Material_IsActive] DEFAULT ((1)),
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_CDV_Material_CreatedAt] DEFAULT (GETDATE()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_CDV_Material] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_Material_Code] UNIQUE ([Code]),
        CONSTRAINT [FK_CDV_Material_MaterialType]
            FOREIGN KEY ([MaterialTypeId]) REFERENCES [nhvpa3en_vpa01].[CDV_MaterialType] ([Id])
    );
END
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_Material', N'DefaultPrice') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_Material]
    ADD [DefaultPrice] DECIMAL(18,2) NOT NULL
        CONSTRAINT [DF_CDV_Material_DefaultPrice] DEFAULT ((0)) WITH VALUES;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_MaterialType_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_MaterialType_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_MaterialType_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_MaterialType]
    WHERE @Id = 0 OR Id = @Id
    ORDER BY Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_MaterialType_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_MaterialType_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_MaterialType_Upsert]
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
            FROM [nhvpa3en_vpa01].[CDV_MaterialType]
            WHERE Code = @Code
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'MATERIAL_TYPE_CODE_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_MaterialType]
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
            UPDATE [nhvpa3en_vpa01].[CDV_MaterialType]
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_MaterialType_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_MaterialType_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_MaterialType_Delete]
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
            FROM [nhvpa3en_vpa01].[CDV_Material]
            WHERE MaterialTypeId = @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'MATERIAL_TYPE_IS_IN_USE' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        DELETE FROM [nhvpa3en_vpa01].[CDV_MaterialType]
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Material_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Material_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Material_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.*,
        mt.Code AS MaterialTypeCode,
        mt.Name AS MaterialTypeName
    FROM [nhvpa3en_vpa01].[CDV_Material] m
    LEFT JOIN [nhvpa3en_vpa01].[CDV_MaterialType] mt ON mt.Id = m.MaterialTypeId
    WHERE @Id = 0 OR m.Id = @Id
    ORDER BY m.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Material_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Material_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Material_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @MaterialTypeId INT,
    @Code VARCHAR(50),
    @Name NVARCHAR(200),
    @Unit NVARCHAR(50) = NULL,
    @Specification NVARCHAR(500) = NULL,
    @Description NVARCHAR(500) = NULL,
    @DefaultPrice DECIMAL(18,2) = 0,
    @MinimumStock DECIMAL(18,2) = 0,
    @CurrentStock DECIMAL(18,2) = 0,
    @IsActive BIT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_MaterialType]
            WHERE Id = @MaterialTypeId
        )
        BEGIN
            SELECT @Id AS ID, -2 AS ErrCode, 'MATERIAL_TYPE_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_Material]
            WHERE Code = @Code
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'MATERIAL_CODE_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF ISNULL(@Id, -1) = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_Material]
            (
                MaterialTypeId,
                Code,
                Name,
                Unit,
                Specification,
                Description,
                DefaultPrice,
                MinimumStock,
                CurrentStock,
                IsActive,
                CreatedBy
            )
            VALUES
            (
                @MaterialTypeId,
                @Code,
                @Name,
                @Unit,
                @Specification,
                @Description,
                ISNULL(@DefaultPrice, 0),
                @MinimumStock,
                @CurrentStock,
                ISNULL(@IsActive, 1),
                @UserId
            );

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_Material]
            SET
                MaterialTypeId = @MaterialTypeId,
                Code = @Code,
                Name = @Name,
                Unit = @Unit,
                Specification = @Specification,
                Description = @Description,
                DefaultPrice = ISNULL(@DefaultPrice, 0),
                MinimumStock = @MinimumStock,
                CurrentStock = @CurrentStock,
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Material_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Material_Delete] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Material_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_Material]
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
