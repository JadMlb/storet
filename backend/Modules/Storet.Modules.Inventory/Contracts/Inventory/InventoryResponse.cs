using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Storet.Core.Mappers;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Contracts.Inventory;

public class InventoryResponse
{
	public Guid ItemId { get; set; }
	public float QuantityInStock { get; set; }
	public float MinQuantity { get; set; }
	public float? MaxQuantity { get; set; }
	[JsonConverter (typeof (LowercaseEnumConverter<Status>))]
	[EnumDataType (typeof (Status))]
	public Status Status { get; set; }
}