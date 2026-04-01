using Microsoft.EntityFrameworkCore;
using Storet.Core.Repository;
using Storet.Modules.Inventory.Data;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Repositories.StorageLocations;

public class StorageLocationsRepository : BaseRepository<StoretInventoryDbContext>, IStorageLocationsRepository
{
	public StorageLocationsRepository (StoretInventoryDbContext context) : base (context) {}
	
	public async Task<bool> ExistsAsync (Guid key, Guid userId)
	{
		return await context.StorageLocations
							.AsNoTracking()
							.AnyAsync (s => s.Id == key && s.UserId == userId);
	}
	
	public async Task<IEnumerable<StorageLocation>> GetAllAsync (Guid userId)
	{
		return await context.StorageLocations
							.AsNoTracking()
							.Where (s => s.UserId == userId)
							.ToListAsync();
	}
	
	public async Task<StorageLocation?> GetOneAsync (Guid key, Guid userId)
	{
		return await context.StorageLocations
							.AsNoTracking()
							.FirstOrDefaultAsync (s => s.Id == key && s.UserId == userId);
	}
	
	public async Task<StorageLocation?> InsertAsync (StorageLocation model)
	{
		await context.StorageLocations.AddAsync (model);
		await context.SaveChangesAsync();
		return model;
	}
	
	public async Task<StorageLocation?> UpdateAsync (Guid key, Guid userId, StorageLocation model)
	{
		var existing = await context.StorageLocations.FirstOrDefaultAsync (s => s.Id == key && s.UserId == userId);
		if (existing == null)
			return null;
			
		existing.Name = model.Name;
		existing.Description = model.Description;
		
		await context.SaveChangesAsync();
		return existing;
	}
	
	public async Task<bool> DeleteAsync (Guid key, Guid userId)
	{
		var old = await context.StorageLocations.FirstOrDefaultAsync (s => s.Id == key && s.UserId == userId);
		if (old == null)
			return false;
			
		context.StorageLocations.Remove (old);
		await context.SaveChangesAsync();
		
		return true;
	}
}