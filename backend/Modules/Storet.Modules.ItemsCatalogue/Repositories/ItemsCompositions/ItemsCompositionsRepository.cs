using Microsoft.EntityFrameworkCore;
using Storet.Core.Repository;
using Storet.Modules.ItemsCatalogue.Data;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Repositories.ItemsCompositions;

public class ItemsCompositionsRepository : BaseRepository<StoretItemsCatalogueDbContext>, IItemsCompositionRepository
{
	public ItemsCompositionsRepository (StoretItemsCatalogueDbContext context) : base (context) {}

	public async Task<IEnumerable<ItemComposition>> GetAllForItemAsync (Guid itemId, Guid userId)
	{
		return await context.ItemsCompositions
							.AsNoTracking()
							.Where (c => c.ParentItemId == itemId && c.UserId == userId)
							.Include (c => c.ComponentItem)
							.ToListAsync();
	}
	
	public async Task<IEnumerable<Guid>> GetNotUsedByAnyAsync (IEnumerable<Guid> items, Guid userId)
	{
		var usedIds = await context.ItemsCompositions
									.AsNoTracking()
									.Where (i => i.UserId == userId)
									.Where (i => items.Contains (i.ComponentItemId))
									.Select (i => i.ComponentItemId)
									.Distinct()
									.ToListAsync();
		return items.Except (usedIds)
					.ToList();
	}
	
	public async Task<int> BulkInsertAsync (IEnumerable<ItemComposition> itemCompositions)
	{
		await context.ItemsCompositions.AddRangeAsync (itemCompositions);
		return await context.SaveChangesAsync();
	}
	
	public async Task<bool> UpdateAsync (Guid parentItemId, Guid componentItemId, Guid userId, short quantity)
	{
		var relationship = await context.ItemsCompositions
										.FirstOrDefaultAsync (
											c => c.ParentItemId == parentItemId
												&& c.ComponentItemId == componentItemId
												&& c.UserId == userId
										);
		if (relationship == null)
			return false;
			
		relationship.Quantity = quantity;
		
		await context.SaveChangesAsync();
		
		return true;
	}
	
	public async Task<int> BulkDeleteForItemAsync (Guid parentItemId, Guid userId, IEnumerable<Guid> componentIds)
	{
		return await context.ItemsCompositions
							.Where (i => i.UserId == userId && i.ParentItemId == parentItemId && componentIds.Contains (i.ComponentItemId))
							.ExecuteDeleteAsync();
	}
	
	public async Task<IEnumerable<Guid>> DeleteAllForItemAsync (Guid itemId, Guid userId)
	{
		var toBeDeleted = await context.ItemsCompositions
										.Where (i => i.ParentItemId == itemId && i.UserId == userId)
										.ToListAsync();
		context.ItemsCompositions.RemoveRange (toBeDeleted);
		await context.SaveChangesAsync();
		
		return toBeDeleted.Select(c => c.ComponentItemId).ToList();
	}
}