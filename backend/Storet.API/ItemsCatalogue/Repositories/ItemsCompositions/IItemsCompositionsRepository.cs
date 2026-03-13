using Storet.API.ItemsCatalogue.Models;

namespace Storet.API.ItemsCatalogue.Repositories.ItemsCompositions;

public interface IItemsCompositionRepository
{
	public Task<IEnumerable<ItemComposition>> GetAllForItemAsync (Guid itemId);
	public Task<int> BulkInsertAsync (IEnumerable<ItemComposition> itemCompositions);
	public Task<bool> UpdateAsync (Guid parentItemId, Guid componentItemId, short? quantity = null, string? unit = null);
	public Task<int> BulkDeleteForItemAsync (Guid parentItemId, IEnumerable<Guid> itemIds);
	public Task<int> DeleteAllForItemAsync (Guid parentItemId);
}