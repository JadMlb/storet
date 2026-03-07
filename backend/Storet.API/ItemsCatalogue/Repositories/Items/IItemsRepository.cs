using Storet.API.ItemsCatalogue.Models;
using Storet.API.Core.Repository;

namespace Storet.API.ItemsCatalogue.Repositories.Items;

public interface IItemsRepository : IPaginatedCrudRepository<Item, Guid, string>, IExistenceCheckable<Guid>
{
}