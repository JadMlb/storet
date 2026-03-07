namespace Storet.API.ItemsCatalogue.Contracts.Categories;

public class CategoryResponseWithParent : CategoryResponseWithSubCategories
{
	public CategoryResponseWithSubCategories? ParentCategory { get; set; }
}