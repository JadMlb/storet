using Storet.Core.Service;
using Storet.Modules.Inventory.Contracts.Inventory;

namespace Storet.Modules.Inventory.Services.Inventory;

public interface IInventoryService : IInsertable<InventoryResponse, InventoryInsertRequest>, IUpdatable<InventoryResponse, Guid, InventoryUpdateRequest>, IDeletable<Guid>
{
	public Task<IEnumerable<InventoryResponse>> GetAllAsync ();
	public Task<bool> UpdateInventoryQuantitiesAsync (Dictionary<Guid, float> quantities);
}