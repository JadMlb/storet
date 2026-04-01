using Storet.Core.Repository;

namespace Storet.Modules.Inventory.Repositories.Inventory;

public interface IInventoryRepository : IExistenceCheckable<Guid>, IInsertable<Models.Inventory>, IUpdatable<Models.Inventory, Guid>, IDeletable<Guid>
{
	public Task<IEnumerable<Models.Inventory>> GetAllAsync (Guid userId);
}