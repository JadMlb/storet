using AutoMapper;
using Storet.Modules.Inventory.Contracts.InventoryMovement;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Mappers;

public class InventoryMovementNumberOfItemsValueResolver : IValueResolver<InventoryModificationRequest, InventoryMovement, int>
{
	public int Resolve (InventoryModificationRequest source, InventoryMovement destination, int destMember, ResolutionContext context)
	{
		return source.Items.Count;
	}
}