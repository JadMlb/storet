using Storet.API.ItemsCatalogue.Contracts.ItemsCompositions;

namespace Storet.API.ItemsCatalogue.Contracts.Items;

public class ItemUpdateRequest
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public List<int>? Categories { get; set; }
	public List<ItemCompositionRequest>? Components { get; set; }
}