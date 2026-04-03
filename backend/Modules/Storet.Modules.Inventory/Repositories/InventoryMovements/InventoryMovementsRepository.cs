using Microsoft.EntityFrameworkCore;
using Storet.Core.Repository;
using Storet.Core.Utils;
using Storet.Modules.Inventory.Data;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Repositories.InventoryMovements;

public class InventoryMovementsRepository : BaseRepository<StoretInventoryDbContext>, IInventoryMovementsRepository
{
	public InventoryMovementsRepository (StoretInventoryDbContext context) : base (context) {}

	public async Task<IEnumerable<InventoryMovement>> GetAllAsync (Query<DateTimeOffset?> query, Guid userId)
	{
		return await context.InventoryMovements
							.AsNoTracking()
							.Where (m => m.UserId == userId)
							.OrderByDescending (m => m.ExecutedAt)
							.PaginateQuery (query, "ExecutedAt")
							.ToListAsync();
	}
	
	public async Task<DateTimeOffset?> GetPreviousKeyAsync (Query<DateTimeOffset?> query, Guid userId)
	{
		if (query.Key == null)
			return null;
		
		var previousCursorLog = await context.InventoryMovements
												.AsNoTracking()
												.Where (m => m.UserId == userId)
												.Where (m => m.ExecutedAt < query.Key)
												.OrderBy (m => m.ExecutedAt)
												.Take (query.PageSize + 1)
												.FirstOrDefaultAsync();
		return previousCursorLog?.ExecutedAt;
	}
	
	public async Task<int> BulkInsertAsync (IEnumerable<InventoryMovement> movements)
	{
		await context.InventoryMovements.AddRangeAsync (movements);
		return await context.SaveChangesAsync();
	}
}