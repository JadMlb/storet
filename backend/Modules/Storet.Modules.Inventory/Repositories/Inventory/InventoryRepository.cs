using Microsoft.EntityFrameworkCore;
using Storet.Core.Repository;
using Storet.Modules.Inventory.Data;

namespace Storet.Modules.Inventory.Repositories.Inventory;

public class InventoryRepository : BaseRepository<StoretInventoryDbContext>, IInventoryRepository
{
	public InventoryRepository (StoretInventoryDbContext context) : base (context) {}
	
	
	public async Task<IEnumerable<Models.Inventory>> GetAllAsync (Guid userId)
	{
		return await context.Inventories
							.AsNoTracking()
							.Where (i => i.UserId == userId)
							.ToListAsync();
	}
	
	public async Task<Models.Inventory?> GetOneAsync (Guid itemId, Guid userId)
	{
		return await context.Inventories
							.AsNoTracking()
							.FirstOrDefaultAsync (i => i.UserId == userId && i.ItemId == itemId);
	}
	
	public async Task<Dictionary<Guid, Models.Inventory>> GetAllFromListAsync (Guid userId, IEnumerable<Guid> ids)
	{
		return await context.Inventories
							.AsNoTracking()
							.Where (i => i.UserId == userId && ids.Contains (i.ItemId))
							.ToDictionaryAsync (i => i.ItemId);
	}
	
	public async Task<bool> ExistsAsync (Guid key, Guid userId)
	{
		return await context.Inventories
							.AsNoTracking()
							.AnyAsync (i => i.ItemId == key && i.UserId == userId);
	}
	
	public async Task<int> BulkInsertAsync (IEnumerable<Models.Inventory> models)
	{
		await context.Inventories.AddRangeAsync (models);
		return await context.SaveChangesAsync();
	}
	
	public async Task<Models.Inventory?> UpdateAsync (Guid key, Guid userId, Models.Inventory model)
	{
		var oldValue = await context.Inventories
									.FirstOrDefaultAsync (i => i.ItemId == key && i.UserId == userId);
		
		if (oldValue == null)
			return null;
			
		oldValue.MaxQuantity = model.MaxQuantity;
		oldValue.MinQuantity = model.MinQuantity;
		oldValue.QuantityInStock = model.QuantityInStock;
		oldValue.Status = model.Status;
		
		await context.SaveChangesAsync();
		
		return oldValue;
	}
	
	public async Task<int> BulkUpdateAsync (Guid userId, IEnumerable<Models.Inventory> values)
	{
		var ids = values.Select (i => i.ItemId);
		var oldValues = await context.Inventories
										.Where (i => ids.Contains (i.ItemId))
										.Where (i => i.UserId == userId)
										.ToListAsync();
		var updates = values.ToDictionary (i => i.ItemId);
		foreach (var value in oldValues)
		{
			if (value == null || !updates.TryGetValue (value.ItemId, out var updatedValue))
				continue;
			value.MaxQuantity = updatedValue.MaxQuantity;
			value.MinQuantity = updatedValue.MinQuantity;
			value.QuantityInStock = updatedValue.QuantityInStock;
			value.Status = updatedValue.Status;
		}
		
		return await context.SaveChangesAsync();
	}
	
	public async Task<bool> BulkDeleteAsync (Guid userId, IEnumerable<Guid> itemIds)
	{
		var deletedRows = await context.Inventories
										.Where (i => i.UserId == userId && itemIds.Contains (i.ItemId))
										.ExecuteDeleteAsync();
		return deletedRows == itemIds.Count();
	}
}