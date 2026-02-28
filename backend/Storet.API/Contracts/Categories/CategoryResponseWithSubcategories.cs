namespace Storet.API.Contracts.Categories;

public class CategoryResponseWithSubCategories : CategoryResponse
{
	public ICollection<CategoryResponseWithSubCategories> SubCategories { get; set; } = [];
}