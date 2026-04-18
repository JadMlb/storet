using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Storet.Core.Mappers;
using Storet.Modules.Inventory.Contracts.StorageLocation;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Contracts.InventoryMovement;

public class InventoryMovementResponse
{
	public Guid Id { get; set; }
	public StorageLocationResponse? Location { get; set; }
	public int NumberOfItems { get; set; }
	public DateTimeOffset ExecutedAt { get; set; }
	[JsonConverter (typeof (LowercaseEnumConverter<MovementDirection>))]
	[EnumDataType (typeof (MovementDirection))]
	public MovementDirection Direction { get; set; }
	[JsonConverter (typeof (LowercaseEnumConverter<MovementSource>))]
	[EnumDataType (typeof (MovementSource))]
	public MovementSource Source { get; set; }
}