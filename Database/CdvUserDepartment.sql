IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'DepartmentId') IS NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User]
        ADD [DepartmentId] INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CDV_User_Departmant')
    AND OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Departmant]', N'U') IS NOT NULL
    AND NOT EXISTS
    (
        SELECT 1
        FROM [nhvpa3en_vpa01].[CDV_User] u
        WHERE u.DepartmentId IS NOT NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM [nhvpa3en_vpa01].[CDV_Departmant] d
              WHERE d.Id = u.DepartmentId
          )
    )
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User]
        ADD CONSTRAINT [FK_CDV_User_Departmant]
        FOREIGN KEY ([DepartmentId]) REFERENCES [nhvpa3en_vpa01].[CDV_Departmant] ([Id]);
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_User_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_User_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_User_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.*,
        d.Name AS DepartmentName,
        d.Code AS DepartmentCode
    FROM [nhvpa3en_vpa01].[CDV_User] u
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Departmant] d ON d.Id = u.DepartmentId
    WHERE @Id = 0 OR u.Id = @Id
    ORDER BY u.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_User_SelectByDepartmentCode]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_User_SelectByDepartmentCode] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_User_SelectByDepartmentCode]
(
    @DepartmentCode VARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.*,
        d.Name AS DepartmentName,
        d.Code AS DepartmentCode
    FROM [nhvpa3en_vpa01].[CDV_User] u
    INNER JOIN [nhvpa3en_vpa01].[CDV_Departmant] d ON d.Id = u.DepartmentId
    WHERE d.Code = @DepartmentCode
    ORDER BY u.Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_User_Login]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_User_Login] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_User_Login]
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
            u.DepartmentId,
            d.Name AS DepartmentName,
            d.Code AS DepartmentCode,
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
        LEFT JOIN [nhvpa3en_vpa01].[CDV_Departmant] d ON d.Id = u.DepartmentId
        WHERE u.Username = @Username
          AND u.PasswordHash = @PasswordHash;
    END TRY
    BEGIN CATCH
        SELECT CAST(0 AS BIT) AS IsSuccess, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_User_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_User_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_User_Upsert]
(
    @Id INT = -1,
    @UserId INT = 0,
    @FullName NVARCHAR(200),
    @Username VARCHAR(100),
    @PasswordHash VARCHAR(500) = NULL,
    @Phone VARCHAR(20),
    @Email VARCHAR(100),
    @RoleID INT,
    @RoleName VARCHAR(100),
    @BranchId INT = NULL,
    @DepartmentId INT = NULL,
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
                DepartmentId,
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
                @DepartmentId,
                @IsActive,
                @AvatarUrl
            );

            SET @Id = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE [nhvpa3en_vpa01].[CDV_User]
            SET
                FullName = @FullName,
                Username = @Username,
                PasswordHash = COALESCE(NULLIF(LTRIM(RTRIM(@PasswordHash)), ''), PasswordHash),
                Phone = @Phone,
                Email = @Email,
                RoleId = @RoleID,
                RoleName = @RoleName,
                BranchId = COALESCE(@BranchId, BranchId),
                DepartmentId = @DepartmentId,
                IsActive = @IsActive,
                AvatarUrl = @AvatarUrl
            WHERE Id = @Id;
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
