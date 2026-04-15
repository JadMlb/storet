using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Storet.Core.Mappers;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Contracts.Items;

public class ItemResponseWithUnit : ItemResponse
{
	[JsonConverter (typeof (LowercaseEnumConverter<Unit>))]
	[EnumDataType (typeof (Unit))]
	public Unit Unit { get; set; }
	public float Quantity { get; set; }
}