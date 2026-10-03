IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_User_UpdateAvatar]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_User_UpdateAvatar] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_User_UpdateAvatar]
(
    @Id INT,
    @AvatarUrl NVARCHAR(500),
    @UpdatedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [nhvpa3en_vpa01].[CDV_User]
    SET
        AvatarUrl = @AvatarUrl,
        UpdatedAt = GETDATE(),
        UpdatedBy = COALESCE(@UpdatedBy, UpdatedBy)
    WHERE Id = @Id;

    EXEC [nhvpa3en_vpa01].[CDV_User_Select] @Id = @Id;
END
GO
