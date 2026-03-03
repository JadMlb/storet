using Microsoft.EntityFrameworkCore;
using Storet.API.Data;
using Storet.API.Models;
using Storet.API.Repositories.Base;
using Storet.API.Utils;

namespace Storet.API.Repositories.Items;

public class ItemsRepository : BaseRepository, IItemsRepository
{
	public ItemsRepository (StoretDbContext context) : base (context) {}

	public async Task<IEnumerable<Item>> GetAllAsync (Query<string> query)
	{
		return await context.Items
							.AsNoTracking()
							.PaginateQuery (query, "Name")
							.ToListAsync();
	}
	
	public async Task<string?> GetPreviousKeyAsync (Query<string> query)
	{
		if (query.Key == null)
			return null;
			
		var previousCursorItem = await context.Items
												.AsNoTracking()
												.Where (i => String.Compare (i.Name, query.Key) < 0)
												.OrderByDescending (i => i.Name)
												.Take (query.PageSize + 1)
												.FirstOrDefaultAsync();
		return previousCursorItem?.Name;
	}
	
	public async Task<Item?> GetOneAsync (Guid key)
	{
		return await context.Items
							.AsNoTracking()
							.Include (i => i.ItemCategories)
							.ThenInclude (i => i.Category)
							.FirstOrDefaultAsync (i => i.Id == key);
	}
	
	public async Task<bool> ExistsAsync (Guid key)
	{
		return await context.Items
							.AsNoTracking()
							.AnyAsync (i => i.Id == key);
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

	public async Task<Item?> UpdateAsync (Guid key, Item model)
	{
		var existing = await context.Items
									.FirstOrDefaultAsync (i => i.Id == key);
		if (existing == null)
			return null;

		existing.Name = model.Name;
		existing.Description = model.Description;

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

	public async Task<bool> DeleteAsync (Guid key)
	{
		var existing = await context.Items.FirstOrDefaultAsync (i => i.Id == key);
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
}