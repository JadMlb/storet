namespace Storet.API.Contracts.Categories;

public class CategoryResponseWithParent : CategoryResponseWithSubCategories
{
	public CategoryResponseWithSubCategories? ParentCategory { get; set; }
}