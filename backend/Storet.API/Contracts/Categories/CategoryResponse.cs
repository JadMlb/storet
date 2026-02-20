namespace Storet.API.Contracts.Categories;

public class CategoryResponse
{
	public int Id { get; set; }
	public string Label { get; set; } = string.Empty;
	public ICollection<CategoryResponse> SubCategories { get; set; } = [];
}