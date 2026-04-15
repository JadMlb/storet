using Storet.Core.Repository;
using Storet.Modules.Inventory.Data;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Repositories.InventoryMovementItems;

public class InventoryMovementItemsRepository : BaseRepository<StoretInventoryDbContext>, IInventoryMovementItemsRepository
{
	public InventoryMovementItemsRepository (StoretInventoryDbContext context) : base (context) {}
	
	public async Task<IEnumerable<InventoryMovementItem>> BulkInsertAsync (IEnumerable<InventoryMovementItem> items)
	{
		await context.AddRangeAsync (items);
		await context.SaveChangesAsync();
		return items;
	}
}