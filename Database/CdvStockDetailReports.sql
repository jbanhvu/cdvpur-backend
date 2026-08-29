SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockInDetail_Report]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_Report] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockInDetail_Report]
(
    @FromDate DATE,
    @ToDate DATE,
    @MaterialId INT = NULL,
    @Manufacturerid INT = NULL,
    @SupplierId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        HeaderId = si.Id,
        DetailId = sid.Id,
        StockNo = si.StockInNo,
        StockDate = CAST(si.StockInDate AS DATE),
        StockDateTime = si.StockInDate,
        SupplierId = si.SupplierId,
        SupplierCode = supplier.Code,
        SupplierName = supplier.Name,
        Manufacturerid = sid.Manufacturerid,
        ManufacturerCode = manufacturer.Code,
        ManufacturerName = manufacturer.Name,
        MaterialId = sid.MaterialId,
        MaterialCode = material.Code,
        MaterialName = material.Name,
        Unit = material.Unit,
        Specification = material.Specification,
        Qty = CAST(ISNULL(sid.Qty, 0) AS DECIMAL(18,2)),
        UnitPrice = sid.UnitPrice,
        Amount = CAST(ISNULL(sid.Qty, 0) * ISNULL(sid.UnitPrice, 0) AS DECIMAL(18,2)),
        LotNo = sid.LotNo,
        ExpiredDate = sid.ExpiredDate,
        HeaderStatus = si.Status,
        HeaderNote = si.Note,
        DetailNote = sid.Note
    FROM [nhvpa3en_vpa01].[CDV_StockInDetail] sid
    JOIN [nhvpa3en_vpa01].[CDV_StockIn] si ON si.Id = sid.StockInId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Supplier] supplier ON supplier.Id = si.SupplierId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Manufacturer] manufacturer ON manufacturer.Id = sid.Manufacturerid
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] material ON material.Id = sid.MaterialId
    WHERE CAST(si.StockInDate AS DATE) >= @FromDate
      AND CAST(si.StockInDate AS DATE) <= @ToDate
      AND (@MaterialId IS NULL OR sid.MaterialId = @MaterialId)
      AND (@Manufacturerid IS NULL OR sid.Manufacturerid = @Manufacturerid)
      AND (@SupplierId IS NULL OR si.SupplierId = @SupplierId)
    ORDER BY si.StockInDate, si.StockInNo, sid.Id;
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_StockOutDetail_Report]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_Report] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_StockOutDetail_Report]
(
    @FromDate DATE,
    @ToDate DATE,
    @MaterialId INT = NULL,
    @Manufacturerid INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        HeaderId = so.Id,
        DetailId = sod.Id,
        StockNo = so.StockOutNo,
        StockDate = CAST(so.StockOutDate AS DATE),
        StockDateTime = so.StockOutDate,
        SupplierId = CAST(NULL AS INT),
        SupplierCode = CAST(NULL AS VARCHAR(50)),
        SupplierName = CAST(NULL AS NVARCHAR(200)),
        Manufacturerid = sod.Manufacturerid,
        ManufacturerCode = manufacturer.Code,
        ManufacturerName = manufacturer.Name,
        MaterialId = sod.MaterialId,
        MaterialCode = material.Code,
        MaterialName = material.Name,
        Unit = material.Unit,
        Specification = material.Specification,
        Qty = CAST(ISNULL(sod.Qty, 0) AS DECIMAL(18,2)),
        UnitPrice = CAST(NULL AS DECIMAL(18,2)),
        Amount = CAST(0 AS DECIMAL(18,2)),
        LotNo = CAST(NULL AS NVARCHAR(100)),
        ExpiredDate = CAST(NULL AS DATE),
        HeaderStatus = so.Status,
        HeaderNote = so.Note,
        DetailNote = sod.Note
    FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] sod
    JOIN [nhvpa3en_vpa01].[CDV_StockOut] so ON so.Id = sod.StockOutId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Manufacturer] manufacturer ON manufacturer.Id = sod.Manufacturerid
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] material ON material.Id = sod.MaterialId
    WHERE CAST(so.StockOutDate AS DATE) >= @FromDate
      AND CAST(so.StockOutDate AS DATE) <= @ToDate
      AND (@MaterialId IS NULL OR sod.MaterialId = @MaterialId)
      AND (@Manufacturerid IS NULL OR sod.Manufacturerid = @Manufacturerid)
    ORDER BY so.StockOutDate, so.StockOutNo, sod.Id;
END
GO
