SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Purchase]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_Purchase]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [PurchaseNo] VARCHAR(50) NOT NULL,
        [Title] NVARCHAR(1000) NULL,
        [RequesterUserId] NVARCHAR(100) NULL,
        [RequesterUserName] NVARCHAR(400) NULL,
        [DepartmentId] NVARCHAR(100) NULL,
        [DepartmentName] NVARCHAR(400) NULL,
        [PurchaseDate] DATE NOT NULL,
        [PurchaseOverview] NVARCHAR(MAX) NULL,
        [SelectedSupplierReason] NVARCHAR(MAX) NULL,
        [CurrencyCode] VARCHAR(10) NOT NULL CONSTRAINT [DF_CDV_Purchase_CurrencyCode] DEFAULT ('VND'),
        [TotalAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_Purchase_TotalAmount] DEFAULT ((0)),
        [VatType] NVARCHAR(100) NULL,
        [PaymentTerms] NVARCHAR(500) NULL,
        [Status] VARCHAR(30) NOT NULL,
        [ApprovalDocumentId] VARCHAR(100) NOT NULL,
        [DocumentFormId] VARCHAR(100) NULL,
        [CreatedTime] DATETIMEOFFSET NULL,
        [CompletedTime] DATETIMEOFFSET NULL,
        [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_CDV_Purchase_CreatedDate] DEFAULT (SYSDATETIME()),
        [UpdatedDate] DATETIME2 NULL,
        CONSTRAINT [PK_CDV_Purchase] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_Purchase_PurchaseNo] UNIQUE ([PurchaseNo]),
        CONSTRAINT [UQ_CDV_Purchase_ApprovalDocumentId] UNIQUE ([ApprovalDocumentId])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_PurchaseDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_PurchaseDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [PurchaseId] INT NOT NULL,
        [ItemName] NVARCHAR(1000) NOT NULL,
        [SupplierName] NVARCHAR(400) NULL,
        [Amount] DECIMAL(18,2) NULL,
        [Delivery] NVARCHAR(400) NULL,
        [Note] NVARCHAR(1000) NULL,
        CONSTRAINT [PK_CDV_PurchaseDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_PurchaseDetail_Purchase]
            FOREIGN KEY ([PurchaseId]) REFERENCES [nhvpa3en_vpa01].[CDV_Purchase] ([Id])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_PurchaseRequestLink]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_PurchaseRequestLink]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [PurchaseId] INT NOT NULL,
        [PurchaseRequestId] INT NULL,
        [PurchaseRequestApprovalDocumentId] VARCHAR(100) NULL,
        [PurchaseRequestNo] VARCHAR(50) NULL,
        [PurchaseRequestTitle] NVARCHAR(1000) NULL,
        CONSTRAINT [PK_CDV_PurchaseRequestLink] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_PurchaseRequestLink_Purchase]
            FOREIGN KEY ([PurchaseId]) REFERENCES [nhvpa3en_vpa01].[CDV_Purchase] ([Id]),
        CONSTRAINT [FK_CDV_PurchaseRequestLink_PurchaseRequest]
            FOREIGN KEY ([PurchaseRequestId]) REFERENCES [nhvpa3en_vpa01].[CDV_PurchaseRequest] ([Id])
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_CDV_PurchaseRequestLink_Purchase_RequestApproval'
      AND object_id = OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_PurchaseRequestLink]')
)
BEGIN
    CREATE UNIQUE INDEX [UX_CDV_PurchaseRequestLink_Purchase_RequestApproval]
    ON [nhvpa3en_vpa01].[CDV_PurchaseRequestLink] ([PurchaseId], [PurchaseRequestApprovalDocumentId])
    WHERE [PurchaseRequestApprovalDocumentId] IS NOT NULL;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Purchase_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Purchase_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Purchase_Select]
(
    @Id INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_Purchase]
    WHERE @Id = 0 OR Id = @Id
    ORDER BY Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_PurchaseDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_PurchaseDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_PurchaseDetail_Select]
(
    @Id INT = NULL,
    @PurchaseId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_PurchaseDetail]
    WHERE (@Id IS NULL OR Id = @Id)
      AND (@PurchaseId IS NULL OR PurchaseId = @PurchaseId)
    ORDER BY Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_PurchaseRequestLink_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_PurchaseRequestLink_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_PurchaseRequestLink_Select]
(
    @Id INT = NULL,
    @PurchaseId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        link.*,
        request.PRNo,
        request.Title AS RequestTitle
    FROM [nhvpa3en_vpa01].[CDV_PurchaseRequestLink] link
    LEFT JOIN [nhvpa3en_vpa01].[CDV_PurchaseRequest] request ON request.Id = link.PurchaseRequestId
    WHERE (@Id IS NULL OR link.Id = @Id)
      AND (@PurchaseId IS NULL OR link.PurchaseId = @PurchaseId)
    ORDER BY link.Id;
END
GO
