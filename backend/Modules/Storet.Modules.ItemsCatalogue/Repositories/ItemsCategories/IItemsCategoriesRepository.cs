using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Repositories.ItemsCategories;

public interface IItemsCategoriesRepository
{
	public Task<IEnumerable<ItemCategory>> GetAllForItemAsync (Guid itemId);
	public Task<int> BulkInsertAsync (IEnumerable<ItemCategory> itemCategories);
	public Task<int> BulkDeleteForItemAsync (Guid itemId, IEnumerable<int> categoryIds);
	public Task<int> DeleteAllForItemAsync (Guid itemId);
}