SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Inventory_Report]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Inventory_Report] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Inventory_Report]
(
    @Year INT,
    @Month INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATE = DATEFROMPARTS(@Year, @Month, 1);
    DECLARE @EndDate DATE = DATEADD(MONTH, 1, @StartDate);

    ;WITH OpeningStock AS
    (
        SELECT
            io.MaterialId,
            io.ManufacturerId,
            CAST(SUM(ISNULL(io.Quantity, 0)) AS DECIMAL(18,2)) AS OpenStockQuantity,
            CAST(SUM(ISNULL(io.Quantity, 0) * ISNULL(m.DefaultPrice, 0)) AS DECIMAL(18,2)) AS OpenStockAmount
        FROM [nhvpa3en_vpa01].[CDV_InventoryOpening] io
        LEFT JOIN [nhvpa3en_vpa01].[CDV_Material] m ON m.Id = io.MaterialId
        WHERE io.OpeningDate = @StartDate
        GROUP BY io.MaterialId, io.ManufacturerId
    ),
    StockMove AS
    (
        SELECT
            sid.MaterialId,
            sid.Manufacturerid AS ManufacturerId,
            CAST(si.StockInDate AS DATE) AS MoveDate,
            CAST(sid.Qty AS DECIMAL(18,2)) AS InputQty,
            CAST(ISNULL(sid.Qty, 0) * ISNULL(sid.UnitPrice, 0) AS DECIMAL(18,2)) AS InputAmount,
            CAST(0 AS DECIMAL(18,2)) AS OutputQty,
            CAST(0 AS DECIMAL(18,2)) AS OutputAmount
        FROM [nhvpa3en_vpa01].[CDV_StockInDetail] sid
        JOIN [nhvpa3en_vpa01].[CDV_StockIn] si ON si.Id = sid.StockInId
        WHERE si.StockInDate >= @StartDate
          AND si.StockInDate < @EndDate

        UNION ALL

        SELECT
            sod.MaterialId,
            sod.ManufacturerID AS ManufacturerId,
            CAST(so.StockOutDate AS DATE) AS MoveDate,
            CAST(0 AS DECIMAL(18,2)) AS InputQty,
            CAST(0 AS DECIMAL(18,2)) AS InputAmount,
            CAST(ISNULL(sod.Qty, 0) AS DECIMAL(18,2)) AS OutputQty,
            CAST(0 AS DECIMAL(18,2)) AS OutputAmount
        FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] sod
        JOIN [nhvpa3en_vpa01].[CDV_StockOut] so ON so.Id = sod.StockOutId
        WHERE so.StockOutDate >= @StartDate
          AND so.StockOutDate < @EndDate
    ),
    MaterialManufacturerIds AS
    (
        SELECT MaterialId, ManufacturerId FROM OpeningStock

        UNION

        SELECT MaterialId, ManufacturerId FROM StockMove
    ),
    MaterialSummary AS
    (
        SELECT
            mi.MaterialId,
            mi.ManufacturerId,
            ISNULL(os.OpenStockQuantity, 0) AS OpenStockQuantity,
            ISNULL(os.OpenStockAmount, 0) AS OpenStockAmount,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate THEN sm.InputQty ELSE 0 END) AS InputQuantity,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate THEN sm.InputAmount ELSE 0 END) AS InputAmount,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate THEN sm.OutputQty ELSE 0 END) AS OutputQuantity,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate THEN sm.OutputAmount ELSE 0 END) AS OutputAmount,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 1 THEN sm.InputQty ELSE 0 END) AS InputQuantity1,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 1 THEN sm.OutputQty ELSE 0 END) AS OutQuantity1,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 2 THEN sm.InputQty ELSE 0 END) AS InputQuantity2,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 2 THEN sm.OutputQty ELSE 0 END) AS OutQuantity2,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 3 THEN sm.InputQty ELSE 0 END) AS InputQuantity3,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 3 THEN sm.OutputQty ELSE 0 END) AS OutQuantity3,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 4 THEN sm.InputQty ELSE 0 END) AS InputQuantity4,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 4 THEN sm.OutputQty ELSE 0 END) AS OutQuantity4,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 5 THEN sm.InputQty ELSE 0 END) AS InputQuantity5,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 5 THEN sm.OutputQty ELSE 0 END) AS OutQuantity5,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 6 THEN sm.InputQty ELSE 0 END) AS InputQuantity6,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 6 THEN sm.OutputQty ELSE 0 END) AS OutQuantity6,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 7 THEN sm.InputQty ELSE 0 END) AS InputQuantity7,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 7 THEN sm.OutputQty ELSE 0 END) AS OutQuantity7,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 8 THEN sm.InputQty ELSE 0 END) AS InputQuantity8,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 8 THEN sm.OutputQty ELSE 0 END) AS OutQuantity8,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 9 THEN sm.InputQty ELSE 0 END) AS InputQuantity9,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 9 THEN sm.OutputQty ELSE 0 END) AS OutQuantity9,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 10 THEN sm.InputQty ELSE 0 END) AS InputQuantity10,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 10 THEN sm.OutputQty ELSE 0 END) AS OutQuantity10,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 11 THEN sm.InputQty ELSE 0 END) AS InputQuantity11,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 11 THEN sm.OutputQty ELSE 0 END) AS OutQuantity11,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 12 THEN sm.InputQty ELSE 0 END) AS InputQuantity12,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 12 THEN sm.OutputQty ELSE 0 END) AS OutQuantity12,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 13 THEN sm.InputQty ELSE 0 END) AS InputQuantity13,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 13 THEN sm.OutputQty ELSE 0 END) AS OutQuantity13,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 14 THEN sm.InputQty ELSE 0 END) AS InputQuantity14,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 14 THEN sm.OutputQty ELSE 0 END) AS OutQuantity14,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 15 THEN sm.InputQty ELSE 0 END) AS InputQuantity15,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 15 THEN sm.OutputQty ELSE 0 END) AS OutQuantity15,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 16 THEN sm.InputQty ELSE 0 END) AS InputQuantity16,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 16 THEN sm.OutputQty ELSE 0 END) AS OutQuantity16,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 17 THEN sm.InputQty ELSE 0 END) AS InputQuantity17,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 17 THEN sm.OutputQty ELSE 0 END) AS OutQuantity17,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 18 THEN sm.InputQty ELSE 0 END) AS InputQuantity18,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 18 THEN sm.OutputQty ELSE 0 END) AS OutQuantity18,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 19 THEN sm.InputQty ELSE 0 END) AS InputQuantity19,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 19 THEN sm.OutputQty ELSE 0 END) AS OutQuantity19,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 20 THEN sm.InputQty ELSE 0 END) AS InputQuantity20,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 20 THEN sm.OutputQty ELSE 0 END) AS OutQuantity20,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 21 THEN sm.InputQty ELSE 0 END) AS InputQuantity21,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 21 THEN sm.OutputQty ELSE 0 END) AS OutQuantity21,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 22 THEN sm.InputQty ELSE 0 END) AS InputQuantity22,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 22 THEN sm.OutputQty ELSE 0 END) AS OutQuantity22,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 23 THEN sm.InputQty ELSE 0 END) AS InputQuantity23,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 23 THEN sm.OutputQty ELSE 0 END) AS OutQuantity23,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 24 THEN sm.InputQty ELSE 0 END) AS InputQuantity24,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 24 THEN sm.OutputQty ELSE 0 END) AS OutQuantity24,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 25 THEN sm.InputQty ELSE 0 END) AS InputQuantity25,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 25 THEN sm.OutputQty ELSE 0 END) AS OutQuantity25,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 26 THEN sm.InputQty ELSE 0 END) AS InputQuantity26,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 26 THEN sm.OutputQty ELSE 0 END) AS OutQuantity26,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 27 THEN sm.InputQty ELSE 0 END) AS InputQuantity27,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 27 THEN sm.OutputQty ELSE 0 END) AS OutQuantity27,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 28 THEN sm.InputQty ELSE 0 END) AS InputQuantity28,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 28 THEN sm.OutputQty ELSE 0 END) AS OutQuantity28,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 29 THEN sm.InputQty ELSE 0 END) AS InputQuantity29,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 29 THEN sm.OutputQty ELSE 0 END) AS OutQuantity29,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 30 THEN sm.InputQty ELSE 0 END) AS InputQuantity30,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 30 THEN sm.OutputQty ELSE 0 END) AS OutQuantity30,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 31 THEN sm.InputQty ELSE 0 END) AS InputQuantity31,
            SUM(CASE WHEN sm.MoveDate >= @StartDate AND sm.MoveDate < @EndDate AND DAY(sm.MoveDate) = 31 THEN sm.OutputQty ELSE 0 END) AS OutQuantity31
        FROM MaterialManufacturerIds mi
        LEFT JOIN OpeningStock os
            ON os.MaterialId = mi.MaterialId
           AND ISNULL(os.ManufacturerId, -1) = ISNULL(mi.ManufacturerId, -1)
        LEFT JOIN StockMove sm
            ON sm.MaterialId = mi.MaterialId
           AND ISNULL(sm.ManufacturerId, -1) = ISNULL(mi.ManufacturerId, -1)
        GROUP BY mi.MaterialId, mi.ManufacturerId, os.OpenStockQuantity, os.OpenStockAmount
    )
    SELECT
        Manufacturer = ISNULL(mf.Name, ''),
        [Mat Code] = m.Code,
        [Mat Name] = m.Name,
        ms.OpenStockQuantity,
        ms.OpenStockAmount,
        ms.InputQuantity,
        ms.InputAmount,
        ms.OutputQuantity,
        ms.OutputAmount,
        InventoryQuantity = ms.OpenStockQuantity + ms.InputQuantity - ms.OutputQuantity,
        InventoryAmount = ms.OpenStockAmount + ms.InputAmount - ms.OutputAmount,
        ms.InputQuantity1,
        ms.OutQuantity1,
        ms.InputQuantity2,
        ms.OutQuantity2,
        ms.InputQuantity3,
        ms.OutQuantity3,
        ms.InputQuantity4,
        ms.OutQuantity4,
        ms.InputQuantity5,
        ms.OutQuantity5,
        ms.InputQuantity6,
        ms.OutQuantity6,
        ms.InputQuantity7,
        ms.OutQuantity7,
        ms.InputQuantity8,
        ms.OutQuantity8,
        ms.InputQuantity9,
        ms.OutQuantity9,
        ms.InputQuantity10,
        ms.OutQuantity10,
        ms.InputQuantity11,
        ms.OutQuantity11,
        ms.InputQuantity12,
        ms.OutQuantity12,
        ms.InputQuantity13,
        ms.OutQuantity13,
        ms.InputQuantity14,
        ms.OutQuantity14,
        ms.InputQuantity15,
        ms.OutQuantity15,
        ms.InputQuantity16,
        ms.OutQuantity16,
        ms.InputQuantity17,
        ms.OutQuantity17,
        ms.InputQuantity18,
        ms.OutQuantity18,
        ms.InputQuantity19,
        ms.OutQuantity19,
        ms.InputQuantity20,
        ms.OutQuantity20,
        ms.InputQuantity21,
        ms.OutQuantity21,
        ms.InputQuantity22,
        ms.OutQuantity22,
        ms.InputQuantity23,
        ms.OutQuantity23,
        ms.InputQuantity24,
        ms.OutQuantity24,
        ms.InputQuantity25,
        ms.OutQuantity25,
        ms.InputQuantity26,
        ms.OutQuantity26,
        ms.InputQuantity27,
        ms.OutQuantity27,
        ms.InputQuantity28,
        ms.OutQuantity28,
        ms.InputQuantity29,
        ms.OutQuantity29,
        ms.InputQuantity30,
        ms.OutQuantity30,
        ms.InputQuantity31,
        ms.OutQuantity31
    FROM MaterialSummary ms
    JOIN [nhvpa3en_vpa01].[CDV_Material] m ON m.Id = ms.MaterialId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Manufacturer] mf ON mf.Id = ms.ManufacturerId
    WHERE ms.OpenStockQuantity <> 0
       OR ms.OpenStockAmount <> 0
       OR ms.InputQuantity <> 0
       OR ms.InputAmount <> 0
       OR ms.OutputQuantity <> 0
       OR ms.OutputAmount <> 0
    ORDER BY m.Code, mf.Name;
END
GO
