using Storet.Core.Repository;

namespace Storet.Modules.Inventory.Repositories.Inventory;

public interface IInventoryRepository : IExistenceCheckable<Guid>, IInsertable<Models.Inventory>, IUpdatable<Models.Inventory, Guid>, IDeletable<Guid>
{
	public Task<Models.Inventory?> GetOneAsync (Guid itemId, Guid userId);
	public Task<Dictionary<Guid, Models.Inventory>> GetAllFromListAsync (Guid userId, IEnumerable<Guid> ids);
	public Task<IEnumerable<Models.Inventory>> GetAllAsync (Guid userId);
	public Task<int> BulkUpdateAsync (Guid userId, IEnumerable<Models.Inventory> values);
}