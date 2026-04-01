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
	
	public async Task<bool> ExistsAsync (Guid key, Guid userId)
	{
		return await context.Inventories
							.AsNoTracking()
							.AnyAsync (i => i.ItemId == key && i.UserId == userId);
	}
	
	public async Task<Models.Inventory?> InsertAsync (Models.Inventory model)
	{
		await context.Inventories.AddAsync (model);
		await context.SaveChangesAsync();
		return model;
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
	
	public async Task<bool> DeleteAsync (Guid key, Guid userId)
	{
		var old = await context.Inventories.FirstOrDefaultAsync (i => i.ItemId == key && i.UserId == userId);
		if (old == null)
			return false;
			
		context.Inventories.Remove (old);
		await context.SaveChangesAsync();
		
		return true;
	}
}