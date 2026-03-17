using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Storet.API.Core.Validation;
using Storet.API.ItemsCatalogue.Contracts.ItemsCompositions;
using Storet.API.ItemsCatalogue.Mappers;
using Storet.API.ItemsCatalogue.Models;

namespace Storet.API.ItemsCatalogue.Contracts.Items;

public class ItemUpdateRequest
{
	[MinLength (1)]
	public string? Name { get; set; }
	public string? Description { get; set; }
	[PositiveValue]
	public float? Quantity { get; set; }
	[JsonConverter (typeof (LowercaseUnitConverter))]
	[EnumDataType (typeof (Unit))]
	public Unit? Unit { get; set; }
	public List<int>? Categories { get; set; }
	public List<ItemCompositionRequest>? Components { get; set; }
}