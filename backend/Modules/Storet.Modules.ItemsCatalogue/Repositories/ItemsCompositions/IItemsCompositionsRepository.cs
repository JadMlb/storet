using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Repositories.ItemsCompositions;

public interface IItemsCompositionRepository
{
	public Task<Dictionary<Guid, List<Guid>>> GetComponentIdsForItemsAsync (IEnumerable<Guid> itemsIds, Guid userId);
	public Task<IEnumerable<Guid>> GetNotUsedByAnyAsync (IEnumerable<Guid> items, Guid userId);
	public Task<int> BulkInsertAsync (IEnumerable<ItemComposition> itemCompositions);
	public Task<IEnumerable<Guid>> DeleteAllForItemAsync (Guid parentItemId, Guid userId);
}