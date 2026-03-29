using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Repositories.ItemsCategories;

public interface IItemsCategoriesRepository
{
	public Task<IEnumerable<ItemCategory>> GetAllForItemAsync (Guid itemId, Guid userId);
	public Task<int> BulkInsertAsync (IEnumerable<ItemCategory> itemCategories);
	public Task<int> BulkDeleteForItemAsync (Guid itemId, Guid userId, IEnumerable<int> categoryIds);
	public Task<int> DeleteAllForItemAsync (Guid itemId, Guid userId);
}