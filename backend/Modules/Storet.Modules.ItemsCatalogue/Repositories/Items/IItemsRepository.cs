using Storet.Core.Repository;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Repositories.Items;

public interface IItemsRepository : IPaginatedCrudRepository<Item, Guid, string>, IExistenceCheckable<Guid>, IBulkExistenceCheckable<Guid>
{
	public Task<IEnumerable<Item>> BulkInsertAsync (IEnumerable<Item> items);
	public Task<IEnumerable<Item>> GetAllComponentsAsync ();
	public Task<bool> BulkDeleteAsync (IEnumerable<Guid> itemsIds);
}