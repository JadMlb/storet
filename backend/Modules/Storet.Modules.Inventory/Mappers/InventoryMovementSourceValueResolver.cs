using AutoMapper;
using Storet.Modules.Inventory.Contracts.InventoryMovement;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Mappers;

public class InventoryMovementSourceValueResolver : IValueResolver<InventoryModificationRequest, InventoryMovement, MovementSource>
{
	public MovementSource Resolve (InventoryModificationRequest source, InventoryMovement destination, MovementSource destMember, ResolutionContext context)
	{
		if (source.Source.HasValue)
			return source.Source.Value;
		
		if (source.SourceLocationId.HasValue && source.DestinationLocationId.HasValue)
			return MovementSource.Transfer;
		
		// item(s) exiting inventory & assuming "sale" source must be provided (1st condition) => usage case
		if (source.SourceLocationId.HasValue)
			return MovementSource.Usage;
		
		// item(s) entering inventory & assuming "gift" source must be provided (1st condition) => purchase case
		return MovementSource.Purchase;
	}
}