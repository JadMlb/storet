using Storet.Core.Service;
using Storet.Modules.Inventory.Contracts.Inventory;

namespace Storet.Modules.Inventory.Services.Inventory;

public interface IInventoryService : IUpdatable<InventoryResponse, Guid, InventoryUpdateRequest>
{
	public Task<IEnumerable<InventoryResponse>> BulkInsertAsync (IEnumerable<Guid> itemIds);
	public Task<IEnumerable<InventoryResponse>> GetAllAsync ();
	public Task<bool> UpdateInventoryQuantitiesAsync (Dictionary<Guid, float> quantities);
	public Task<bool> BulkDeleteAsync (IEnumerable<Guid> itemIds);
}