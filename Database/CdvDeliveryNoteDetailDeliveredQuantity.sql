IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_DeliveredQuantityByDO_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_DeliveredQuantityByDO_Select] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_DeliveredQuantityByDO_Select]
(
    @DOListJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH InputDO AS
    (
        SELECT DISTINCT
            LTRIM(RTRIM([value])) AS DELIVERY
        FROM OPENJSON(@DOListJson)
        WHERE NULLIF(LTRIM(RTRIM([value])), '') IS NOT NULL
    ),
    Delivered AS
    (
        SELECT
            LTRIM(RTRIM(DELIVERY)) AS DELIVERY,
            SUM(ISNULL(Quantity, 0)) AS DeliveredQuantity
        FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
        WHERE DELIVERY IS NOT NULL
        GROUP BY LTRIM(RTRIM(DELIVERY))
    )
    SELECT
        i.DELIVERY,
        ISNULL(d.DeliveredQuantity, 0) AS DeliveredQuantity
    FROM InputDO i
    LEFT JOIN Delivered d ON d.DELIVERY = i.DELIVERY
    ORDER BY i.DELIVERY;
END
GO
