namespace Storet.Modules.Inventory.Models;

public class Inventory
{
	public Guid ItemId { get; set; }
	public Guid UserId { get; set; }
	public float QuantityInStock { get; set; }
	public float MinQuantity { get; set; }
	public float? MaxQuantity { get; set; }
	public Status Status { get; set; }
}