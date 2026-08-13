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

    ;WITH StockMove AS
    (
        SELECT
            sid.MaterialId,
            CAST(si.StockInDate AS DATE) AS MoveDate,
            CAST(sid.Qty AS DECIMAL(18,2)) AS InputQty,
            CAST(ISNULL(sid.Qty, 0) * ISNULL(sid.UnitPrice, 0) AS DECIMAL(18,2)) AS InputAmount,
            CAST(0 AS DECIMAL(18,2)) AS OutputQty,
            CAST(0 AS DECIMAL(18,2)) AS OutputAmount
        FROM [nhvpa3en_vpa01].[CDV_StockInDetail] sid
        JOIN [nhvpa3en_vpa01].[CDV_StockIn] si ON si.Id = sid.StockInId
        WHERE si.StockInDate < @EndDate

        UNION ALL

        SELECT
            sod.MaterialId,
            CAST(so.StockOutDate AS DATE) AS MoveDate,
            CAST(0 AS DECIMAL(18,2)) AS InputQty,
            CAST(0 AS DECIMAL(18,2)) AS InputAmount,
            CAST(ISNULL(sod.Qty, 0) AS DECIMAL(18,2)) AS OutputQty,
            CAST(ISNULL(sod.Qty, 0) * ISNULL(sod.UnitPrice, 0) AS DECIMAL(18,2)) AS OutputAmount
        FROM [nhvpa3en_vpa01].[CDV_StockOutDetail] sod
        JOIN [nhvpa3en_vpa01].[CDV_StockOut] so ON so.Id = sod.StockOutId
        WHERE so.StockOutDate < @EndDate
    ),
    MaterialSummary AS
    (
        SELECT
            MaterialId,
            SUM(CASE WHEN MoveDate < @StartDate THEN InputQty - OutputQty ELSE 0 END) AS OpenStockQuantity,
            SUM(CASE WHEN MoveDate < @StartDate THEN InputAmount - OutputAmount ELSE 0 END) AS OpenStockAmount,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate THEN InputQty ELSE 0 END) AS InputQuantity,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate THEN InputAmount ELSE 0 END) AS InputAmount,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate THEN OutputQty ELSE 0 END) AS OutputQuantity,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate THEN OutputAmount ELSE 0 END) AS OutputAmount,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 1 THEN InputQty ELSE 0 END) AS InputQuantity1,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 1 THEN OutputQty ELSE 0 END) AS OutQuantity1,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 2 THEN InputQty ELSE 0 END) AS InputQuantity2,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 2 THEN OutputQty ELSE 0 END) AS OutQuantity2,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 3 THEN InputQty ELSE 0 END) AS InputQuantity3,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 3 THEN OutputQty ELSE 0 END) AS OutQuantity3,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 4 THEN InputQty ELSE 0 END) AS InputQuantity4,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 4 THEN OutputQty ELSE 0 END) AS OutQuantity4,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 5 THEN InputQty ELSE 0 END) AS InputQuantity5,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 5 THEN OutputQty ELSE 0 END) AS OutQuantity5,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 6 THEN InputQty ELSE 0 END) AS InputQuantity6,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 6 THEN OutputQty ELSE 0 END) AS OutQuantity6,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 7 THEN InputQty ELSE 0 END) AS InputQuantity7,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 7 THEN OutputQty ELSE 0 END) AS OutQuantity7,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 8 THEN InputQty ELSE 0 END) AS InputQuantity8,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 8 THEN OutputQty ELSE 0 END) AS OutQuantity8,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 9 THEN InputQty ELSE 0 END) AS InputQuantity9,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 9 THEN OutputQty ELSE 0 END) AS OutQuantity9,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 10 THEN InputQty ELSE 0 END) AS InputQuantity10,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 10 THEN OutputQty ELSE 0 END) AS OutQuantity10,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 11 THEN InputQty ELSE 0 END) AS InputQuantity11,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 11 THEN OutputQty ELSE 0 END) AS OutQuantity11,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 12 THEN InputQty ELSE 0 END) AS InputQuantity12,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 12 THEN OutputQty ELSE 0 END) AS OutQuantity12,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 13 THEN InputQty ELSE 0 END) AS InputQuantity13,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 13 THEN OutputQty ELSE 0 END) AS OutQuantity13,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 14 THEN InputQty ELSE 0 END) AS InputQuantity14,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 14 THEN OutputQty ELSE 0 END) AS OutQuantity14,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 15 THEN InputQty ELSE 0 END) AS InputQuantity15,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 15 THEN OutputQty ELSE 0 END) AS OutQuantity15,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 16 THEN InputQty ELSE 0 END) AS InputQuantity16,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 16 THEN OutputQty ELSE 0 END) AS OutQuantity16,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 17 THEN InputQty ELSE 0 END) AS InputQuantity17,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 17 THEN OutputQty ELSE 0 END) AS OutQuantity17,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 18 THEN InputQty ELSE 0 END) AS InputQuantity18,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 18 THEN OutputQty ELSE 0 END) AS OutQuantity18,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 19 THEN InputQty ELSE 0 END) AS InputQuantity19,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 19 THEN OutputQty ELSE 0 END) AS OutQuantity19,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 20 THEN InputQty ELSE 0 END) AS InputQuantity20,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 20 THEN OutputQty ELSE 0 END) AS OutQuantity20,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 21 THEN InputQty ELSE 0 END) AS InputQuantity21,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 21 THEN OutputQty ELSE 0 END) AS OutQuantity21,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 22 THEN InputQty ELSE 0 END) AS InputQuantity22,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 22 THEN OutputQty ELSE 0 END) AS OutQuantity22,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 23 THEN InputQty ELSE 0 END) AS InputQuantity23,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 23 THEN OutputQty ELSE 0 END) AS OutQuantity23,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 24 THEN InputQty ELSE 0 END) AS InputQuantity24,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 24 THEN OutputQty ELSE 0 END) AS OutQuantity24,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 25 THEN InputQty ELSE 0 END) AS InputQuantity25,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 25 THEN OutputQty ELSE 0 END) AS OutQuantity25,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 26 THEN InputQty ELSE 0 END) AS InputQuantity26,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 26 THEN OutputQty ELSE 0 END) AS OutQuantity26,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 27 THEN InputQty ELSE 0 END) AS InputQuantity27,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 27 THEN OutputQty ELSE 0 END) AS OutQuantity27,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 28 THEN InputQty ELSE 0 END) AS InputQuantity28,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 28 THEN OutputQty ELSE 0 END) AS OutQuantity28,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 29 THEN InputQty ELSE 0 END) AS InputQuantity29,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 29 THEN OutputQty ELSE 0 END) AS OutQuantity29,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 30 THEN InputQty ELSE 0 END) AS InputQuantity30,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 30 THEN OutputQty ELSE 0 END) AS OutQuantity30,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 31 THEN InputQty ELSE 0 END) AS InputQuantity31,
            SUM(CASE WHEN MoveDate >= @StartDate AND MoveDate < @EndDate AND DAY(MoveDate) = 31 THEN OutputQty ELSE 0 END) AS OutQuantity31
        FROM StockMove
        GROUP BY MaterialId
    )
    SELECT
        Supplier = ISNULL(suppliers.Supplier, ''),
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
    OUTER APPLY
    (
        SELECT Supplier = STUFF(
        (
            SELECT DISTINCT ', ' + s.Name
            FROM [nhvpa3en_vpa01].[CDV_StockInDetail] sid
            JOIN [nhvpa3en_vpa01].[CDV_StockIn] si ON si.Id = sid.StockInId
            JOIN [nhvpa3en_vpa01].[CDV_Supplier] s ON s.Id = si.SupplierId
            WHERE sid.MaterialId = ms.MaterialId
              AND si.StockInDate < @EndDate
            FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '')
    ) suppliers
    WHERE ms.OpenStockQuantity <> 0
       OR ms.OpenStockAmount <> 0
       OR ms.InputQuantity <> 0
       OR ms.InputAmount <> 0
       OR ms.OutputQuantity <> 0
       OR ms.OutputAmount <> 0
    ORDER BY m.Code;
END
GO
