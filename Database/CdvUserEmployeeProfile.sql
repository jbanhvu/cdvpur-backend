IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'Nationality') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [Nationality] NVARCHAR(50) NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'IdentityNumber') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [IdentityNumber] VARCHAR(30) NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'IdentityIssueDate') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [IdentityIssueDate] DATE NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'Address') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [Address] NVARCHAR(500) NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'EmergencyContact') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [EmergencyContact] NVARCHAR(100) NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'EmergencyPhone') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [EmergencyPhone] VARCHAR(20) NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'ContractStartDate') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [ContractStartDate] DATE NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'ContractEndDate') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [ContractEndDate] DATE NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'AttendanceCode') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [AttendanceCode] VARCHAR(50) NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'NaverWorksUserId') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [NaverWorksUserId] VARCHAR(100) NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'UpdatedAt') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [UpdatedAt] DATETIME NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'UpdatedBy') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [UpdatedBy] INT NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'EmployeeCode') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [EmployeeCode] VARCHAR(30) NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'Gender') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [Gender] TINYINT NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'DateOfBirth') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [DateOfBirth] DATE NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'JoinDate') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [JoinDate] DATE NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'ResignDate') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [ResignDate] DATE NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'EmploymentStatus') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [EmploymentStatus] TINYINT NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'Position') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [Position] VARCHAR(30) NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'ManagerId') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [ManagerId] INT NULL;
GO
IF COL_LENGTH(N'[nhvpa3en_vpa01].[CDV_User]', 'EmploymentType') IS NULL
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User] ADD [EmploymentType] TINYINT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_CDV_User_EmployeeCode' AND object_id = OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_User]'))
   AND NOT EXISTS
   (
       SELECT 1
       FROM [nhvpa3en_vpa01].[CDV_User]
       WHERE EmployeeCode IS NOT NULL AND LTRIM(RTRIM(EmployeeCode)) <> ''
       GROUP BY EmployeeCode
       HAVING COUNT(*) > 1
   )
