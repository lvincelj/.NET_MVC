namespace HospitalManagementApp.Models;

public class GlobalSearchViewModel
{
    public string Query { get; set; } = string.Empty;
    public List<GlobalSearchResultItem> Results { get; set; } = [];
}

public class GlobalSearchResultItem
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
}
