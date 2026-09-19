IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Customer_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Select] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Code,
        Name,
        Address,
        IsActive
    FROM [nhvpa3en_vpa01].[CDV_Customer]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
    ORDER BY Id DESC;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Customer_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Upsert]
    @Id INT = -1,
    @UserId INT = 0,
    @Code VARCHAR(50),
    @Name NVARCHAR(200),
    @Address NVARCHAR(300) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_Customer]
        (
            Code,
            Name,
            Address,
            IsActive
        )
        VALUES
        (
            @Code,
            @Name,
            @Address,
            ISNULL(@IsActive, 1)
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_Customer]
        SET
            Code = @Code,
            Name = @Name,
            Address = @Address,
            IsActive = ISNULL(@IsActive, IsActive)
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_Customer_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Customer_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Customer_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [nhvpa3en_vpa01].[CDV_Customer]
    SET IsActive = 0
    WHERE Id = @Id;

    EXEC [nhvpa3en_vpa01].[CDV_Customer_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Product_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Select] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Code,
        Name,
        IsActive
    FROM [nhvpa3en_vpa01].[CDV_Product]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
    ORDER BY Id DESC;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Product_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Upsert]
    @Id INT = -1,
    @UserId INT = 0,
    @Code VARCHAR(50),
    @Name NVARCHAR(300) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_Product]
        (
            Code,
            Name,
            IsActive
        )
        VALUES
        (
            @Code,
            @Name,
            ISNULL(@IsActive, 1)
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_Product]
        SET
            Code = @Code,
            Name = @Name,
            IsActive = ISNULL(@IsActive, IsActive)
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_Product_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Product_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Product_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [nhvpa3en_vpa01].[CDV_Product]
    SET IsActive = 0
    WHERE Id = @Id;

    EXEC [nhvpa3en_vpa01].[CDV_Product_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Vehicle_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Select] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        VehicleNo,
        DefaultDriverName,
        IsActive
    FROM [nhvpa3en_vpa01].[CDV_Vehicle]
    WHERE (@Id IS NULL OR @Id = 0 OR Id = @Id)
    ORDER BY Id DESC;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Vehicle_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Upsert]
    @Id INT = -1,
    @UserId INT = 0,
    @VehicleNo VARCHAR(50),
    @DefaultDriverName NVARCHAR(100) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_Vehicle]
        (
            VehicleNo,
            DefaultDriverName,
            IsActive
        )
        VALUES
        (
            @VehicleNo,
            @DefaultDriverName,
            ISNULL(@IsActive, 1)
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_Vehicle]
        SET
            VehicleNo = @VehicleNo,
            DefaultDriverName = @DefaultDriverName,
            IsActive = ISNULL(@IsActive, IsActive)
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_Vehicle_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Vehicle_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Vehicle_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [nhvpa3en_vpa01].[CDV_Vehicle]
    SET IsActive = 0
    WHERE Id = @Id;

    EXEC [nhvpa3en_vpa01].[CDV_Vehicle_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNote_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Select] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Select]
    @Id INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        dn.Id,
        dn.DeliveryNo,
        dn.DeliveryDate,
        dn.CustomerId,
        c.Code AS CustomerCode,
        c.Name AS CustomerName,
        dn.VehicleId,
        v.VehicleNo,
        v.DefaultDriverName,
        dn.ReceiverName,
        dn.TotalQuantity,
        dn.Remark,
        dn.CreatedAt,
        dn.CreatedBy,
        dn.UpdatedAt,
        dn.UpdatedBy
    FROM [nhvpa3en_vpa01].[CDV_DeliveryNote] dn
    INNER JOIN [nhvpa3en_vpa01].[CDV_Customer] c ON c.Id = dn.CustomerId
    LEFT JOIN [nhvpa3en_vpa01].[CDV_Vehicle] v ON v.Id = dn.VehicleId
    WHERE (@Id IS NULL OR @Id = 0 OR dn.Id = @Id)
    ORDER BY dn.DeliveryDate DESC, dn.Id DESC;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNote_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Upsert]
    @Id INT = -1,
    @DeliveryNo VARCHAR(50),
    @DeliveryDate DATE,
    @CustomerId INT,
    @VehicleId INT = NULL,
    @ReceiverName NVARCHAR(100) = NULL,
    @TotalQuantity INT = NULL,
    @Remark NVARCHAR(500) = NULL,
    @CreatedBy INT = NULL,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_DeliveryNote]
        (
            DeliveryNo,
            DeliveryDate,
            CustomerId,
            VehicleId,
            ReceiverName,
            TotalQuantity,
            Remark,
            CreatedAt,
            CreatedBy
        )
        VALUES
        (
            @DeliveryNo,
            @DeliveryDate,
            @CustomerId,
            @VehicleId,
            @ReceiverName,
            @TotalQuantity,
            @Remark,
            GETDATE(),
            @CreatedBy
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_DeliveryNote]
        SET
            DeliveryNo = @DeliveryNo,
            DeliveryDate = @DeliveryDate,
            CustomerId = @CustomerId,
            VehicleId = @VehicleId,
            ReceiverName = @ReceiverName,
            TotalQuantity = @TotalQuantity,
            Remark = @Remark,
            UpdatedAt = GETDATE(),
            UpdatedBy = @UpdatedBy
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_DeliveryNote_Select] @Id = @Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNote_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNote_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    WHERE DeliveryNoteId = @Id;

    DELETE FROM [nhvpa3en_vpa01].[CDV_DeliveryNote]
    WHERE Id = @Id;

    SELECT @Id AS Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Select]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Select] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Select]
    @Id INT = 0,
    @DeliveryNoteId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.Id,
        d.DeliveryNoteId,
        d.Seq,
        d.ProductId,
        p.Code AS ProductCode,
        p.Name AS ProductName,
        d.Quantity,
        d.DeliveryTime,
        d.TrolleyBox,
        d.DeliveryTag,
        d.RFIDTag,
        d.Remark
    FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail] d
    INNER JOIN [nhvpa3en_vpa01].[CDV_Product] p ON p.Id = d.ProductId
    WHERE (@Id IS NULL OR @Id = 0 OR d.Id = @Id)
      AND (@DeliveryNoteId IS NULL OR d.DeliveryNoteId = @DeliveryNoteId)
    ORDER BY d.DeliveryNoteId DESC, ISNULL(d.Seq, d.Id), d.Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Upsert]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Upsert] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Upsert]
    @Id INT = -1,
    @DeliveryNoteId INT,
    @Seq INT = NULL,
    @ProductId INT,
    @Quantity INT,
    @DeliveryTime VARCHAR(20) = NULL,
    @TrolleyBox VARCHAR(100) = NULL,
    @DeliveryTag VARCHAR(100) = NULL,
    @RFIDTag VARCHAR(100) = NULL,
    @Remark NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF ISNULL(@Id, -1) <= 0
    BEGIN
        INSERT INTO [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
        (
            DeliveryNoteId,
            Seq,
            ProductId,
            Quantity,
            DeliveryTime,
            TrolleyBox,
            DeliveryTag,
            RFIDTag,
            Remark
        )
        VALUES
        (
            @DeliveryNoteId,
            @Seq,
            @ProductId,
            @Quantity,
            @DeliveryTime,
            @TrolleyBox,
            @DeliveryTag,
            @RFIDTag,
            @Remark
        );

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
        SET
            DeliveryNoteId = @DeliveryNoteId,
            Seq = @Seq,
            ProductId = @ProductId,
            Quantity = @Quantity,
            DeliveryTime = @DeliveryTime,
            TrolleyBox = @TrolleyBox,
            DeliveryTag = @DeliveryTag,
            RFIDTag = @RFIDTag,
            Remark = @Remark
        WHERE Id = @Id;
    END;

    EXEC [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Select] @Id = @Id, @DeliveryNoteId = NULL;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Delete]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Delete] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_Delete]
    @Id INT,
    @UserId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
    WHERE Id = @Id;

    SELECT @Id AS Id;
