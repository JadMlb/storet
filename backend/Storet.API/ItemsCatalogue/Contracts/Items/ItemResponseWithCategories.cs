using Storet.API.ItemsCatalogue.Contracts.Categories;
using Storet.API.ItemsCatalogue.Contracts.ItemsCompositions;

namespace Storet.API.ItemsCatalogue.Contracts.Items;

public class ItemResponseDetails : ItemResponse
{
	public List<CategoryResponse> Categories { get; set; } = [];
	public List<ItemCompositionResponse> Components { get; set; } = [];
}