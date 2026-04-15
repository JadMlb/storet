using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Repositories.InventoryMovementItems;

public interface IInventoryMovementItemsRepository
{
	public Task<IEnumerable<InventoryMovementItem>> BulkInsertAsync (IEnumerable<InventoryMovementItem> items);
}
