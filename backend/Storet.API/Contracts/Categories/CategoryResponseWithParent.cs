namespace Storet.API.Contracts.Categories;

public class CategoryResponseWithParent : CategoryResponse
{
	public CategoryResponse? ParentCategory { get; set; }
}