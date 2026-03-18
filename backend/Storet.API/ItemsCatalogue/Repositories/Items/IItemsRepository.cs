using Storet.API.ItemsCatalogue.Models;
using Storet.API.Core.Repository;

namespace Storet.API.ItemsCatalogue.Repositories.Items;

public interface IItemsRepository : IPaginatedCrudRepository<Item, Guid, string>, IExistenceCheckable<Guid>, IBulkExistenceCheckable<Guid>
{
	public Task<IEnumerable<Item>> BulkInsertAsync (IEnumerable<Item> items);
	public Task<IEnumerable<Item>> GetAllComponentsAsync ();
	public Task<bool> BulkDeleteAsync (IEnumerable<Guid> itemsIds);
}