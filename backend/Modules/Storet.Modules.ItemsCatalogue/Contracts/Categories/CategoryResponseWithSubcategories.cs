namespace Storet.Modules.ItemsCatalogue.Contracts.Categories;

public class CategoryResponseWithSubCategories : CategoryResponse
{
	public ICollection<CategoryResponseWithSubCategories> SubCategories { get; set; } = [];
}