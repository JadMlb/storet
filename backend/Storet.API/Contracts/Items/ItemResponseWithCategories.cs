using Storet.API.Contracts.Categories;

namespace Storet.API.Contracts.Items;

public class ItemResponseWithCategories : ItemResponse
{
	public List<CategoryResponse> Categories { get; set; } = [];
}