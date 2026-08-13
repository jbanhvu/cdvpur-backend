namespace ChangdaeVinaPurchasingApi.Dtos;

public class MenuTreeDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public List<MenuChildDto> Children { get; set; } = [];
}
