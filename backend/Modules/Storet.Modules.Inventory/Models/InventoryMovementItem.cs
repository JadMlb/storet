namespace Storet.Modules.Inventory.Models;

public class InventoryMovementItem
{
	public Guid MovementId { get; set; }
	public Guid ItemId { get; set; }
	public Guid UserId { get; set; }
	public float Quantity { get; set; }
	public short Ordinal { get; set; }
}