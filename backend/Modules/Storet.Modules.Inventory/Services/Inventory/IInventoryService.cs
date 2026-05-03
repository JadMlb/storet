using Storet.Core.Service;
using Storet.Modules.Inventory.Contracts.Inventory;
using Storet.Modules.Inventory.Queries;

namespace Storet.Modules.Inventory.Services.Inventory;

public interface IInventoryService : IUpdatable<InventoryResponse, Guid, InventoryUpdateRequest>
{
	public Task BulkInsertAsync (IEnumerable<Guid> itemIds);
	///<summary>
	/// Fetches the inventories of provided items and their components and returns a dictionary mapping the parent item ids to the inventories of their components
	///</summary>
	public Task<Dictionary<Guid, IEnumerable<InventoryResponse>>> GetAllAsync (InventoryFilterQuery query);
	public Task<IEnumerable<InventoryResponse>> GetOneAsync (Guid itemId);
	public Task<bool> UpdateInventoryQuantitiesAsync (Dictionary<Guid, float> quantities);
	public Task<bool> BulkDeleteAsync (IEnumerable<Guid> itemIds);
}