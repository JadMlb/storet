using Storet.Core.Service;
using Storet.Modules.Inventory.Contracts.InventoryMovement;

namespace Storet.Modules.Inventory.Services.InventoryMovements;

public interface IInventoryMovementsService : IPaginatedRetrievable<InventoryMovementResponse, InventoryMovementDetailsResponse, Guid, DateTimeOffset?>, IInsertable<IEnumerable<InventoryMovementDetailsResponse>, InventoryModificationRequest>
{}