END;
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_BulkSave]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_BulkSave] AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail_BulkSave]
(
    @DeliveryNoteId INT,
    @UserId INT = 0,
    @DetailsJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_DeliveryNote] WHERE Id = @DeliveryNoteId)
        BEGIN
            SELECT @DeliveryNoteId AS ID, -2 AS ErrCode, 'DELIVERY_NOTE_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        IF ISJSON(@DetailsJson) <> 1
        BEGIN
            SELECT @DeliveryNoteId AS ID, -1 AS ErrCode, 'DETAILS_JSON_INVALID' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        DECLARE @Details TABLE
        (
            Id INT NULL,
            Seq INT NULL,
            ProductId INT NULL,
            Quantity INT NULL,
            DeliveryTime VARCHAR(20) NULL,
            TrolleyBox VARCHAR(100) NULL,
            DeliveryTag VARCHAR(100) NULL,
            RFIDTag VARCHAR(100) NULL,
            Remark NVARCHAR(300) NULL
        );

        INSERT INTO @Details
        (
            Id,
            Seq,
            ProductId,
            Quantity,
            DeliveryTime,
            TrolleyBox,
            DeliveryTag,
            RFIDTag,
            Remark
        )
        SELECT
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.id')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.Id'))
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.seq')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.Seq'))
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.productId')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.ProductId'))
            ),
            COALESCE(
                TRY_CONVERT(INT, JSON_VALUE([value], '$.quantity')),
                TRY_CONVERT(INT, JSON_VALUE([value], '$.Quantity'))
            ),
            COALESCE(
                JSON_VALUE([value], '$.deliveryTime'),
                JSON_VALUE([value], '$.DeliveryTime')
            ),
            COALESCE(
                JSON_VALUE([value], '$.trolleyBox'),
                JSON_VALUE([value], '$.TrolleyBox')
            ),
            COALESCE(
                JSON_VALUE([value], '$.deliveryTag'),
                JSON_VALUE([value], '$.DeliveryTag')
            ),
            COALESCE(
                JSON_VALUE([value], '$.rfidTag'),
                JSON_VALUE([value], '$.RFIDTag'),
                JSON_VALUE([value], '$.RfidTag')
            ),
            COALESCE(
                JSON_VALUE([value], '$.remark'),
                JSON_VALUE([value], '$.Remark')
            )
        FROM OPENJSON(@DetailsJson);

        IF EXISTS (SELECT 1 FROM @Details WHERE ProductId IS NULL OR Quantity IS NULL)
        BEGIN
            SELECT @DeliveryNoteId AS ID, -3 AS ErrCode, 'DETAIL_PRODUCT_QUANTITY_REQUIRED' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM @Details d
            WHERE NOT EXISTS (SELECT 1 FROM [nhvpa3en_vpa01].[CDV_Product] p WHERE p.Id = d.ProductId)
        )
        BEGIN
            SELECT @DeliveryNoteId AS ID, -4 AS ErrCode, 'PRODUCT_NOT_FOUND' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END;

        DELETE target
        FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail] target
        WHERE target.DeliveryNoteId = @DeliveryNoteId
          AND NOT EXISTS
          (
              SELECT 1
              FROM @Details source
              WHERE source.Id IS NOT NULL
                AND source.Id = target.Id
          );

        UPDATE target
        SET
            Seq = source.Seq,
            ProductId = source.ProductId,
            Quantity = source.Quantity,
            DeliveryTime = source.DeliveryTime,
            TrolleyBox = source.TrolleyBox,
            DeliveryTag = source.DeliveryTag,
            RFIDTag = source.RFIDTag,
            Remark = source.Remark
        FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail] target
        JOIN @Details source ON source.Id = target.Id
        WHERE target.DeliveryNoteId = @DeliveryNoteId;

        INSERT INTO [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
        (
            DeliveryNoteId,
            Seq,
            ProductId,
            Quantity,
            DeliveryTime,
            TrolleyBox,
            DeliveryTag,
            RFIDTag,
            Remark
        )
        SELECT
            @DeliveryNoteId,
            source.Seq,
            source.ProductId,
            source.Quantity,
            source.DeliveryTime,
            source.TrolleyBox,
            source.DeliveryTag,
            source.RFIDTag,
            source.Remark
        FROM @Details source
        WHERE ISNULL(source.Id, -1) = -1
           OR NOT EXISTS
           (
               SELECT 1
               FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail] target
               WHERE target.Id = source.Id
                 AND target.DeliveryNoteId = @DeliveryNoteId
           );

        UPDATE [nhvpa3en_vpa01].[CDV_DeliveryNote]
        SET
            TotalQuantity =
            (
                SELECT SUM(Quantity)
                FROM [nhvpa3en_vpa01].[CDV_DeliveryNoteDetail]
                WHERE DeliveryNoteId = @DeliveryNoteId
            ),
            UpdatedAt = GETDATE(),
            UpdatedBy = @UserId
        WHERE Id = @DeliveryNoteId;

        COMMIT TRAN;

        SELECT @DeliveryNoteId AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @DeliveryNoteId AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END;
GO
