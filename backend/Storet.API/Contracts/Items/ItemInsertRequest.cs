using System.ComponentModel.DataAnnotations;

namespace Storet.API.Contracts.Items;

public class ItemInsertRequest
{
	[Required (AllowEmptyStrings = false)]
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	[MinLength (1)]
	public List<int> Categories { get; set; } = [];
}