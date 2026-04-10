using Storet.Core.Service;
using Storet.Modules.Inventory.Contracts.StorageLocation;

namespace Storet.Modules.Inventory.Services.StorageLocations;

public interface IStorageLocationsService : ICrudService<StorageLocationResponse, StorageLocationDetailsResponse, StorageLocationInsertRequest, StorageLocationUpdateRequest, Guid>
{}