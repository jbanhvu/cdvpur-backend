SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_ExitPermission]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_ExitPermission]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ExitNo] VARCHAR(50) NOT NULL,
        [Title] NVARCHAR(1000) NULL,
        [RequesterUserId] NVARCHAR(100) NULL,
        [RequesterUserName] NVARCHAR(400) NULL,
        [RequesterDepartmentId] NVARCHAR(100) NULL,
        [RequesterDepartmentName] NVARCHAR(400) NULL,
        [ExitType] NVARCHAR(200) NULL,
        [ExpectedReturnDate] DATE NULL,
        [CarrierUserId] NVARCHAR(100) NULL,
        [CarrierUserName] NVARCHAR(400) NULL,
        [CarrierDepartmentId] NVARCHAR(100) NULL,
        [CarrierDepartmentName] NVARCHAR(400) NULL,
        [ReceiverCompany] NVARCHAR(500) NULL,
        [VehicleNo] NVARCHAR(100) NULL,
        [DriverName] NVARCHAR(200) NULL,
        [Reason] NVARCHAR(MAX) NULL,
        [Status] VARCHAR(30) NOT NULL,
        [ApprovalDocumentId] VARCHAR(100) NOT NULL,
        [DocumentFormId] VARCHAR(100) NULL,
        [CreatedTime] DATETIMEOFFSET NULL,
        [CompletedTime] DATETIMEOFFSET NULL,
        [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_CDV_ExitPermission_CreatedDate] DEFAULT (SYSDATETIME()),
        [UpdatedDate] DATETIME2 NULL,
        CONSTRAINT [PK_CDV_ExitPermission] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_ExitPermission_ExitNo] UNIQUE ([ExitNo]),
        CONSTRAINT [UQ_CDV_ExitPermission_ApprovalDocumentId] UNIQUE ([ApprovalDocumentId])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_ExitPermissionDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_ExitPermissionDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ExitPermissionId] INT NOT NULL,
        [LineNo] INT NULL,
        [ItemName] NVARCHAR(500) NULL,
        [ItemCode] NVARCHAR(200) NULL,
        [Unit] NVARCHAR(100) NULL,
        [Qty] DECIMAL(18,2) NULL,
        [Remark] NVARCHAR(1000) NULL,
        CONSTRAINT [PK_CDV_ExitPermissionDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_ExitPermissionDetail_ExitPermission]
            FOREIGN KEY ([ExitPermissionId]) REFERENCES [nhvpa3en_vpa01].[CDV_ExitPermission] ([Id])
            ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_ExitPermission_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_ExitPermission_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_ExitPermission_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_ExitPermission]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
    ORDER BY CreatedTime DESC, Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_ExitPermissionDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_ExitPermissionDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_ExitPermissionDetail_Select]
    @Id INT = 0,
    @ExitPermissionId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_ExitPermissionDetail]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
      AND (@ExitPermissionId IS NULL OR ExitPermissionId = @ExitPermissionId)
    ORDER BY Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_ExitPermission_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_ExitPermission_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_ExitPermission_Upsert]
    @ExitNo VARCHAR(50),
    @Title NVARCHAR(1000) = NULL,
    @RequesterUserId NVARCHAR(100) = NULL,
    @RequesterUserName NVARCHAR(400) = NULL,
    @RequesterDepartmentId NVARCHAR(100) = NULL,
    @RequesterDepartmentName NVARCHAR(400) = NULL,
    @ExitType NVARCHAR(200) = NULL,
    @ExpectedReturnDate DATE = NULL,
    @CarrierUserId NVARCHAR(100) = NULL,
    @CarrierUserName NVARCHAR(400) = NULL,
    @CarrierDepartmentId NVARCHAR(100) = NULL,
    @CarrierDepartmentName NVARCHAR(400) = NULL,
    @ReceiverCompany NVARCHAR(500) = NULL,
    @VehicleNo NVARCHAR(100) = NULL,
    @DriverName NVARCHAR(200) = NULL,
    @Reason NVARCHAR(MAX) = NULL,
    @Status VARCHAR(30),
    @ApprovalDocumentId VARCHAR(100),
    @DocumentFormId VARCHAR(100) = NULL,
    @CreatedTime DATETIMEOFFSET = NULL,
    @CompletedTime DATETIMEOFFSET = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM [nhvpa3en_vpa01].[CDV_ExitPermission]
        WHERE ApprovalDocumentId = @ApprovalDocumentId
    )
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_ExitPermission]
        SET
            ExitNo = @ExitNo,
            Title = @Title,
            RequesterUserId = @RequesterUserId,
            RequesterUserName = @RequesterUserName,
            RequesterDepartmentId = @RequesterDepartmentId,
            RequesterDepartmentName = @RequesterDepartmentName,
            ExitType = @ExitType,
            ExpectedReturnDate = @ExpectedReturnDate,
            CarrierUserId = @CarrierUserId,
            CarrierUserName = @CarrierUserName,
            CarrierDepartmentId = @CarrierDepartmentId,
            CarrierDepartmentName = @CarrierDepartmentName,
            ReceiverCompany = @ReceiverCompany,
            VehicleNo = @VehicleNo,
            DriverName = @DriverName,
            Reason = @Reason,
            Status = @Status,
            DocumentFormId = @DocumentFormId,
            CreatedTime = @CreatedTime,
            CompletedTime = @CompletedTime,
            UpdatedDate = SYSDATETIME()
        WHERE ApprovalDocumentId = @ApprovalDocumentId;
    END
    ELSE
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_ExitPermission]
        (
            ExitNo,
            Title,
            RequesterUserId,
            RequesterUserName,
            RequesterDepartmentId,
            RequesterDepartmentName,
            ExitType,
            ExpectedReturnDate,
            CarrierUserId,
            CarrierUserName,
            CarrierDepartmentId,
            CarrierDepartmentName,
            ReceiverCompany,
            VehicleNo,
            DriverName,
            Reason,
            Status,
            ApprovalDocumentId,
            DocumentFormId,
            CreatedTime,
            CompletedTime
        )
        VALUES
        (
            @ExitNo,
            @Title,
            @RequesterUserId,
            @RequesterUserName,
            @RequesterDepartmentId,
            @RequesterDepartmentName,
            @ExitType,
            @ExpectedReturnDate,
            @CarrierUserId,
            @CarrierUserName,
            @CarrierDepartmentId,
            @CarrierDepartmentName,
            @ReceiverCompany,
            @VehicleNo,
            @DriverName,
            @Reason,
            @Status,
            @ApprovalDocumentId,
            @DocumentFormId,
            @CreatedTime,
            @CompletedTime
        );
    END

    SELECT Id
    FROM [nhvpa3en_vpa01].[CDV_ExitPermission]
    WHERE ApprovalDocumentId = @ApprovalDocumentId;
END
GO
