using Microsoft.EntityFrameworkCore;
using Storet.Core.Repository;
using Storet.Modules.ItemsCatalogue.Data;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Repositories.ItemsCompositions;

public class ItemsCompositionsRepository : BaseRepository<StoretItemsCatalogueDbContext>, IItemsCompositionRepository
{
	public ItemsCompositionsRepository (StoretItemsCatalogueDbContext context) : base (context) {}

	public async Task<Dictionary<Guid, List<Guid>>> GetComponentIdsForItemsAsync (IEnumerable<Guid> itemsIds, Guid userId)
	{
		var components = await context.ItemsCompositions
										.AsNoTracking()
										.Where (c => c.UserId == userId)
										.Where (c => itemsIds.Contains (c.ParentItemId))
										.Select (c => new {c.ParentItemId, c.ComponentItemId})
										.GroupBy (c => c.ParentItemId)
										.ToDictionaryAsync (g => g.Key, g => g.Select(c => c.ComponentItemId).ToList());
		return itemsIds.ToDictionary (
			parentId => parentId,
			parentId => components.GetValueOrDefault (parentId, [])
		);
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