SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'nhvpa3en_vpa01')
BEGIN
    EXEC(N'CREATE SCHEMA [nhvpa3en_vpa01]');
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Dictionary_GetByModule]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Dictionary_GetByModule] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Dictionary_GetByModule]
(
    @Module NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [dbo].[CDV_Dictionary]
    WHERE [Module] = @Module
    ORDER BY [SortOrder], [Id];
END
GO

IF OBJECT_ID(N'[nhvpa3en_vpa01].[CDV_Dictionary_GetByID]', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE [nhvpa3en_vpa01].[CDV_Dictionary_GetByID] AS BEGIN SET NOCOUNT ON; END');
GO

ALTER PROCEDURE [nhvpa3en_vpa01].[CDV_Dictionary_GetByID]
(
    @ID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [dbo].[CDV_Dictionary]
    WHERE [Id] = @ID;
END
GO
