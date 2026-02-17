namespace Storet.Backend.Contracts.Categories;

public class CategoryRequest
{
	public int Id { get; set; }
	public string Label { get; set; } = string.Empty;
	public CategoryRequest? ParentCategory { get; set; }
	public IEnumerable<CategoryRequest> SubCategories { get; set; } = [];
}