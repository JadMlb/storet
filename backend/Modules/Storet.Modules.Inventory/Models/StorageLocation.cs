using System.ComponentModel.DataAnnotations;

namespace Storet.Modules.Inventory.Models;

public class StorageLocation
{
	public Guid Id { get; set; }
	[Required (AllowEmptyStrings = false)]
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public Guid UserId { get; set; }
}