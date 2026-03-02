using Microsoft.EntityFrameworkCore;
using Storet.API.Data;
using Storet.API.Models;
using Storet.API.Repositories.Base;
using Storet.API.Utils;

namespace Storet.API.Repositories.ItemsCategories;

public class ItemsCategoriesRepository : BaseRepository, IItemsCategoriesRepository
{
	public ItemsCategoriesRepository (StoretDbContext context) : base (context) {}

	public async Task<IEnumerable<ItemCategory>> GetAllForItemAsync (Guid itemId)
	{
		return await context.ItemsCategories
							.AsNoTracking()
							.Where (i => i.ItemId == itemId)
							.Include (i => i.Category)
							.ToListAsync();
	}

	public async Task<int> BulkInsertAsync (IEnumerable<ItemCategory> itemCategories)
	{
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		return await context.SaveChangesAsync();
	}
	
	public async Task<int> BulkDeleteAsync (IEnumerable<(Guid itemId, int categoryId)> itemCategories)
	{
		return await context.ItemsCategories
							.WhereComposite (itemCategories, ["ItemId", "CategoryId"])
							.ExecuteDeleteAsync();
	}

	public async Task<int> DeleteAllForItemAsync (Guid itemId)
	{
		return await context.ItemsCategories
							.Where (i => i.ItemId == itemId)
							.ExecuteDeleteAsync();
	}
}