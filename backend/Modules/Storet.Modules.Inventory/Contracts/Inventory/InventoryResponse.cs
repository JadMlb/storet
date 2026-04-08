using Storet.Modules.Inventory.Models;
using Storet.Modules.ItemsCatalogue.Contracts.Items;

namespace Storet.Modules.Inventory.Contracts.Inventory;

public class InventoryResponse
{
	public ItemResponse Item { get; set; } = null!;
	public float QuantityInStock { get; set; }
	public float MinQuantity { get; set; }
	public float? MaxQuantity { get; set; }
	public Status Status { get; set; }
}