using Microsoft.EntityFrameworkCore;
using Storet.API.Core.Repository;
using Storet.API.ItemsCatalogue.Data;
using Storet.API.ItemsCatalogue.Models;

namespace Storet.API.ItemsCatalogue.Repositories.ItemsCategories;

public class ItemsCategoriesRepository : BaseRepository<StoretItemsCatalogueDbContext>, IItemsCategoriesRepository
{
	public ItemsCategoriesRepository (StoretItemsCatalogueDbContext context) : base (context) {}

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
	
	public async Task<int> BulkDeleteForItemAsync (Guid itemId, IEnumerable<int> categoryIds)
	{
		return await context.ItemsCategories
							.Where (i => i.ItemId == itemId && categoryIds.Contains (i.CategoryId))
							.ExecuteDeleteAsync();
	}

	public async Task<int> DeleteAllForItemAsync (Guid itemId)
	{
		return await context.ItemsCategories
							.Where (i => i.ItemId == itemId)
							.ExecuteDeleteAsync();
	}
}