BEGIN
    CREATE UNIQUE INDEX [UX_CDV_User_EmployeeCode]
    ON [nhvpa3en_vpa01].[CDV_User] ([EmployeeCode])
    WHERE [EmployeeCode] IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CDV_User_Manager')
   AND NOT EXISTS
   (
       SELECT 1
       FROM [nhvpa3en_vpa01].[CDV_User] u
       WHERE u.ManagerId IS NOT NULL
         AND NOT EXISTS
         (
             SELECT 1
             FROM [nhvpa3en_vpa01].[CDV_User] m
             WHERE m.Id = u.ManagerId
         )
   )
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_User]
        ADD CONSTRAINT [FK_CDV_User_Manager]
        FOREIGN KEY ([ManagerId]) REFERENCES [nhvpa3en_vpa01].[CDV_User] ([Id]);
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
        d.Code AS DepartmentCode,
        m.FullName AS ManagerName,
        m.EmployeeCode AS ManagerEmployeeCode
    FROM [nhvpa3en_vpa01].[CDV_User] u
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Departmant] d ON d.Id = u.DepartmentId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_User] m ON m.Id = u.ManagerId
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
        d.Code AS DepartmentCode,
        m.FullName AS ManagerName,
        m.EmployeeCode AS ManagerEmployeeCode
    FROM [nhvpa3en_vpa01].[CDV_User] u
    INNER JOIN [nhvpa3en_vpa01].[CDV_Departmant] d ON d.Id = u.DepartmentId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_User] m ON m.Id = u.ManagerId
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
        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_User] WHERE Username = @Username)
        BEGIN
            SELECT CAST(0 AS BIT) AS IsSuccess, 404 AS ErrCode, 'Username does not exist' AS ErrMsg;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_User] WHERE Username = @Username AND PasswordHash = @PasswordHash)
        BEGIN
            SELECT CAST(0 AS BIT) AS IsSuccess, 401 AS ErrCode, 'Wrong password' AS ErrMsg;
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_User] WHERE Username = @Username AND ISNULL(IsActive, 0) = 0)
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
            u.AvatarUrl,
            u.Nationality,
            u.IdentityNumber,
            u.IdentityIssueDate,
            u.Address,
            u.EmergencyContact,
            u.EmergencyPhone,
            u.ContractStartDate,
            u.ContractEndDate,
            u.AttendanceCode,
            u.NaverWorksUserId,
            u.UpdatedAt,
            u.UpdatedBy,
            u.EmployeeCode,
            u.Gender,
            u.DateOfBirth,
            u.JoinDate,
            u.ResignDate,
            u.EmploymentStatus,
            u.Position,
            u.ManagerId,
            m.FullName AS ManagerName,
            m.EmployeeCode AS ManagerEmployeeCode,
            u.EmploymentType
        FROM [nhvpa3en_vpa01].[CDV_User] u
        LEFT JOIN [nhvpa3en_vpa01].[CDV_Departmant] d ON d.Id = u.DepartmentId
        LEFT JOIN [nhvpa3en_vpa01].[CDV_User] m ON m.Id = u.ManagerId
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
    @Nationality NVARCHAR(50) = NULL,
    @IdentityNumber VARCHAR(30) = NULL,
    @IdentityIssueDate DATE = NULL,
    @Address NVARCHAR(500) = NULL,
    @EmergencyContact NVARCHAR(100) = NULL,
    @EmergencyPhone VARCHAR(20) = NULL,
    @ContractStartDate DATE = NULL,
    @ContractEndDate DATE = NULL,
    @AttendanceCode VARCHAR(50) = NULL,
    @NaverWorksUserId VARCHAR(100) = NULL,
    @UpdatedBy INT = NULL,
    @EmployeeCode VARCHAR(30) = NULL,
    @Gender TINYINT = NULL,
    @DateOfBirth DATE = NULL,
    @JoinDate DATE = NULL,
    @ResignDate DATE = NULL,
    @EmploymentStatus TINYINT = NULL,
    @Position VARCHAR(30) = NULL,
    @ManagerId INT = NULL,
    @EmploymentType TINYINT = NULL,
    @IsActive BIT,
    @AvatarUrl NVARCHAR(500)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NULLIF(LTRIM(RTRIM(@EmployeeCode)), '') IS NOT NULL
           AND EXISTS
           (
               SELECT 1
               FROM [nhvpa3en_vpa01].[CDV_User]
               WHERE EmployeeCode = @EmployeeCode
                 AND Id <> ISNULL(@Id, -1)
           )
        BEGIN
            SELECT @Id AS ID, -1 AS ErrCode, 'EMPLOYEE_CODE_ALREADY_EXISTS' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF @Id = -1
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[CDV_User]
            (
                FullName, Username, PasswordHash, Phone, Email, RoleID, RoleName, BranchId, DepartmentId,
                Nationality, IdentityNumber, IdentityIssueDate, Address, EmergencyContact, EmergencyPhone,
                ContractStartDate, ContractEndDate, AttendanceCode, NaverWorksUserId, EmployeeCode,
                Gender, DateOfBirth, JoinDate, ResignDate, EmploymentStatus, Position, ManagerId,
                EmploymentType, IsActive, AvatarUrl
            )
            VALUES
            (
                @FullName, @Username, @PasswordHash, @Phone, @Email, @RoleID, @RoleName, @BranchId, @DepartmentId,
                @Nationality, @IdentityNumber, @IdentityIssueDate, @Address, @EmergencyContact, @EmergencyPhone,
                @ContractStartDate, @ContractEndDate, @AttendanceCode, @NaverWorksUserId, NULLIF(LTRIM(RTRIM(@EmployeeCode)), ''),
                @Gender, @DateOfBirth, @JoinDate, @ResignDate, @EmploymentStatus, @Position, @ManagerId,
                @EmploymentType, @IsActive, @AvatarUrl
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
                Nationality = @Nationality,
                IdentityNumber = @IdentityNumber,
                IdentityIssueDate = @IdentityIssueDate,
                Address = @Address,
                EmergencyContact = @EmergencyContact,
                EmergencyPhone = @EmergencyPhone,
                ContractStartDate = @ContractStartDate,
                ContractEndDate = @ContractEndDate,
                AttendanceCode = @AttendanceCode,
                NaverWorksUserId = @NaverWorksUserId,
                UpdatedAt = GETDATE(),
                UpdatedBy = COALESCE(@UpdatedBy, @UserId),
                EmployeeCode = NULLIF(LTRIM(RTRIM(@EmployeeCode)), ''),
                Gender = @Gender,
                DateOfBirth = @DateOfBirth,
                JoinDate = @JoinDate,
                ResignDate = @ResignDate,
                EmploymentStatus = @EmploymentStatus,
                Position = @Position,
                ManagerId = @ManagerId,
                EmploymentType = @EmploymentType,
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
