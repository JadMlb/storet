using Storet.API.Models;
using Storet.API.Repositories.Base;

namespace Storet.API.Repositories.Items;

public interface IItemsRepository : IPaginatedCrudRepository<Item, Guid, string>, IExistenceCheckable<Guid>
{
}