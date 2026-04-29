using Moq;
using Storet.Modules.Inventory.EventsHandlers;
using Storet.Modules.Inventory.Services.Inventory;
using Storet.Modules.ItemsCatalogue.Events;

namespace Storet.Inventory.Tests.EventHandlers;

public class ItemsDeletedEventHandlerTests
{
	private readonly Mock<IInventoryService> mockService;
	private readonly ItemsDeletedEventHandler handler;
	private readonly List<Guid> itemIds;
	private readonly ItemsDeletedEvent notification;
	
	public ItemsDeletedEventHandlerTests ()
	{
		mockService = new Mock<IInventoryService>();
		handler = new ItemsDeletedEventHandler (mockService.Object);
		itemIds = [Guid.NewGuid(), Guid.NewGuid()];
		notification = new ItemsDeletedEvent (itemIds, Guid.NewGuid());
	}
	
	[Fact]
	public async Task HandleWhenNotificationContainsNonExistingItemIdShouldReturnVoid ()
	{
		mockService.Setup (s => s.BulkDeleteAsync (It.IsAny<IEnumerable<Guid>>()))
					.ReturnsAsync (false);
		
		await handler.Handle (notification, default);
		
		mockService.Verify (s => s.BulkDeleteAsync (It.Is<IEnumerable<Guid>> (ids => ids.ToHashSet().SetEquals (notification.ItemIds))), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task HandleWhenNotificationContainsExistingItemIdsShouldReturnVoid ()
	{
		mockService.Setup (s => s.BulkDeleteAsync (It.IsAny<IEnumerable<Guid>>()))
					.ReturnsAsync (true);
		
		await handler.Handle (notification, default);
		
		mockService.Verify (s => s.BulkDeleteAsync (It.Is<IEnumerable<Guid>> (ids => ids.ToHashSet().SetEquals (notification.ItemIds))), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
}