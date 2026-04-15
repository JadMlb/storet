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
							.Include (m => m.StorageLocation)
							.PaginateQuery (query, "ExecutedAt", reverseOrder: true)
							.ToListAsync();
	}
	
	public async Task<DateTimeOffset?> GetPreviousKeyAsync (Query<DateTimeOffset?> query, Guid userId)
	{
		if (query.Key == null)
			return null;
		
		var previousCursorLog = await context.InventoryMovements
												.AsNoTracking()
												.Where (m => m.UserId == userId)
												.Where (m => m.ExecutedAt > query.Key)
												.OrderBy (m => m.ExecutedAt)
												.Take (query.PageSize + 1)
												.FirstOrDefaultAsync();
		return previousCursorLog?.ExecutedAt;
	}
	
	public async Task<InventoryMovement?> GetOneAsync (Guid id, Guid userId)
	{
		return await context.InventoryMovements
							.AsNoTracking()
							.Include (m => m.Items)
							.Include (m => m.StorageLocation)
							.FirstOrDefaultAsync (m => m.Id == id && m.UserId == userId);
	}
	
	public async Task<InventoryMovement?> InsertAsync (InventoryMovement model)
	{
		await context.InventoryMovements.AddAsync (model);
		await context.SaveChangesAsync();
		
		return await context.InventoryMovements
							.AsNoTracking()
							.Include (m => m.StorageLocation)
							.FirstOrDefaultAsync (m => m.Id == model.Id && m.UserId == m.UserId);
	}
}