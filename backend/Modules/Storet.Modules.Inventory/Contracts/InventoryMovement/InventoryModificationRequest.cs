using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Storet.Core.Mappers;
using Storet.Core.Validation;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Contracts.InventoryMovement;

public class InventoryModificationRequest
{
	public Guid? SourceLocationId { get; set; }
	public Guid? DestinationLocationId { get; set; }
	[PastDateTime]
	public DateTimeOffset ExecutedAt { get; set; } = DateTimeOffset.Now;
	[MinCount (1)]
	public Dictionary<Guid, float> Items { get; set; } = [];
	[JsonConverter (typeof (LowercaseEnumConverter<MovementSource>))]
	[EnumDataType (typeof (MovementSource))]
	public MovementSource? Source { get; set; }
}