SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_PurchaseRequestDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [nhvpa3en_vpa01].[CDV_PurchaseRequestDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [PurchaseRequestId] INT NOT NULL,
        [UseDepartment] NVARCHAR(200) NULL,
        [ItemName] NVARCHAR(500) NOT NULL,
        [Specification] NVARCHAR(500) NULL,
        [Unit] NVARCHAR(50) NULL,
        [Quantity] DECIMAL(18,4) NOT NULL,
        [EstimatedUnitPrice] DECIMAL(18,2) NULL,
        [EstimatedAmount] DECIMAL(18,2) NULL,
        CONSTRAINT [PK_CDV_PurchaseRequestDetail] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_CDV_PurchaseRequestDetail_PurchaseRequest]
            FOREIGN KEY ([PurchaseRequestId]) REFERENCES [nhvpa3en_vpa01].[CDV_PurchaseRequest] ([Id])
    );
END
GO

IF COL_LENGTH(N'nhvpa3en_vpa01.CDV_PurchaseRequest', N'Reason') IS NOT NULL
BEGIN
    ALTER TABLE [nhvpa3en_vpa01].[CDV_PurchaseRequest]
        ALTER COLUMN [Reason] NVARCHAR(MAX) NULL;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_PurchaseRequestDetail_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_PurchaseRequestDetail_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_PurchaseRequestDetail_Upsert]
    @PurchaseRequestId INT,
    @Details [nhvpa3en_vpa01].[CDV_PurchaseRequestDetailType] READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    UPDATE D
    SET
        D.UseDepartment = S.UseDepartment,
        D.ItemName = S.ItemName,
        D.Specification = S.Specification,
        D.Unit = S.Unit,
        D.Quantity = S.Quantity,
        D.EstimatedUnitPrice = S.EstimatedUnitPrice,
        D.EstimatedAmount = S.EstimatedAmount
    FROM [nhvpa3en_vpa01].[CDV_PurchaseRequestDetail] D
    INNER JOIN @Details S ON D.Id = S.Id
    WHERE D.PurchaseRequestId = @PurchaseRequestId;

    DELETE D
    FROM [nhvpa3en_vpa01].[CDV_PurchaseRequestDetail] D
    WHERE D.PurchaseRequestId = @PurchaseRequestId
      AND NOT EXISTS
      (
          SELECT 1
          FROM @Details S
          WHERE ISNULL(S.Id, 0) > 0
            AND S.Id = D.Id
      );

    INSERT INTO [nhvpa3en_vpa01].[CDV_PurchaseRequestDetail]
    (
        PurchaseRequestId,
        UseDepartment,
        ItemName,
        Specification,
        Unit,
        Quantity,
        EstimatedUnitPrice,
        EstimatedAmount
    )
    SELECT
        @PurchaseRequestId,
        S.UseDepartment,
        S.ItemName,
        S.Specification,
        S.Unit,
        S.Quantity,
        S.EstimatedUnitPrice,
        S.EstimatedAmount
    FROM @Details S
    WHERE ISNULL(S.Id, 0) = 0;

    UPDATE PR
    SET
        TotalAmount = ISNULL(
            (
                SELECT SUM(ISNULL(D.EstimatedAmount, 0))
                FROM [nhvpa3en_vpa01].[CDV_PurchaseRequestDetail] D
                WHERE D.PurchaseRequestId = PR.Id
            ),
            0
        ),
        UpdatedDate = GETDATE()
    FROM [nhvpa3en_vpa01].[CDV_PurchaseRequest] PR
    WHERE PR.Id = @PurchaseRequestId;

    COMMIT TRANSACTION;

    SELECT @PurchaseRequestId AS PurchaseRequestId;
END
GO
