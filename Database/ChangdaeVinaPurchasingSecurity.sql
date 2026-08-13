SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Role]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_Role]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        CONSTRAINT [PK_CDV_Role] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Function]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_Function]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Code] VARCHAR(10) NOT NULL,
        [ParentId] INT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Module] VARCHAR(100) NULL,
        [Route] VARCHAR(300) NULL,
        [Icon] VARCHAR(100) NULL,
        [SortOrder] INT NOT NULL CONSTRAINT [DF_CDV_Function_SortOrder] DEFAULT ((0)),
        [FeatureType] VARCHAR(50) NOT NULL CONSTRAINT [DF_CDV_Function_FeatureType] DEFAULT ('MENU'),
        [IsMenu] BIT NOT NULL CONSTRAINT [DF_CDV_Function_IsMenu] DEFAULT ((1)),
        [IsActive] BIT NOT NULL CONSTRAINT [DF_CDV_Function_IsActive] DEFAULT ((1)),
        CONSTRAINT [PK_CDV_Function] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_User]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_User]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [BranchId] INT NULL,
        [RoleId] INT NULL,
        [Username] VARCHAR(100) NOT NULL,
        [PasswordHash] VARCHAR(500) NOT NULL,
        [FullName] NVARCHAR(200) NULL,
        [Phone] VARCHAR(20) NULL,
        [Email] VARCHAR(100) NULL,
        [IsActive] BIT NULL CONSTRAINT [DF_CDV_User_IsActive] DEFAULT ((1)),
        [CreatedAt] DATETIME NULL CONSTRAINT [DF_CDV_User_CreatedAt] DEFAULT (GETDATE()),
        [RoleName] VARCHAR(100) NOT NULL CONSTRAINT [DF_CDV_User_RoleName] DEFAULT ('Staff'),
        [AvatarUrl] NVARCHAR(500) NULL,
        CONSTRAINT [PK_CDV_User] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_User_Username] UNIQUE ([Username])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RoleFunctionPermission]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RoleId] INT NOT NULL,
        [FunctionId] INT NOT NULL,
        [ListPermission] VARCHAR(100) NULL,
        [ListApprovalRole] NVARCHAR(100) NULL,
        CONSTRAINT [PK_CDV_RoleFunctionPermission] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CDV_User_Branch')
    AND OBJECT_ID(N'[nhvpa3en_vpa01].[Sol_Branch]', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User]
        ADD CONSTRAINT [FK_CDV_User_Branch]
        FOREIGN KEY ([BranchId]) REFERENCES [nhvpa3en_vpa01].[Sol_Branch] ([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CDV_User_Role')
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User]
        ADD CONSTRAINT [FK_CDV_User_Role]
        FOREIGN KEY ([RoleId]) REFERENCES [nhvpa3en_vpa01].[CDV_Role] ([Id]);
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Function_Select]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_Function_Select];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Function_Select]
(
    @Id INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT f.*
    FROM [nhvpa3en_vpa01].[CDV_Function] f
    WHERE (@Id = 0 OR @Id IS NULL OR f.Id = @Id)
    ORDER BY f.SortOrder, f.Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Function_Upsert]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_Function_Upsert];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Function_Upsert]
(
    @Id INT = NULL,
    @Code VARCHAR(10),
    @ParentId INT = NULL,
    @Name NVARCHAR(100),
    @Module VARCHAR(100) = NULL,
    @Route VARCHAR(300) = NULL,
    @Icon VARCHAR(100) = NULL,
    @SortOrder INT = 0,
    @FeatureType VARCHAR(50) = 'MENU',
    @IsMenu BIT = 1,
    @IsActive BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, 0) = 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_Function]
        (
            Code, ParentId, Name, Module, Route, Icon,
            SortOrder, FeatureType, IsMenu, IsActive
        )
        VALUES
        (
            @Code, @ParentId, @Name, @Module, @Route, @Icon,
            @SortOrder, @FeatureType, @IsMenu, @IsActive
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_Function]
        SET
            Code = @Code,
            ParentId = @ParentId,
            Name = @Name,
            Module = @Module,
            Route = @Route,
            Icon = @Icon,
            SortOrder = @SortOrder,
            FeatureType = @FeatureType,
            IsMenu = @IsMenu,
            IsActive = @IsActive
        WHERE Id = @Id;
    END

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_Function]
    WHERE Id = @Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Role_Select]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_Role_Select];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Role_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.*
    FROM [nhvpa3en_vpa01].[CDV_Role] r
    WHERE @Id = 0 OR @Id IS NULL OR r.Id = @Id
    ORDER BY r.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Role_Upsert]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_Role_Upsert];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Role_Upsert]
