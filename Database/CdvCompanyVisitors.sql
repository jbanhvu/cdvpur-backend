SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_CompanyVisitor]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_CompanyVisitor]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [VisitorNo] VARCHAR(50) NOT NULL,
        [Title] NVARCHAR(1000) NULL,
        [RequesterUserId] NVARCHAR(100) NULL,
        [RequesterUserName] NVARCHAR(400) NULL,
        [RequesterDepartmentId] NVARCHAR(100) NULL,
        [RequesterDepartmentName] NVARCHAR(400) NULL,
        [CompanyName] NVARCHAR(500) NULL,
        [RepresentativeName] NVARCHAR(400) NULL,
        [VisitDate] DATE NULL,
        [StartTime] TIME NULL,
        [EndTime] TIME NULL,
        [Purpose] NVARCHAR(MAX) NULL,
        [ContactUserId] NVARCHAR(100) NULL,
        [ContactUserName] NVARCHAR(400) NULL,
        [Status] VARCHAR(30) NOT NULL,
        [ApprovalDocumentId] VARCHAR(100) NOT NULL,
        [DocumentFormId] VARCHAR(100) NULL,
        [CreatedTime] DATETIMEOFFSET NULL,
        [CompletedTime] DATETIMEOFFSET NULL,
        [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_CDV_CompanyVisitor_CreatedDate] DEFAULT (SYSDATETIME()),
        [UpdatedDate] DATETIME2 NULL,
        CONSTRAINT [PK_CDV_CompanyVisitor] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_CompanyVisitor_VisitorNo] UNIQUE ([VisitorNo]),
        CONSTRAINT [UQ_CDV_CompanyVisitor_ApprovalDocumentId] UNIQUE ([ApprovalDocumentId])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_CompanyVisitorDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_CompanyVisitorDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [CompanyVisitorId] INT NOT NULL,
        [VisitorName] NVARCHAR(400) NULL,
        [VisitorTitle] NVARCHAR(400) NULL,
        [IdentityNo] NVARCHAR(200) NULL,
        CONSTRAINT [PK_CDV_CompanyVisitorDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_CompanyVisitorDetail_CompanyVisitor]
            FOREIGN KEY ([CompanyVisitorId]) REFERENCES [nhvpa3en_vpa01].[CDV_CompanyVisitor] ([Id])
            ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_CompanyVisitor_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_CompanyVisitor_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_CompanyVisitor_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_CompanyVisitor]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
    ORDER BY CreatedTime DESC, Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_CompanyVisitorDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_CompanyVisitorDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_CompanyVisitorDetail_Select]
    @Id INT = 0,
    @CompanyVisitorId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_CompanyVisitorDetail]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
      AND (@CompanyVisitorId IS NULL OR CompanyVisitorId = @CompanyVisitorId)
    ORDER BY Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_CompanyVisitor_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_CompanyVisitor_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_CompanyVisitor_Upsert]
    @VisitorNo VARCHAR(50),
    @Title NVARCHAR(1000) = NULL,
    @RequesterUserId NVARCHAR(100) = NULL,
    @RequesterUserName NVARCHAR(400) = NULL,
    @RequesterDepartmentId NVARCHAR(100) = NULL,
    @RequesterDepartmentName NVARCHAR(400) = NULL,
    @CompanyName NVARCHAR(500) = NULL,
    @RepresentativeName NVARCHAR(400) = NULL,
    @VisitDate DATE = NULL,
    @StartTime TIME = NULL,
    @EndTime TIME = NULL,
    @Purpose NVARCHAR(MAX) = NULL,
    @ContactUserId NVARCHAR(100) = NULL,
    @ContactUserName NVARCHAR(400) = NULL,
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
        FROM [nhvpa3en_vpa01].[CDV_CompanyVisitor]
        WHERE ApprovalDocumentId = @ApprovalDocumentId
    )
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_CompanyVisitor]
        SET
            VisitorNo = @VisitorNo,
            Title = @Title,
            RequesterUserId = @RequesterUserId,
            RequesterUserName = @RequesterUserName,
            RequesterDepartmentId = @RequesterDepartmentId,
            RequesterDepartmentName = @RequesterDepartmentName,
            CompanyName = @CompanyName,
            RepresentativeName = @RepresentativeName,
            VisitDate = @VisitDate,
            StartTime = @StartTime,
            EndTime = @EndTime,
            Purpose = @Purpose,
            ContactUserId = @ContactUserId,
            ContactUserName = @ContactUserName,
            Status = @Status,
            DocumentFormId = @DocumentFormId,
            CreatedTime = @CreatedTime,
            CompletedTime = @CompletedTime,
            UpdatedDate = SYSDATETIME()
        WHERE ApprovalDocumentId = @ApprovalDocumentId;
    END
    ELSE
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_CompanyVisitor]
        (
            VisitorNo,
            Title,
            RequesterUserId,
            RequesterUserName,
            RequesterDepartmentId,
            RequesterDepartmentName,
            CompanyName,
            RepresentativeName,
            VisitDate,
            StartTime,
            EndTime,
            Purpose,
            ContactUserId,
            ContactUserName,
            Status,
            ApprovalDocumentId,
            DocumentFormId,
            CreatedTime,
            CompletedTime
        )
        VALUES
        (
            @VisitorNo,
            @Title,
            @RequesterUserId,
            @RequesterUserName,
            @RequesterDepartmentId,
            @RequesterDepartmentName,
            @CompanyName,
            @RepresentativeName,
            @VisitDate,
            @StartTime,
            @EndTime,
            @Purpose,
            @ContactUserId,
            @ContactUserName,
            @Status,
            @ApprovalDocumentId,
            @DocumentFormId,
            @CreatedTime,
            @CompletedTime
        );
    END

    SELECT Id
    FROM [nhvpa3en_vpa01].[CDV_CompanyVisitor]
    WHERE ApprovalDocumentId = @ApprovalDocumentId;
END
GO
