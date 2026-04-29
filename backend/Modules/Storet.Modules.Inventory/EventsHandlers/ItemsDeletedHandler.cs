using MediatR;
using Storet.Modules.Inventory.Services.Inventory;
using Storet.Modules.ItemsCatalogue.Events;

namespace Storet.Modules.Inventory.EventsHandlers;

public class ItemsDeletedEventHandler : INotificationHandler<ItemsDeletedEvent>
{
	private readonly IInventoryService service;
	
	public ItemsDeletedEventHandler (IInventoryService service)
	{
		this.service = service;
	}
	
	public async Task Handle (ItemsDeletedEvent notification, CancellationToken cancellationToken)
	{
		await service.BulkDeleteAsync (notification.ItemIds);
	}
}