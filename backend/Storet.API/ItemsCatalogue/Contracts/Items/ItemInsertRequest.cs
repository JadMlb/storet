using System.ComponentModel.DataAnnotations;
using Storet.API.Core.Validation;

namespace Storet.API.ItemsCatalogue.Contracts.Items;

public class ItemInsertRequest
{
	[Required (AllowEmptyStrings = false)]
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	[MinCount (1)]
	public List<int> Categories { get; set; } = [];
}