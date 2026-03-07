namespace Storet.API.ItemsCatalogue.Contracts.Categories;

public class CategoryResponseWithSubCategories : CategoryResponse
{
	public ICollection<CategoryResponseWithSubCategories> SubCategories { get; set; } = [];
}