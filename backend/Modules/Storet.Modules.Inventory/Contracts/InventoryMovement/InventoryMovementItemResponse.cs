using Storet.Modules.ItemsCatalogue.Contracts.Items;

namespace Storet.Modules.Inventory.Contracts.InventoryMovement;

public class InventoryMovementItemResponse
{
	public ItemResponseWithUnit Item { get; set; } = null!;
	public float Quantity { get; set; }
}