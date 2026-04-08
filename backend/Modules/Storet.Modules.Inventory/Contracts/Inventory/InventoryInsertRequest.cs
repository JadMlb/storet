using System.ComponentModel.DataAnnotations;
using Storet.Core.Validation;

namespace Storet.Modules.Inventory.Contracts.Inventory;

public class InventoryInsertRequest
{
	[Required]
	public Guid ItemId { get; set; }
	[PositiveValue (include: true)]
	public float MinQuantity { get; set; } = 0;
	[PositiveValue]
	public float? MaxQuantity { get; set; }
}