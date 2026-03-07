using Storet.API.ItemsCatalogue.Contracts.Categories;

namespace Storet.API.ItemsCatalogue.Contracts.Items;

public class ItemResponseWithCategories : ItemResponse
{
	public List<CategoryResponse> Categories { get; set; } = [];
}