(
    @Id INT = -1,
    @Name NVARCHAR(100),
    @Description NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_Role]
            WHERE Name = @Name
              AND Id <> @Id
        )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'ROLE_NAME_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF @Id = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_Role] (Name, Description)
            VALUES (@Name, @Description);

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_Role]
            SET Name = @Name,
                Description = @Description
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

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RoleFunctionPermission_Select]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_RoleFunctionPermission_Select];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RoleFunctionPermission_Select]
(
    @Id INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT rfp.*
    FROM [nhvpa3en_vpa01].[CDV_RoleFunctionPermission] rfp
    WHERE (@Id IS NULL OR rfp.Id = @Id);
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RoleFunctionPermission_SelectByRoleID]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_RoleFunctionPermission_SelectByRoleID];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RoleFunctionPermission_SelectByRoleID]
(
    @RoleId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT rfp.*
    FROM [nhvpa3en_vpa01].[CDV_RoleFunctionPermission] rfp
    WHERE rfp.RoleId = @RoleId;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RoleFunctionPermission_SelectByUserID]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_RoleFunctionPermission_SelectByUserID];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RoleFunctionPermission_SelectByUserID]
(
    @UserID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        RFP.FunctionId AS Id,
        F.Module,
        F.Name,
        F.Icon,
        F.Route,
        F.ParentId,
        RFP.ListPermission
    FROM [nhvpa3en_vpa01].[CDV_RoleFunctionPermission] RFP
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Role] R ON R.Id = RFP.RoleId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_User] U ON U.RoleId = R.Id
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Function] F ON F.Id = RFP.FunctionId
    WHERE U.Id = @UserID;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_RoleFunctionPermission_Upsert]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_RoleFunctionPermission_Upsert];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_RoleFunctionPermission_Upsert]
(
    @Id INT = NULL,
    @RoleId INT,
    @FunctionId INT,
    @ListPermission VARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, 0) = 0
    BEGIN
        IF EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
            WHERE RoleId = @RoleId
              AND FunctionId = @FunctionId
        )
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
            SET ListPermission = @ListPermission
            WHERE RoleId = @RoleId
              AND FunctionId = @FunctionId;

            SELECT *
            FROM [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
            WHERE RoleId = @RoleId
              AND FunctionId = @FunctionId;

            RETURN;
        END

        INSERT INTO [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
        (
            RoleId,
            FunctionId,
            ListPermission
        )
        VALUES
        (
            @RoleId,
            @FunctionId,
            @ListPermission
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
        SET
            RoleId = @RoleId,
            FunctionId = @FunctionId,
            ListPermission = @ListPermission
        WHERE Id = @Id;
    END

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
    WHERE Id = @Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_User_Select]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_User_Select];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_User_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id = 0
        SELECT * FROM [nhvpa3en_vpa01].[CDV_User] ORDER BY Id DESC;
    ELSE
        SELECT * FROM [nhvpa3en_vpa01].[CDV_User] WHERE Id = @Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_User_Login]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_User_Login];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_User_Login]
