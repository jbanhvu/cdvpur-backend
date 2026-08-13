using ChangdaeVinaPurchasingApi.Dtos;
using ChangdaeVinaPurchasingApi.Repositories;

namespace ChangdaeVinaPurchasingApi.Services;

public class RoleFunctionPermissionService
{
    private readonly RoleFunctionPermissionRepository _repository;

    public RoleFunctionPermissionService(RoleFunctionPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MenuTreeDto>> GetMenuTreeByUserIdAsync(int userId)
    {
        List<MenuFlatDto> menus = await _repository.GetMenuFlatByUserIdAsync(userId);

        return BuildMenuTree(menus);
    }

    public List<MenuTreeDto> BuildMenuTree(IEnumerable<MenuFlatDto> flatMenus)
    {
        List<MenuFlatDto> menus = flatMenus.ToList();
        ILookup<int?, MenuFlatDto> menusByParentId = menus.ToLookup(menu => menu.ParentId);

        return menusByParentId[null]
            .OrderBy(menu => menu.SortOrder ?? int.MaxValue)
            .ThenBy(menu => menu.Id)
            .Select(parent => new MenuTreeDto
            {
                Key = parent.Module ?? string.Empty,
                Title = parent.Name ?? string.Empty,
                Icon = parent.Icon,
                Children = menusByParentId[GetMenuId(parent)]
                    .OrderBy(child => child.SortOrder ?? int.MaxValue)
                    .ThenBy(child => child.Id)
                    .Select(child => new MenuChildDto
                    {
                        Title = child.Name ?? string.Empty,
                        Path = child.Route,
                        Permissions = ParsePermissions(child.ListPermission)
                    })
                    .ToList()
            })
            .ToList();
    }

    private static int GetMenuId(MenuFlatDto menu)
    {
        return menu.FunctionId ?? menu.Id;
    }

    private static string[] ParsePermissions(string? listPermission)
    {
        if (string.IsNullOrWhiteSpace(listPermission))
        {
            return [];
        }

        return listPermission
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .ToArray();
    }
}
