using Microsoft.EntityFrameworkCore;
using Storet.Core.Repository;
using Storet.Modules.ItemsCatalogue.Data;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Repositories.ItemsCategories;

public class ItemsCategoriesRepository : BaseRepository<StoretItemsCatalogueDbContext>, IItemsCategoriesRepository
{
	public ItemsCategoriesRepository (StoretItemsCatalogueDbContext context) : base (context) {}

	public async Task<IEnumerable<ItemCategory>> GetAllForItemAsync (Guid itemId, Guid userId)
	{
		return await context.ItemsCategories
							.AsNoTracking()
							.Where (i => i.ItemId == itemId && i.UserId == userId)
							.Include (i => i.Category)
							.ToListAsync();
	}

	public async Task<int> BulkInsertAsync (IEnumerable<ItemCategory> itemCategories)
	{
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		return await context.SaveChangesAsync();
	}
	
	public async Task<int> BulkDeleteForItemAsync (Guid itemId, Guid userId, IEnumerable<int> categoryIds)
	{
		return await context.ItemsCategories
							.Where (i => i.ItemId == itemId && categoryIds.Contains (i.CategoryId) && i.UserId == userId)
							.ExecuteDeleteAsync();
	}

	public async Task<int> DeleteAllForItemAsync (Guid itemId, Guid userId)
	{
		return await context.ItemsCategories
							.Where (i => i.ItemId == itemId && i.UserId == userId)
							.ExecuteDeleteAsync();
	}
}