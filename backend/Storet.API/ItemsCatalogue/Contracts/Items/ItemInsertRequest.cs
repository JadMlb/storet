using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Storet.API.Core.Validation;
using Storet.API.ItemsCatalogue.Contracts.ItemsCompositions;
using Storet.API.ItemsCatalogue.Mappers;
using Storet.API.ItemsCatalogue.Models;

namespace Storet.API.ItemsCatalogue.Contracts.Items;

public class ItemInsertRequest
{
	[Required (AllowEmptyStrings = false)]
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	[PositiveValue]
	public float Quantity { get; set; }
	[Required]
	[JsonConverter (typeof (LowercaseUnitConverter))]
	[EnumDataType (typeof (Unit))]
	public Unit Unit { get; set; }
	[MinCount (1)]
	public ICollection<int> Categories { get; set; } = [];
	public ICollection<ItemCompositionRequest>? Components { get; set; }
}