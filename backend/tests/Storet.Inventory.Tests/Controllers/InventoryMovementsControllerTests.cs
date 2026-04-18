using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Storet.Core.Exceptions;
using Storet.Core.Utils;
using Storet.Modules.Inventory.Contracts.InventoryMovement;
using Storet.Modules.Inventory.Contracts.StorageLocation;
using Storet.Modules.Inventory.Controllers;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Services.InventoryMovements;
using Storet.Modules.ItemsCatalogue.Contracts.Items;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Tests.Common.Controller;

namespace Storet.Inventory.Tests.Controllers;

public class InventoryMovementsControllerTests : ControllerTestsBase<InventoryMovementsController, IInventoryMovementsService>
{
	protected override InventoryMovementsController InitControllerInstance ()
	{
		return new InventoryMovementsController (mockService.Object);
	}
	
	[Fact]
	public async Task GetAllWithInvalidQueryShouldReturnBadRequest ()
	{
		var query = new Query<DateTimeOffset?>
		{
			PageSize = 0
		};
		
		mockService.Setup (m => m.GetAllAsync (query))
					.ThrowsAsync (new ArgumentException ("Invalid page size for query"));
		
		var result = await controller.GetAll (query);
		result.Result.Should().BeOfType<BadRequestObjectResult>();
		
		mockService.Verify (m => m.GetAllAsync (query), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllWithNoMatchShouldReturnOkWithEmptyList ()
	{
		var query = new Query<DateTimeOffset?>();
		mockService.Setup (m => m.GetAllAsync (query))
					.ReturnsAsync (new PaginatedResponse<InventoryMovementResponse, DateTimeOffset?>());
		
		var result = await controller.GetAll (query);
		var okResult = result.Result as OkObjectResult;
		
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);
		
		var returned = okResult.Value as PaginatedResponse<InventoryMovementResponse, DateTimeOffset?>;
		
		returned.Should().NotBeNull();
		returned.Data.Should().BeEmpty();
		returned.HasNext.Should().BeFalse();
		returned.HasPrevious.Should().BeFalse();
		returned.Next.Should().BeNull();
		returned.Previous.Should().BeNull();
		
		mockService.Verify (m => m.GetAllAsync (query), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	private void SeedService (DateTimeOffset referenceTimestamp)
	{
		var location = new StorageLocationResponse
		{
			Id = Guid.NewGuid(),
			Name = "Fridge"
		};

		List<InventoryMovementResponse> logs = [
			new ()
			{
				Id = Guid.NewGuid(),
				ExecutedAt = referenceTimestamp,
				Direction = MovementDirection.Out,
				NumberOfItems = 2,
				Source = MovementSource.Usage,
				Location = location
			},
			new ()
			{
				Id = Guid.NewGuid(),
				ExecutedAt = referenceTimestamp.AddHours (-1),
				Direction = MovementDirection.In,
				NumberOfItems = 2,
				Source = MovementSource.Purchase,
				Location = location
			},
			new ()
			{
				Id = Guid.NewGuid(),
				ExecutedAt = referenceTimestamp.AddHours(-1).AddSeconds (-1),
				Direction = MovementDirection.In,
				NumberOfItems = 1,
				Source = MovementSource.Purchase,
				Location = location
			}
		];
		
		mockService.Setup (m => m.GetAllAsync (It.IsAny<Query<DateTimeOffset?>>()))
					.ReturnsAsync (
						(Query<DateTimeOffset?> query) =>
						{
							var previousKey = logs.Where (m => m.ExecutedAt > query.Key)
								.OrderBy (m => m.ExecutedAt)
								.Take (query.PageSize + 1)
								.FirstOrDefault()?.ExecutedAt;
								
							var filtered = logs.AsEnumerable();
							if (query.Key != null)
								filtered = filtered.Where (l => l.ExecutedAt <= query.Key);
							filtered = filtered.Take (query.PageSize + 1);
								
							return new PaginatedResponse<InventoryMovementResponse, DateTimeOffset?>
							{
								Data = filtered.Take(query.PageSize).ToList(),
								Next = filtered.Last().ExecutedAt,
								Previous = previousKey
							};
						}
					);
	}
	
	[Fact]
	public async Task GetAllWithDataShouldReturnOkWithFilledList ()
	{
		var timestamp = DateTimeOffset.Now;
		SeedService (timestamp);
		
		var query = new Query<DateTimeOffset?>
		{
			PageSize = 1
		};
		
		var result = await controller.GetAll (query);
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);
		
		var returned = okResult.Value as PaginatedResponse<InventoryMovementResponse, DateTimeOffset?>;
		
		returned.Should().NotBeNull();
		returned.Data.Should().HaveCount (1);
		returned.HasNext.Should().BeTrue();
		returned.HasPrevious.Should().BeFalse();
		returned.Next.Should().NotBeNull();
		returned.Next.Should().Be (timestamp.AddHours (-1));
		returned.Previous.Should().BeNull();
		
		mockService.Verify (m => m.GetAllAsync (query), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllWithDataOnSecondPageShouldReturnFilledListWithPreviousAndNext ()
	{
		var timestamp = DateTimeOffset.Now;
		SeedService (timestamp);
		
		var query = new Query<DateTimeOffset?>
		{
			PageSize = 1,
			Key = timestamp.AddHours (-1)
		};
		
		var result = await controller.GetAll (query);
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);
		
		var returned = okResult.Value as PaginatedResponse<InventoryMovementResponse, DateTimeOffset?>;
		
		returned.Should().NotBeNull();
		returned.Data.Should().HaveCount (1);
		returned.HasNext.Should().BeTrue();
		returned.HasPrevious.Should().BeTrue();
		returned.Next.Should().NotBeNull();
		returned.Next.Should().Be (timestamp.AddHours(-1).AddSeconds (-1));
		returned.Previous.Should().NotBeNull();
		returned.Previous.Should().Be (timestamp);
		
		mockService.Verify (m => m.GetAllAsync (query), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetOneWithNonExistingIdShouldReturnNotFound ()
	{
		var id = Guid.NewGuid();
		
		mockService.Setup (m => m.GetOneAsync (id))
					.ReturnsAsync ((InventoryMovementDetailsResponse?) null);
		
		var result = await controller.GetOne (id);
		result.Result.Should().BeOfType<NotFoundResult>();
		
		mockService.Verify (m => m.GetOneAsync (id), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetOneWithExistingIdShouldReturnOkWithObjectAndItems ()
	{
		var id = Guid.NewGuid();
		var location = new StorageLocationResponse
		{
			Id = Guid.NewGuid(),
			Name = "Fridge",
		};
		Guid beefId = Guid.NewGuid(), spicesId = Guid.NewGuid();
		
		mockService.Setup (m => m.GetOneAsync (id))
					.ReturnsAsync (
						new InventoryMovementDetailsResponse ()
						{
							Id = id,
							ExecutedAt = DateTimeOffset.Now,
							Direction = MovementDirection.Out,
							NumberOfItems = 2,
							Source = MovementSource.Usage,
							Location = location,
							Items = [
								new ()
								{
									Item = new () {Id = beefId, Name = "Beef", Quantity = 0.5f, Unit = Unit.Kilogramme},
									Quantity = 0.5f
								},
								new ()
								{
									Item = new () {Id = spicesId, Name = "Spices", Quantity = 0.2f, Unit = Unit.Kilogramme},
									Quantity = 0.01f
								}
							]
						}
					);
		
		var result = await controller.GetOne (id);
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);
		
		var returned = okResult.Value as InventoryMovementDetailsResponse;
		returned.Should().NotBeNull();
		returned.Location.Should().NotBeNull();
		returned.Location.Id.Should().Be (location.Id);
		returned.Direction.Should().Be (MovementDirection.Out);
		returned.ExecutedAt.Should().BeCloseTo (DateTimeOffset.Now, TimeSpan.FromSeconds (1));
		returned.NumberOfItems.Should().Be (2);
		returned.Items.Should().HaveCount (2);
		returned.Items.Should().Contain (i => i.Item.Id == beefId);
		returned.Items.Should().Contain (i => i.Item.Id == spicesId);
		
		mockService.Verify (m => m.GetOneAsync (id), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithFutureTimestampShouldReturnBadRequest ()
	{
		var dto = new InventoryModificationRequest
		{
			DestinationLocationId = Guid.NewGuid(),
			ExecutedAt = DateTimeOffset.Now.AddHours (1),
			Source = MovementSource.Purchase,
			Items = {
				{Guid.NewGuid(), 1}
			}
		};
		
		ValidateModel (dto);
		
		var result = await controller.Log (dto);
		result.Result.Should().BeOfType<BadRequestObjectResult>();
		
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithMissingSourceAndDestinationShouldReturnBadRequest ()
	{
		var dto = new InventoryModificationRequest
		{
			Source = MovementSource.Purchase,
			Items = {
				{Guid.NewGuid(), 1}
			}
		};
		
		mockService.Setup (m => m.InsertAsync (dto))
					.ThrowsAsync (new ArgumentException ("Neither source nor destination were specified"));
		
		ValidateModel (dto);
		var result = await controller.Log (dto);
		result.Result.Should().BeOfType<BadRequestObjectResult>();
		
		mockService.Verify (m => m.InsertAsync (dto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithNonExistingStorageLocationIdShouldReturnNotFound ()
	{
		var locationId = Guid.NewGuid();
		var dto = new InventoryModificationRequest
		{
			DestinationLocationId = locationId,
			Source = MovementSource.Purchase,
			Items = {
				{Guid.NewGuid(), 1}
			}
		};
		
		mockService.Setup (m => m.InsertAsync (dto))
					.ThrowsAsync (new EntityNotFoundException (nameof (StorageLocation), locationId));
		
		ValidateModel (dto);
		var res = await controller.Log (dto);
		res.Result.Should().BeOfType<NotFoundObjectResult>();
		
		mockService.Verify (m => m.InsertAsync (dto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithNonExistingItemIdShouldReturnNotFound ()
	{
		var locationId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		var dto = new InventoryModificationRequest
		{
			DestinationLocationId = locationId,
			Source = MovementSource.Purchase,
			Items = {
				{itemId, 1}
			}
		};
		mockService.Setup (i => i.InsertAsync (dto))
					.ThrowsAsync (new EntityNotFoundException (nameof (Item), itemId));
		
		ValidateModel (dto);
		var result = await controller.Log (dto);
		result.Result.Should().BeOfType<NotFoundObjectResult>();
		
		mockService.Verify (m => m.InsertAsync (dto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithExistingItemIdShouldReturnOk ()
	{
		var movementId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		var dto = new InventoryModificationRequest
		{
			DestinationLocationId = locationId,
			Items = {
				{itemId, 0.5f}
			}
		};
		
		mockService.Setup (m => m.InsertAsync (dto))
					.ReturnsAsync ([
						new InventoryMovementDetailsResponse
						{
							Id = movementId,
							Location = new StorageLocationResponse
							{
								Id = locationId,
								Name = "Fridge",
							},
							Direction = MovementDirection.In,
							Source = MovementSource.Purchase,
							NumberOfItems = 1,
							Items = [
								new ()
								{
									Item = new ItemResponseWithUnit
									{
										Id = itemId,
										Name = "Chicken",
										Quantity = 0.5f,
										Unit = Unit.Kilogramme
									},
									Quantity = 0.5f
								}
							]
						}
					]);
					
		ValidateModel (dto);
		var res = await controller.Log (dto);
		var okResult = res.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);
		
		var returned = okResult.Value as IEnumerable<InventoryMovementDetailsResponse>;
		
		returned.Should().NotBeNull();
		returned.Should().HaveCount (1);
		returned.First().Direction.Should().Be (MovementDirection.In);
		returned.First().Location.Should().NotBeNull();
		returned.First().NumberOfItems.Should().Be (1);
		returned.First().Items.Should().HaveCount (1);
		
		mockService.Verify (m => m.InsertAsync (dto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithBothSourceAndDestinationShouldReturnInserted ()
	{
		Guid outMovementId = Guid.NewGuid(), inMovementId = Guid.NewGuid();
		var sourceLocation = new StorageLocationResponse
		{
			Id = Guid.NewGuid(),
			Name = "Cupboard"
		};
		var destinationLocation = new StorageLocationResponse
		{
			Id = Guid.NewGuid(),
			Name = "Drawer"
		};
		var item = new ItemResponseWithUnit ()
		{
			Id = Guid.NewGuid(),
			Name = "Beef",
			Quantity = 0.5f,
			Unit = Unit.Kilogramme
		};
		
		var itemsInTransaction = new List<InventoryMovementItemResponse>
		{
			new ()
			{
				Item = item,
				Quantity = 0.5f
			}
		};
		
		var dto = new InventoryModificationRequest
		{
			SourceLocationId = sourceLocation.Id,
			DestinationLocationId = destinationLocation.Id,
			Source = MovementSource.Transfer,
			Items = {
				{item.Id, 0.5f}
			}
		};
		
		mockService.Setup (i => i.InsertAsync (dto))
					.ReturnsAsync ([
						new InventoryMovementDetailsResponse
						{
							Id = Guid.NewGuid(),
							Direction = MovementDirection.Out,
							Items = itemsInTransaction,
							Location = sourceLocation,
							NumberOfItems = 1,
							Source = MovementSource.Transfer
						},
						new InventoryMovementDetailsResponse
						{
							Id = Guid.NewGuid(),
							Direction = MovementDirection.In,
							Items = itemsInTransaction,
							Location = destinationLocation,
							NumberOfItems = 1,
							Source = MovementSource.Transfer
						}
					]);
		
		ValidateModel (dto);
		var res = await controller.Log (dto);
		var okResult = res.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);
		
		var returned = okResult.Value as IEnumerable<InventoryMovementDetailsResponse>;
		returned.Should().NotBeNull();
		returned.Should().HaveCount (2);
		returned.First().Direction.Should().Be (MovementDirection.Out);
		returned.First().Location.Should().NotBeNull();
		returned.First().Location!.Id.Should().Be (sourceLocation.Id);
		returned.First().NumberOfItems.Should().Be (1);
		returned.First().Items.Should().HaveCount (1);
		returned.First().Items.Should().AllSatisfy (i => i.Quantity.Should().BePositive());
		returned.Last().Direction.Should().Be (MovementDirection.In);
		returned.Last().Location.Should().NotBeNull();
		returned.Last().Location!.Id.Should().Be (destinationLocation.Id);
		returned.Last().NumberOfItems.Should().Be (1);
		returned.Last().Items.Should().HaveCount (1);
		returned.Last().Items.Should().AllSatisfy (i => i.Quantity.Should().BePositive());
		
		mockService.Verify (m => m.InsertAsync (dto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
}