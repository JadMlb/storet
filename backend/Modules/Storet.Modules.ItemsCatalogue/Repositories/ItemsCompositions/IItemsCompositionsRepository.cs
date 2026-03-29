using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Repositories.ItemsCompositions;

public interface IItemsCompositionRepository
{
	public Task<IEnumerable<ItemComposition>> GetAllForItemAsync (Guid itemId, Guid userId);
	public Task<IEnumerable<Guid>> GetNotUsedByAnyAsync (IEnumerable<Guid> items, Guid userId);
	public Task<int> BulkInsertAsync (IEnumerable<ItemComposition> itemCompositions);
	public Task<bool> UpdateAsync (Guid parentItemId, Guid componentItemId, Guid userId, short quantity);
	public Task<int> BulkDeleteForItemAsync (Guid parentItemId, Guid userId, IEnumerable<Guid> itemIds);
	public Task<int> DeleteAllForItemAsync (Guid parentItemId, Guid userId);
}