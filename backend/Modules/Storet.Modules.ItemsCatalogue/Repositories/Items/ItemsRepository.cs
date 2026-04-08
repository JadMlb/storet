using Microsoft.EntityFrameworkCore;
using Storet.Core.Repository;
using Storet.Core.Utils;
using Storet.Modules.ItemsCatalogue.Data;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Repositories.Items;

public class ItemsRepository : BaseRepository<StoretItemsCatalogueDbContext>, IItemsRepository
{
	public ItemsRepository (StoretItemsCatalogueDbContext context) : base (context) {}

	public async Task<IEnumerable<Item>> GetAllAsync (Query<string> query, Guid userId)
	{
		return await context.Items
							.AsNoTracking()
							.Where (i => !i.IsComponent)
							.Where (i => i.UserId == userId)
							.PaginateQuery (query, "Name")
							.ToListAsync();
	}
	
	public async Task<IEnumerable<Item>> GetAllFromListAsync (Guid userId, IEnumerable<Guid> itemIds)
	{
		return await context.Items
							.AsNoTracking()
							.Where (i => !i.IsComponent)
							.Where (i => i.UserId == userId)
							.Where (i => itemIds.Contains (i.Id))
							.ToListAsync();
	}
	
	public async Task<IEnumerable<Item>> GetAllComponentsAsync (Guid userId)
	{
		return await context.Items
							.AsNoTracking()
							.Where (i => i.IsComponent)
							.Where (i => i.UserId == userId)
							.ToListAsync();
	}
	
	public async Task<string?> GetPreviousKeyAsync (Query<string> query, Guid userId)
	{
		if (query.Key == null)
			return null;
			
		var previousCursorItem = await context.Items
												.AsNoTracking()
												.Where (i => i.UserId == userId)
												.Where (i => string.Compare (i.Name, query.Key) < 0)
												.OrderByDescending (i => i.Name)
												.Take (query.PageSize + 1)
												.FirstOrDefaultAsync();
		return previousCursorItem?.Name;
	}
	
	public async Task<Item?> GetOneAsync (Guid key, Guid userId)
	{
		return await context.Items
							.AsNoTracking()
							.Include (i => i.ItemCategories)
							.ThenInclude (i => i.Category)
							.Include (i => i.Components)
							.ThenInclude (i => i.ComponentItem)
							.FirstOrDefaultAsync (i => i.Id == key && i.UserId == userId);
	}
	
	public async Task<bool> ExistsAsync (Guid key, Guid userId)
	{
		return await context.Items
							.AsNoTracking()
							.AnyAsync (i => i.Id == key && i.UserId == userId);
	}
	
	public async Task<bool> AllExistAsync (Guid userId, IEnumerable<Guid> keys)
	{
		var keysSet = keys.ToHashSet();
		var numberOfExistsingIdsInDb = await context.Items
													.AsNoTracking()
													.Where (i => i.IsComponent)
													.Where (i => i.UserId == userId)
													.CountAsync (c => keysSet.Contains (c.Id));
		return keysSet.Count == numberOfExistsingIdsInDb;
	}

	public async Task<Item?> InsertAsync (Item model)
	{
		await context.Items.AddAsync (model);
		try
		{
			await context.SaveChangesAsync();
			return model;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public async Task<IEnumerable<Item>> BulkInsertAsync (IEnumerable<Item> items)
	{
		var itemsList = items.ToList();

		await context.Items.AddRangeAsync (itemsList);
		await context.SaveChangesAsync();
		
		return itemsList;
	}

	public async Task<Item?> UpdateAsync (Guid key, Guid userId, Item model)
	{
		var existing = await context.Items
									.FirstOrDefaultAsync (i => i.Id == key && i.UserId == userId);
		if (existing == null)
			return null;

		existing.Name = model.Name;
		existing.Description = model.Description;
		existing.Quantity = model.Quantity;
		existing.Unit = model.Unit;

		try
		{
			await context.SaveChangesAsync();
			return existing;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public async Task<bool> DeleteAsync (Guid key, Guid userId)
	{
		var existing = await context.Items.FirstOrDefaultAsync (i => i.Id == key && i.UserId == userId);
		if (existing == null)
			return false;
		
		try
		{
			context.Items.Remove (existing);
			await context.SaveChangesAsync();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
	
	public async Task<bool> BulkDeleteAsync (IEnumerable<Guid> itemsIds, Guid userId)
	{
		var existing = await context.Items
									.Where (i => itemsIds.Contains (i.Id) && i.UserId == userId)
									.ToListAsync();
		if (existing.Count == 0)
			return false;
		
		try
		{
			context.Items.RemoveRange (existing);
			await context.SaveChangesAsync();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}