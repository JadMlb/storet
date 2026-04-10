using System.ComponentModel.DataAnnotations;

namespace Storet.Modules.Inventory.Contracts.StorageLocation;

public class StorageLocationUpdateRequest
{
	public string? Name { get; set; }
	public string? Description { get; set; }
}