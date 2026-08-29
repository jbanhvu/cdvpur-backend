SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_BusinessTrip]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_BusinessTrip]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [TripNo] VARCHAR(50) NOT NULL,
        [Title] NVARCHAR(1000) NULL,
        [RequesterUserId] NVARCHAR(100) NULL,
        [RequesterUserName] NVARCHAR(400) NULL,
        [DepartmentId] NVARCHAR(100) NULL,
        [DepartmentName] NVARCHAR(400) NULL,
        [TripType] NVARCHAR(100) NULL,
        [Destination] NVARCHAR(500) NULL,
        [FromDate] DATE NULL,
        [ToDate] DATE NULL,
        [Purpose] NVARCHAR(MAX) NULL,
        [Companions] NVARCHAR(MAX) NULL,
        [PassengerCount] NVARCHAR(100) NULL,
        [UseTime] NVARCHAR(100) NULL,
        [UseCorporateCard] BIT NOT NULL CONSTRAINT [DF_CDV_BusinessTrip_UseCorporateCard] DEFAULT ((0)),
        [TotalAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_CDV_BusinessTrip_TotalAmount] DEFAULT ((0)),
        [Status] VARCHAR(30) NOT NULL,
        [ApprovalDocumentId] VARCHAR(100) NOT NULL,
        [DocumentFormId] VARCHAR(100) NULL,
        [CreatedTime] DATETIMEOFFSET NULL,
        [CompletedTime] DATETIMEOFFSET NULL,
        [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_CDV_BusinessTrip_CreatedDate] DEFAULT (SYSDATETIME()),
        [UpdatedDate] DATETIME2 NULL,
        CONSTRAINT [PK_CDV_BusinessTrip] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CDV_BusinessTrip_TripNo] UNIQUE ([TripNo]),
        CONSTRAINT [UQ_CDV_BusinessTrip_ApprovalDocumentId] UNIQUE ([ApprovalDocumentId])
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_BusinessTripDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_BusinessTripDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [BusinessTripId] INT NOT NULL,
        [TransportationAmount] DECIMAL(18,2) NULL,
        [LodgingAmount] DECIMAL(18,2) NULL,
        [DailyAllowanceAmount] DECIMAL(18,2) NULL,
        [OtherAmount] DECIMAL(18,2) NULL,
        [TotalAmount] DECIMAL(18,2) NULL,
        CONSTRAINT [PK_CDV_BusinessTripDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_BusinessTripDetail_BusinessTrip]
            FOREIGN KEY ([BusinessTripId]) REFERENCES [nhvpa3en_vpa01].[CDV_BusinessTrip] ([Id])
            ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_BusinessTrip_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_BusinessTrip_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_BusinessTrip_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_BusinessTrip]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
    ORDER BY CreatedTime DESC, Id DESC;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_BusinessTripDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_BusinessTripDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_BusinessTripDetail_Select]
    @Id INT = 0,
    @BusinessTripId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [nhvpa3en_vpa01].[CDV_BusinessTripDetail]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
      AND (@BusinessTripId IS NULL OR BusinessTripId = @BusinessTripId)
    ORDER BY Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_BusinessTrip_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_BusinessTrip_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_BusinessTrip_Upsert]
    @TripNo VARCHAR(50),
    @Title NVARCHAR(1000) = NULL,
    @RequesterUserId NVARCHAR(100) = NULL,
    @RequesterUserName NVARCHAR(400) = NULL,
    @DepartmentId NVARCHAR(100) = NULL,
    @DepartmentName NVARCHAR(400) = NULL,
    @TripType NVARCHAR(100) = NULL,
    @Destination NVARCHAR(500) = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @Purpose NVARCHAR(MAX) = NULL,
    @Companions NVARCHAR(MAX) = NULL,
    @PassengerCount NVARCHAR(100) = NULL,
    @UseTime NVARCHAR(100) = NULL,
    @UseCorporateCard BIT = 0,
    @TotalAmount DECIMAL(18,2) = 0,
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
        FROM [nhvpa3en_vpa01].[CDV_BusinessTrip]
        WHERE ApprovalDocumentId = @ApprovalDocumentId
    )
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_BusinessTrip]
        SET
            TripNo = @TripNo,
            Title = @Title,
            RequesterUserId = @RequesterUserId,
            RequesterUserName = @RequesterUserName,
            DepartmentId = @DepartmentId,
            DepartmentName = @DepartmentName,
            TripType = @TripType,
            Destination = @Destination,
            FromDate = @FromDate,
            ToDate = @ToDate,
            Purpose = @Purpose,
            Companions = @Companions,
            PassengerCount = @PassengerCount,
            UseTime = @UseTime,
            UseCorporateCard = ISNULL(@UseCorporateCard, 0),
            TotalAmount = ISNULL(@TotalAmount, 0),
            Status = @Status,
            DocumentFormId = @DocumentFormId,
            CreatedTime = @CreatedTime,
            CompletedTime = @CompletedTime,
            UpdatedDate = SYSDATETIME()
        WHERE ApprovalDocumentId = @ApprovalDocumentId;
    END
    ELSE
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_BusinessTrip]
        (
            TripNo,
            Title,
            RequesterUserId,
            RequesterUserName,
            DepartmentId,
            DepartmentName,
            TripType,
            Destination,
            FromDate,
            ToDate,
            Purpose,
            Companions,
            PassengerCount,
            UseTime,
            UseCorporateCard,
            TotalAmount,
            Status,
            ApprovalDocumentId,
            DocumentFormId,
            CreatedTime,
            CompletedTime
        )
        VALUES
        (
            @TripNo,
            @Title,
            @RequesterUserId,
            @RequesterUserName,
            @DepartmentId,
            @DepartmentName,
            @TripType,
            @Destination,
            @FromDate,
            @ToDate,
            @Purpose,
            @Companions,
            @PassengerCount,
            @UseTime,
            ISNULL(@UseCorporateCard, 0),
            ISNULL(@TotalAmount, 0),
            @Status,
            @ApprovalDocumentId,
            @DocumentFormId,
            @CreatedTime,
            @CompletedTime
        );
    END

    SELECT Id
    FROM [nhvpa3en_vpa01].[CDV_BusinessTrip]
    WHERE ApprovalDocumentId = @ApprovalDocumentId;
END
GO
