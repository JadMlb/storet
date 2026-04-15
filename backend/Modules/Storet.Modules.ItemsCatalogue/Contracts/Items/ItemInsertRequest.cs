using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Storet.Core.Mappers;
using Storet.Core.Validation;
using Storet.Modules.ItemsCatalogue.Contracts.ItemsCompositions;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Contracts.Items;

public class ItemInsertRequest
{
	[Required (AllowEmptyStrings = false)]
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	[PositiveValue]
	public float Quantity { get; set; }
	[Required]
	[JsonConverter (typeof (LowercaseEnumConverter<Unit>))]
	[EnumDataType (typeof (Unit))]
	public Unit Unit { get; set; }
	[MinCount (1)]
	public ICollection<int> Categories { get; set; } = [];
	public ICollection<ItemCompositionRequest>? Components { get; set; }
}