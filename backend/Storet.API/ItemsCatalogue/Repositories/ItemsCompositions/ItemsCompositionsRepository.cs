using Microsoft.EntityFrameworkCore;
using Storet.API.Core.Repository;
using Storet.API.ItemsCatalogue.Data;
using Storet.API.ItemsCatalogue.Models;

namespace Storet.API.ItemsCatalogue.Repositories.ItemsCompositions;

public class ItemsCompositionsRepository : BaseRepository<StoretItemsCatalogueDbContext>, IItemsCompositionRepository
{
	public ItemsCompositionsRepository (StoretItemsCatalogueDbContext context) : base (context) {}

	public async Task<IEnumerable<ItemComposition>> GetAllForItemAsync (Guid itemId)
	{
		return await context.ItemsCompositions
							.AsNoTracking()
							.Where (c => c.ParentItemId == itemId)
							.Include (c => c.ComponentItem)
							.ToListAsync();
	}
	
	public async Task<IEnumerable<Guid>> GetNotUsedByAnyAsync (IEnumerable<Guid> items)
	{
		var usedIds = await context.ItemsCompositions
									.AsNoTracking()
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
	
	public async Task<bool> UpdateAsync (Guid parentItemId, Guid componentItemId, short quantity)
	{
		var relationship = await context.ItemsCompositions
										.FirstOrDefaultAsync (
											c => c.ParentItemId == parentItemId
												&& c.ComponentItemId == componentItemId
										);
		if (relationship == null)
			return false;
			
		relationship.Quantity = quantity;
		
		await context.SaveChangesAsync();
		
		return true;
	}
	
	public async Task<int> BulkDeleteForItemAsync (Guid parentItemId, IEnumerable<Guid> componentIds)
	{
		return await context.ItemsCompositions
							.Where (i => i.ParentItemId == parentItemId && componentIds.Contains (i.ComponentItemId))
							.ExecuteDeleteAsync();
	}
	
	public async Task<int> DeleteAllForItemAsync (Guid itemId)
	{
		return await context.ItemsCompositions
							.Where (i => i.ParentItemId == itemId)
							.ExecuteDeleteAsync();
	}
}