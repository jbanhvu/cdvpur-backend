namespace ChangdaeVinaPurchasingApi.Dtos;

public class MenuChildDto
{
    public string Title { get; set; } = string.Empty;
    public string? Path { get; set; }
    public string[] Permissions { get; set; } = [];
}
