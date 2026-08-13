namespace ChangdaeVinaPurchasingApi.Dtos;

public class MenuFlatDto
{
    public int Id { get; set; }
    public int? FunctionId { get; set; }
    public string? Module { get; set; }
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int? ParentId { get; set; }
    public string? ListPermission { get; set; }
    public int? SortOrder { get; set; }
}
