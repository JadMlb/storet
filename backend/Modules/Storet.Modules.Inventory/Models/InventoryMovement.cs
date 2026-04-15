namespace Storet.Modules.Inventory.Models;

public class InventoryMovement
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public Guid StorageLocationId { get; set; }
	public StorageLocation StorageLocation { get; set; } = null!;
	public List<InventoryMovementItem> Items { get; set; } = [];
	public int NumberOfItems { get; set; }
	public DateTimeOffset ExecutedAt { get; set; } = DateTimeOffset.Now;
	public MovementDirection Direction { get; set; }
	public MovementSource Source { get; set; }
}