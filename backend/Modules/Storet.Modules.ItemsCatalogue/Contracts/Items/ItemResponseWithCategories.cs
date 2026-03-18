using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Storet.Modules.ItemsCatalogue.Contracts.Categories;
using Storet.Modules.ItemsCatalogue.Contracts.ItemsCompositions;
using Storet.Modules.ItemsCatalogue.Mappers;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Contracts.Items;

public class ItemResponseDetails : ItemResponse
{
	public float Quantity { get; set; }
	[JsonConverter (typeof (LowercaseUnitConverter))]
	[EnumDataType (typeof (Unit))]
	public Unit Unit { get; set; }
	public List<CategoryResponse> Categories { get; set; } = [];
	public List<ItemCompositionResponse> Components { get; set; } = [];
}