SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Hiring]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_Hiring]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [HiringNo] VARCHAR(50) NOT NULL,
        [Title] NVARCHAR(1000) NULL,
        [RequesterUserId] NVARCHAR(100) NULL,
        [RequesterUserName] NVARCHAR(400) NULL,
        [RequesterDepartmentId] NVARCHAR(100) NULL,
        [RequesterDepartmentName] NVARCHAR(400) NULL,
        [Status] VARCHAR(30) NOT NULL,
        [ApprovalDocumentId] VARCHAR(100) NOT NULL,
        [DocumentFormId] VARCHAR(100) NULL,
        [CreatedTime] DATETIMEOFFSET NULL,
        [CompletedTime] DATETIMEOFFSET NULL,
        [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_CDV_Hiring_CreatedDate] DEFAULT (SYSDATETIME()),
        [UpdatedDate] DATETIME2 NULL,
        CONSTRAINT [PK_CDV_Hiring] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_Hiring_HiringNo] UNIQUE ([HiringNo]),
        CONSTRAINT [UQ_CDV_Hiring_ApprovalDocumentId] UNIQUE ([ApprovalDocumentId])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_HiringDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_HiringDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [HiringId] INT NOT NULL,
        [PlacementDepartmentId] NVARCHAR(100) NULL,
        [PlacementDepartmentName] NVARCHAR(400) NULL,
        [JobTitle] NVARCHAR(500) NULL,
        [PositionLevel] NVARCHAR(200) NULL,
        [HeadCount] INT NULL,
        [HiringReason] NVARCHAR(200) NULL,
        [ReasonDetail] NVARCHAR(1000) NULL,
        [DesiredStartDate] DATE NULL,
        [SalaryLevel] NVARCHAR(200) NULL,
        [Qualification] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_CDV_HiringDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_HiringDetail_Hiring]
            FOREIGN KEY ([HiringId]) REFERENCES [nhvpa3en_vpa01].[CDV_Hiring] ([Id])
            ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Hiring_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Hiring_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Hiring_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_Hiring]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
    ORDER BY CreatedTime DESC, Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_HiringDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_HiringDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_HiringDetail_Select]
    @Id INT = 0,
    @HiringId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_HiringDetail]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
      AND (@HiringId IS NULL OR HiringId = @HiringId)
    ORDER BY Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Hiring_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Hiring_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Hiring_Upsert]
    @HiringNo VARCHAR(50),
    @Title NVARCHAR(1000) = NULL,
    @RequesterUserId NVARCHAR(100) = NULL,
    @RequesterUserName NVARCHAR(400) = NULL,
    @RequesterDepartmentId NVARCHAR(100) = NULL,
    @RequesterDepartmentName NVARCHAR(400) = NULL,
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
        FROM [nhvpa3en_vpa01].[CDV_Hiring]
        WHERE ApprovalDocumentId = @ApprovalDocumentId
    )
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_Hiring]
        SET
            HiringNo = @HiringNo,
            Title = @Title,
            RequesterUserId = @RequesterUserId,
            RequesterUserName = @RequesterUserName,
            RequesterDepartmentId = @RequesterDepartmentId,
            RequesterDepartmentName = @RequesterDepartmentName,
            Status = @Status,
            DocumentFormId = @DocumentFormId,
            CreatedTime = @CreatedTime,
            CompletedTime = @CompletedTime,
            UpdatedDate = SYSDATETIME()
        WHERE ApprovalDocumentId = @ApprovalDocumentId;
    END
    ELSE
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_Hiring]
        (
            HiringNo,
            Title,
            RequesterUserId,
            RequesterUserName,
            RequesterDepartmentId,
            RequesterDepartmentName,
            Status,
            ApprovalDocumentId,
            DocumentFormId,
            CreatedTime,
            CompletedTime
        )
        VALUES
        (
            @HiringNo,
            @Title,
            @RequesterUserId,
            @RequesterUserName,
            @RequesterDepartmentId,
            @RequesterDepartmentName,
            @Status,
            @ApprovalDocumentId,
            @DocumentFormId,
            @CreatedTime,
            @CompletedTime
        );
    END

    SELECT Id
    FROM [nhvpa3en_vpa01].[CDV_Hiring]
    WHERE ApprovalDocumentId = @ApprovalDocumentId;
END
GO
