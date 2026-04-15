using Storet.Core.Repository;
using Storet.Modules.Inventory.Models;

namespace Storet.Modules.Inventory.Repositories.InventoryMovements;

public interface IInventoryMovementsRepository : IPaginatedRetrievable<InventoryMovement, Guid, DateTimeOffset?>, IInsertable<InventoryMovement>
{}