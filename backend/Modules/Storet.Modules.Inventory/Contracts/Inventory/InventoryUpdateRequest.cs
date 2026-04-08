using Storet.Core.Validation;

namespace Storet.Modules.Inventory.Contracts.Inventory;

public class InventoryUpdateRequest
{
	[PositiveValue (include: true)]
	public float? MinQuantity { get; set; }
	[PositiveValue]
	public float? MaxQuantity { get; set; }
}