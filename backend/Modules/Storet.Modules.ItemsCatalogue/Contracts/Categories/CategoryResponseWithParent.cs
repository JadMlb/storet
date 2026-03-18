namespace Storet.Modules.ItemsCatalogue.Contracts.Categories;

public class CategoryResponseWithParent : CategoryResponseWithSubCategories
{
	public CategoryResponseWithSubCategories? ParentCategory { get; set; }
}