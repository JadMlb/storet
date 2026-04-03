using Storet.Core.Repository;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Repositories.InventoryMovements;

public interface IInventoryMovementsRepository : IPaginatedListRetrievable<InventoryMovement, Guid, DateTimeOffset?>
{
	public Task<int> BulkInsertAsync (IEnumerable<InventoryMovement> movements);
}