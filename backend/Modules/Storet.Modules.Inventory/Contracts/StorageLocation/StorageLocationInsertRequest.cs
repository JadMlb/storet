using System.ComponentModel.DataAnnotations;

namespace Storet.Modules.Inventory.Contracts.StorageLocation;

public class StorageLocationInsertRequest
{
	[Required (AllowEmptyStrings = false)]
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
}