using MediatR;
using Storet.Modules.Inventory.Services.Inventory;
using Storet.Modules.ItemsCatalogue.Events;

namespace Storet.Modules.Inventory.EventsHandlers;

public class ItemsCreatedEventHandler : INotificationHandler<ItemsCreatedEvent>
{
	private readonly IInventoryService service;
	
	public ItemsCreatedEventHandler (IInventoryService service)
	{
		this.service = service;
	}
	
	public async Task Handle (ItemsCreatedEvent notification, CancellationToken cancellationToken)
	{
		await service.BulkInsertAsync (notification.ItemIds);
	}
}