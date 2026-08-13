SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DECLARE @HumanResourceFunctionId INT;
DECLARE @LeaveReportFunctionId INT;
DECLARE @LeaveDetailReportFunctionId INT;
DECLARE @HumanResourceName NVARCHAR(100) =
    NCHAR(78) + NCHAR(104) + NCHAR(226) + NCHAR(110) + NCHAR(32) + NCHAR(115) + NCHAR(7921);
DECLARE @LeaveReportName NVARCHAR(100) =
    NCHAR(66) + NCHAR(225) + NCHAR(111) + NCHAR(32) +
    NCHAR(99) + NCHAR(225) + NCHAR(111) + NCHAR(32) +
    NCHAR(110) + NCHAR(103) + NCHAR(104) + NCHAR(7881) + NCHAR(32) +
    NCHAR(112) + NCHAR(104) + NCHAR(233) + NCHAR(112);
DECLARE @LeaveDetailReportName NVARCHAR(100) =
    NCHAR(66) + NCHAR(225) + NCHAR(111) + NCHAR(32) +
    NCHAR(99) + NCHAR(225) + NCHAR(111) + NCHAR(32) +
    NCHAR(99) + NCHAR(104) + NCHAR(105) + NCHAR(32) +
    NCHAR(116) + NCHAR(105) + NCHAR(7871) + NCHAR(116) + NCHAR(32) +
    NCHAR(110) + NCHAR(103) + NCHAR(104) + NCHAR(7881) + NCHAR(32) +
    NCHAR(112) + NCHAR(104) + NCHAR(233) + NCHAR(112);

IF NOT EXISTS
(
    SELECT 1
    FROM [nhvpa3en_vpa01].[CDV_Function]
    WHERE Code = 'HR'
)
BEGIN
    INSERT INTO [nhvpa3en_vpa01].[CDV_Function]
    (
        Code,
        ParentId,
        Name,
        Module,
        Route,
        Icon,
        SortOrder,
        FeatureType,
        IsMenu,
        IsActive
    )
    VALUES
    (
        'HR',
        NULL,
        @HumanResourceName,
        'human-resources',
        NULL,
        'users-round',
        30,
        'MENU',
        1,
        1
    );
END

SELECT @HumanResourceFunctionId = Id
FROM [nhvpa3en_vpa01].[CDV_Function]
WHERE Code = 'HR';

IF NOT EXISTS
(
    SELECT 1
    FROM [nhvpa3en_vpa01].[CDV_Function]
    WHERE Code = 'HRLEAVE'
)
BEGIN
    INSERT INTO [nhvpa3en_vpa01].[CDV_Function]
    (
        Code,
        ParentId,
        Name,
        Module,
        Route,
        Icon,
        SortOrder,
        FeatureType,
        IsMenu,
        IsActive
    )
    VALUES
    (
        'HRLEAVE',
        @HumanResourceFunctionId,
        @LeaveReportName,
        'employee-leave-summary',
        '/reports/employee-leave-summary',
        'calendar-days',
        1,
        'MENU',
        1,
        1
    );
END
ELSE
BEGIN
    UPDATE [nhvpa3en_vpa01].[CDV_Function]
    SET
        ParentId = @HumanResourceFunctionId,
        Name = @LeaveReportName,
        Module = 'employee-leave-summary',
        Route = '/reports/employee-leave-summary',
        Icon = 'calendar-days',
        SortOrder = 1,
        FeatureType = 'MENU',
        IsMenu = 1,
        IsActive = 1
    WHERE Code = 'HRLEAVE';
END

UPDATE [nhvpa3en_vpa01].[CDV_Function]
SET Name = @HumanResourceName
WHERE Code = 'HR';

IF NOT EXISTS
(
    SELECT 1
    FROM [nhvpa3en_vpa01].[CDV_Function]
    WHERE Code = 'HRLEAVEDTL'
)
BEGIN
    INSERT INTO [nhvpa3en_vpa01].[CDV_Function]
    (
        Code,
        ParentId,
        Name,
        Module,
        Route,
        Icon,
        SortOrder,
        FeatureType,
        IsMenu,
        IsActive
    )
    VALUES
    (
        'HRLEAVEDTL',
        @HumanResourceFunctionId,
        @LeaveDetailReportName,
        'employee-leaves',
        '/reports/employee-leaves',
        'list-checks',
        2,
        'MENU',
        1,
        1
    );
END
ELSE
BEGIN
    UPDATE [nhvpa3en_vpa01].[CDV_Function]
    SET
        ParentId = @HumanResourceFunctionId,
        Name = @LeaveDetailReportName,
        Module = 'employee-leaves',
        Route = '/reports/employee-leaves',
        Icon = 'list-checks',
        SortOrder = 2,
        FeatureType = 'MENU',
        IsMenu = 1,
        IsActive = 1
    WHERE Code = 'HRLEAVEDTL';
END

SELECT @LeaveDetailReportFunctionId = Id
FROM [nhvpa3en_vpa01].[CDV_Function]
WHERE Code = 'HRLEAVEDTL';

SELECT @LeaveReportFunctionId = Id
FROM [nhvpa3en_vpa01].[CDV_Function]
WHERE Code = 'HRLEAVE';

IF NOT EXISTS
(
    SELECT 1
    FROM [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
    WHERE RoleId = 1
      AND FunctionId = @HumanResourceFunctionId
)
BEGIN
    INSERT INTO [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
    (
        RoleId,
        FunctionId,
        ListPermission,
        ListApprovalRole
    )
    VALUES
    (
        1,
        @HumanResourceFunctionId,
        'VIE',
        NULL
    );
END

IF NOT EXISTS
(
    SELECT 1
    FROM [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
    WHERE RoleId = 1
      AND FunctionId = @LeaveReportFunctionId
)
BEGIN
    INSERT INTO [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
    (
        RoleId,
        FunctionId,
        ListPermission,
        ListApprovalRole
    )
    VALUES
    (
        1,
        @LeaveReportFunctionId,
        'VIE,EXP',
        NULL
    );
END

IF NOT EXISTS
(
    SELECT 1
    FROM [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
    WHERE RoleId = 1
      AND FunctionId = @LeaveDetailReportFunctionId
)
BEGIN
    INSERT INTO [nhvpa3en_vpa01].[CDV_RoleFunctionPermission]
    (
        RoleId,
        FunctionId,
        ListPermission,
        ListApprovalRole
    )
    VALUES
    (
        1,
        @LeaveDetailReportFunctionId,
        'VIE,EXP',
        NULL
    );
END
GO
