using Storet.Modules.ItemsCatalogue.Contracts.Categories;
using Storet.Modules.ItemsCatalogue.Contracts.ItemsCompositions;

namespace Storet.Modules.ItemsCatalogue.Contracts.Items;

public class ItemResponseDetails : ItemResponseWithUnit
{
	public List<CategoryResponse> Categories { get; set; } = [];
	public List<ItemCompositionResponse> Components { get; set; } = [];
}