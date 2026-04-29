using FluentAssertions;
using Moq;
using Storet.Core.Exceptions;
using Storet.Modules.Inventory.Contracts.Inventory;
using Storet.Modules.Inventory.EventsHandlers;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Services.Inventory;
using Storet.Modules.ItemsCatalogue.Events;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Inventory.Tests.EventHandlers;

public class ItemsCreatedEventHandlerTests
{
	private readonly Mock<IInventoryService> mockService;
	private readonly ItemsCreatedEventHandler handler;
	private readonly List<Guid> itemIds;
	private readonly ItemsCreatedEvent notification;
	
	public ItemsCreatedEventHandlerTests ()
	{
		mockService = new Mock<IInventoryService>();
		handler = new ItemsCreatedEventHandler (mockService.Object);
		itemIds = [Guid.NewGuid(), Guid.NewGuid()];
		notification = new ItemsCreatedEvent (itemIds, Guid.NewGuid());
	}
	
	[Fact]
	public async Task HandleWhenNotificationContainsNonExistingItemIdShouldThrowNotFoundException ()
	{
		mockService.Setup (s => s.BulkInsertAsync (It.IsAny<IEnumerable<Guid>>()))
					.ThrowsAsync (
						new EntityNotFoundException (nameof (Item), notification.ItemIds)
					);
		
		Func<Task> act = async () => await handler.Handle (notification, default);
		
		await act.Should()
					.ThrowAsync<EntityNotFoundException>()
					.WithMessage ($"Item with ID {itemIds[0]},{itemIds[1]} was not found");
		
		mockService.Verify (s => s.BulkInsertAsync (It.Is<IEnumerable<Guid>> (ids => ids.ToHashSet().SetEquals (notification.ItemIds))), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task HandleWhenNotificationContainsExistingItemIdsShouldReturnVoid ()
	{
		mockService.Setup (s => s.BulkInsertAsync (It.IsAny<IEnumerable<Guid>>()))
					.ReturnsAsync (
						itemIds.Select (
							id => new InventoryResponse ()
							{
								Item = new ()
								{
									Id = id,
									Name = $"{id}"
								},
								Status = Status.EmptyAccepted
							}
						)
					);
		
		await handler.Handle (notification, default);
		
		mockService.Verify (s => s.BulkInsertAsync (It.Is<IEnumerable<Guid>> (ids => ids.ToHashSet().SetEquals (notification.ItemIds))), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
}