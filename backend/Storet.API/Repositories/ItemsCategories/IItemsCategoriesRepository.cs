using Storet.API.Models;

namespace Storet.API.Repositories.ItemsCategories;

public interface IItemsCategoriesRepository
{
	public Task<IEnumerable<ItemCategory>> GetAllForItemAsync (Guid itemId);
	public Task<int> BulkInsertAsync (IEnumerable<ItemCategory> itemCategories);
	public Task<int> BulkDeleteAsync (IEnumerable<(Guid itemId, int categoryId)> itemCategories);
	public Task<int> DeleteAllForItemAsync (Guid itemId);
}