(
    @Username VARCHAR(100),
    @PasswordHash VARCHAR(500)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF NOT EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_User]
            WHERE Username = @Username
        )
        BEGIN
            SELECT CAST(0 AS BIT) AS IsSuccess, 404 AS ErrCode, 'Username does not exist' AS ErrMsg;
            RETURN;
        END

        IF NOT EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_User]
            WHERE Username = @Username
              AND PasswordHash = @PasswordHash
        )
        BEGIN
            SELECT CAST(0 AS BIT) AS IsSuccess, 401 AS ErrCode, 'Wrong password' AS ErrMsg;
            RETURN;
        END

        IF EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_User]
            WHERE Username = @Username
              AND ISNULL(IsActive, 0) = 0
        )
        BEGIN
            SELECT CAST(0 AS BIT) AS IsSuccess, 403 AS ErrCode, 'Account is inactive' AS ErrMsg;
            RETURN;
        END

        SELECT
            CAST(1 AS BIT) AS IsSuccess,
            0 AS ErrCode,
            'SUCCESS' AS ErrMsg,
            u.Id,
            u.BranchId,
            u.RoleId,
            u.RoleName,
            u.Username,
            u.FullName,
            u.Phone,
            u.Email,
            u.IsActive,
            u.CreatedAt,
            u.AvatarUrl
        FROM [nhvpa3en_vpa01].[CDV_User] u
        WHERE u.Username = @Username
          AND u.PasswordHash = @PasswordHash;

        IF OBJECT_ID(N'[nhvpa3en_vpa01].[Sol_AuditLog]', N'U') IS NOT NULL
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[Sol_AuditLog]
            (
                UserId,
                TableName,
                ActionType,
                RecordId,
                NewData,
                CreatedAt
            )
            SELECT
                u.Id,
                'CDV_User',
                'LOGIN',
                u.Id,
                @Username,
                GETDATE()
            FROM [nhvpa3en_vpa01].[CDV_User] u
            WHERE u.Username = @Username;
        END
    END TRY
    BEGIN CATCH
        SELECT CAST(0 AS BIT) AS IsSuccess, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_User_Upsert]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_User_Upsert];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_User_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @FullName NVARCHAR(200),
    @Username VARCHAR(100),
    @PasswordHash VARCHAR(500),
    @Phone VARCHAR(20),
    @Email VARCHAR(100),
    @RoleID INT,
    @RoleName VARCHAR(100),
    @BranchId INT,
    @IsActive BIT,
    @AvatarUrl NVARCHAR(500)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF @Id = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_User]
            (
                FullName,
                Username,
                PasswordHash,
                Phone,
                Email,
                RoleID,
                RoleName,
                BranchId,
                IsActive,
                AvatarUrl
            )
            VALUES
            (
                @FullName,
                @Username,
                @PasswordHash,
                @Phone,
                @Email,
                @RoleID,
                @RoleName,
                @BranchId,
                @IsActive,
                @AvatarUrl
            );

            SET @Id = SCOPE_IDENTITY();

            IF OBJECT_ID(N'[nhvpa3en_vpa01].[Sol_AuditLog]', N'U') IS NOT NULL
            BEGIN
                INSERT INTO [nhvpa3en_vpa01].[Sol_AuditLog]
                (
                    UserId,
                    TableName,
                    ActionType,
                    RecordId,
                    NewData,
                    CreatedAt
                )
                VALUES
                (
                    @UserId,
                    'CDV_User',
                    'INSERT',
                    @Id,
                    @Username,
                    GETDATE()
                );
            END
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_User]
            SET
                FullName = @FullName,
                Username = @Username,
                PasswordHash = @PasswordHash,
                Phone = @Phone,
                Email = @Email,
                RoleId = @RoleID,
                RoleName = @RoleName,
                BranchId = @BranchId,
                IsActive = @IsActive,
                AvatarUrl = @AvatarUrl
            WHERE Id = @Id;

            IF OBJECT_ID(N'[nhvpa3en_vpa01].[Sol_AuditLog]', N'U') IS NOT NULL
            BEGIN
                INSERT INTO [nhvpa3en_vpa01].[Sol_AuditLog]
                (
                    UserId,
                    TableName,
                    ActionType,
                    RecordId,
                    NewData,
                    CreatedAt
                )
                VALUES
                (
                    @UserId,
                    'CDV_User',
                    'UPDATE',
                    @Id,
                    @Username,
                    GETDATE()
                );
            END
        END

        COMMIT TRAN;

        SELECT @Id AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @Id AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[Cdv_User_ChangePassword]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[Cdv_User_ChangePassword];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[Cdv_User_ChangePassword]
(
    @UserId INT,
    @OldPasswordHash VARCHAR(500),
    @NewPasswordHash VARCHAR(500)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_User]
            WHERE Id = @UserId
        )
        BEGIN
            SELECT @UserId AS ID, 404 AS ErrCode, 'User not found' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF NOT EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_User]
            WHERE Id = @UserId
              AND PasswordHash = @OldPasswordHash
        )
        BEGIN
            SELECT @UserId AS ID, 401 AS ErrCode, 'Old password incorrect' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        UPDATE [nhvpa3en_vpa01].[CDV_User]
        SET PasswordHash = @NewPasswordHash
        WHERE Id = @UserId;

        IF OBJECT_ID(N'[nhvpa3en_vpa01].[Sol_AuditLog]', N'U') IS NOT NULL
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[Sol_AuditLog]
            (
                UserId,
                TableName,
                ActionType,
                RecordId,
                NewData,
                CreatedAt
            )
            VALUES
            (
                @UserId,
                'CDV_User',
                'CHANGE_PASSWORD',
                @UserId,
                'Password changed',
                GETDATE()
            );
        END

        COMMIT TRAN;

        SELECT @UserId AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @UserId AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_User_Delete]', N'P') IS NOT NULL
    DROP PROCEDURE [nhvpa3en_vpa01].[CDV_User_Delete];
GO

CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_User_Delete]
(
    @Id INT,
    @UserId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM [nhvpa3en_vpa01].[CDV_User]
        WHERE Id = @Id;

        IF OBJECT_ID(N'[nhvpa3en_vpa01].[Sol_AuditLog]', N'U') IS NOT NULL
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[Sol_AuditLog]
            (
                UserId,
                TableName,
                ActionType,
                RecordId,
                CreatedAt
            )
            VALUES
            (
                @UserId,
                'CDV_User',
                'DELETE',
                @Id,
                GETDATE()
            );
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
