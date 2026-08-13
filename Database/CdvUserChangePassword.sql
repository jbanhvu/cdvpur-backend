SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[Cdv_User_ChangePassword]
(
    @UserId INT,
    @OldPasswordHash VARCHAR(500),
    @NewPasswordHash VARCHAR(500)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_User]
            WHERE Id = @UserId
        )
        BEGIN
            SELECT @UserId AS ID, 404 AS ErrCode, 'User not found' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        IF NOT EXISTS
        (
            SELECT 1
            FROM [nhvpa3en_vpa01].[CDV_User]
            WHERE Id = @UserId
              AND PasswordHash = @OldPasswordHash
        )
        BEGIN
            SELECT @UserId AS ID, 401 AS ErrCode, 'Old password incorrect' AS ErrMsg;
            ROLLBACK TRAN;
            RETURN;
        END

        UPDATE [nhvpa3en_vpa01].[CDV_User]
        SET PasswordHash = @NewPasswordHash
        WHERE Id = @UserId;

        IF OBJECT_ID(N'[nhvpa3en_vpa01].[Sol_AuditLog]', N'U') IS NOT NULL
        BEGIN
            INSERT INTO [nhvpa3en_vpa01].[Sol_AuditLog]
            (
                UserId,
                TableName,
                ActionType,
                RecordId,
                NewData,
                CreatedAt
            )
            VALUES
            (
                @UserId,
                'CDV_User',
                'CHANGE_PASSWORD',
                @UserId,
                'Password changed',
                GETDATE()
            );
        END

        COMMIT TRAN;

        SELECT @UserId AS ID, 0 AS ErrCode, 'SUCCESS' AS ErrMsg;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SELECT @UserId AS ID, ERROR_NUMBER() AS ErrCode, ERROR_MESSAGE() AS ErrMsg;
    END CATCH
END
GO
