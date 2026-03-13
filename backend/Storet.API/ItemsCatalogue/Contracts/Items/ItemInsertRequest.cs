using System.ComponentModel.DataAnnotations;
using Storet.API.Core.Validation;
using Storet.API.ItemsCatalogue.Contracts.ItemsCompositions;
using Storet.API.ItemsCatalogue.Models;

namespace Storet.API.ItemsCatalogue.Contracts.Items;

public class ItemInsertRequest
{
	[Required (AllowEmptyStrings = false)]
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	[MinCount (1)]
	public ICollection<int> Categories { get; set; } = [];
	public ICollection<ItemCompositionRequest>? Components { get; set; }
}