using Storet.API.ItemsCatalogue.Contracts.Categories;
using Storet.API.ItemsCatalogue.Contracts.ItemsCompositions;
using Storet.API.ItemsCatalogue.Models;

namespace Storet.API.ItemsCatalogue.Contracts.Items;

public class ItemResponseDetails : ItemResponse
{
	public float Quantity { get; set; }
	public Unit Unit { get; set; }
	public List<CategoryResponse> Categories { get; set; } = [];
	public List<ItemCompositionResponse> Components { get; set; } = [];
}