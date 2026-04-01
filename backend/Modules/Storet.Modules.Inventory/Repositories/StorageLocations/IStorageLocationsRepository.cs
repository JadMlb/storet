using Storet.Core.Repository;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Repositories.StorageLocations;

public interface IStorageLocationsRepository : ICrudRepository<StorageLocation, Guid>, IExistenceCheckable<Guid>
{}