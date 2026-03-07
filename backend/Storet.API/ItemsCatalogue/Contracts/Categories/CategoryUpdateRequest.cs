namespace Storet.API.ItemsCatalogue.Contracts.Categories;

public class CategoryUpdateRequest
{
	public string? Label { get; set; }
	public int? ParentCategoryId { get; set; }
}