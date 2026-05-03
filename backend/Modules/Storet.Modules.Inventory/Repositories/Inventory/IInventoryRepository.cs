using Storet.Core.Repository;

namespace Storet.Modules.Inventory.Repositories.Inventory;

public interface IInventoryRepository : IExistenceCheckable<Guid>, IUpdatable<Models.Inventory, Guid>
{
	public Task<Models.Inventory?> GetOneAsync (Guid itemId, Guid userId);
	public Task<Dictionary<Guid, Models.Inventory>> GetAllFromListAsync (Guid userId, IEnumerable<Guid> ids);
	public Task<int> BulkInsertAsync (IEnumerable<Models.Inventory> models);
	public Task<int> BulkUpdateAsync (Guid userId, IEnumerable<Models.Inventory> values);
	public Task<bool> BulkDeleteAsync (Guid userId, IEnumerable<Guid> itemIds);
}