using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Storet.Core.Exceptions;
using Storet.Modules.Inventory.Contracts.Inventory;
using Storet.Modules.Inventory.Controllers;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Queries;
using Storet.Modules.Inventory.Services.Inventory;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Tests.Common.Controller;

namespace Storet.Inventory.Tests.Controllers;

public class InventoryControllerTests : ControllerTestsBase<InventoryController, IInventoryService>
{
	protected override InventoryController InitControllerInstance ()
	{
		return new InventoryController (mockService.Object);
	}

	[Fact]
	public async Task GetAllWithEmptyStringShouldReturnBadRequest ()
	{
		InventoryFilterQuery query = new ()
		{
			Items = ""
		};

		mockService.Setup (i => i.GetAllAsync (It.IsAny<InventoryFilterQuery>()))
					.ThrowsAsync (new MalformedRequestException ("Cannot fetch inventories for empty items list"));

		Func<Task> act = async () => await controller.GetAll (query);

		await act.Should().ThrowAsync<MalformedRequestException>()
							.WithMessage ("Cannot fetch inventories for empty items list");

		mockService.Verify (i => i.GetAllAsync (query), Times.Once());
		mockService.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetAllWithInvalidGuidStringShouldReturnBadRequest ()
	{
		InventoryFilterQuery query = new ()
		{
			Items = "123-4qdskjsd-kaskas"
		};
		
		mockService.Setup (i => i.GetAllAsync (It.IsAny<InventoryFilterQuery>()))
					.ThrowsAsync (new MalformedRequestException ("One or more provided ID is not a valid Guid"));

		Func<Task> act = async () => await controller.GetAll (query);

		await act.Should().ThrowAsync<MalformedRequestException>()
							.WithMessage ("One or more provided ID is not a valid Guid");

		mockService.Verify (i => i.GetAllAsync (query), Times.Once());
		mockService.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetAllWithNoInventoryShouldReturnOkWithDictionaryWithEmptyArray ()
	{
		var itemId = Guid.NewGuid();
		InventoryFilterQuery query = new ()
		{
			Items = itemId.ToString()
		};
		var inventories = new Dictionary<Guid, IEnumerable<InventoryResponse>>
		{
			{itemId, []}
		};

		mockService.Setup (i => i.GetAllAsync (It.IsAny<InventoryFilterQuery>()))
					.ReturnsAsync (inventories);

		var result = await controller.GetAll (query);
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returned = okResult.Value as Dictionary<Guid, IEnumerable<InventoryResponse>>;

		returned.Should().NotBeEmpty();
		var resultForItemId = returned.GetValueOrDefault (itemId);
		resultForItemId.Should().NotBeNull();
		resultForItemId.Should().BeEmpty();

		mockService.Verify (i => i.GetAllAsync (query), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllWithDataShouldReturnList ()
	{
		Guid sugarBoxId = Guid.NewGuid(), sugarId = Guid.NewGuid(), soapId = Guid.NewGuid(), appleId = Guid.NewGuid();
		InventoryFilterQuery query = new ()
		{
			Items = $"{sugarBoxId},{soapId}"
		};
		var inventory = new Dictionary<Guid, IEnumerable<InventoryResponse>>
		{
			{sugarBoxId, [new () {ItemId = sugarId, QuantityInStock = 0, Status = Status.EmptyAccepted}]},
			{soapId, [new () {ItemId = soapId, QuantityInStock = 3, Status = Status.Sufficient}]},
			{appleId, [new () {ItemId = appleId, MaxQuantity = 10, QuantityInStock = 3, Status = Status.Critical}]},
		};
		
		mockService.Setup (i => i.GetAllAsync (It.IsAny<InventoryFilterQuery>()))
								.ReturnsAsync (
									(InventoryFilterQuery query) =>
									{
										IEnumerable<Guid> ids = query.Items.Split(",").Select (Guid.Parse);
										return inventory.Where (kv => ids.Contains (kv.Key))
														.ToDictionary();
									}
								);
		
		var result = await controller.GetAll (query);
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returned = okResult.Value as Dictionary<Guid, IEnumerable<InventoryResponse>>;

		returned.Should().NotBeEmpty();
		returned.Should().HaveCount (2);
		returned.Should().Contain (i =>
			i.Key == sugarBoxId
			&& i.Value.Count() == 1
			&& i.Value.First().ItemId == sugarId
			&& i.Value.First().QuantityInStock == 0
			&& i.Value.First().Status == Status.EmptyAccepted
		);
		returned.Should().Contain (i =>
			i.Key == soapId
			&& i.Value.Count() == 1
			&& i.Value.First().ItemId == soapId
			&& i.Value.First().QuantityInStock == 3
			&& i.Value.First().Status == Status.Sufficient
		);

		mockService.Verify (i => i.GetAllAsync (query), Times.Once());
		mockService.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetOneWithNonExistsingInventoryEntryOrItemIdOrComponentsShouldReturnEmptyList ()
	{
		var nonExistingItemId = Guid.NewGuid();

		mockService.Setup (i => i.GetOneAsync (It.IsAny<Guid>()))
					.ReturnsAsync ([]);
		
		var result = await controller.GetOne (nonExistingItemId);
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returned = okResult.Value as IEnumerable<InventoryResponse>;
		returned.Should().BeEmpty();

		mockService.Verify (i => i.GetOneAsync (It.Is<Guid> (id => id == nonExistingItemId)), Times.Once());
		mockService.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetOneWithExistingComponentsShouldReturnList ()
	{
		var itemId = Guid.NewGuid();

		mockService.Setup (i => i.GetOneAsync (It.IsAny<Guid>()))
					.ReturnsAsync ([
						new ()
						{
							ItemId = itemId,
							QuantityInStock = 10,
							Status = Status.Sufficient
						}
					]);
		
		var result = await controller.GetOne (itemId);
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returned = okResult.Value as IEnumerable<InventoryResponse>;
		returned.Should().HaveCount (1);
		var first = returned.First();
		first.Should().NotBeNull();
		first.ItemId.Should().Be (itemId);
		first.QuantityInStock.Should().Be (10);
		first.Status.Should().Be (Status.Sufficient);

		mockService.Verify (i => i.GetOneAsync (It.Is<Guid> (id => id == itemId)), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithNonExistentInventoryEntryShouldReturnNotFound ()
	{
		var updateDto = new InventoryUpdateRequest
		{
			MinQuantity = 1
		};
		var itemId = Guid.NewGuid();
		
		ValidateModel (updateDto);
		mockService.Setup (i => i.UpdateAsync (itemId, updateDto))
					.ThrowsAsync (new EntityNotFoundException (nameof (Modules.Inventory.Models.Inventory), itemId));
		
		Func<Task> act = async () => await controller.Update (itemId, updateDto);

		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Inventory with ID {itemId} was not found");
		mockService.Verify (i => i.UpdateAsync (itemId, updateDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithMaxQuantityLessThanMinQuantityInRequestShouldReturnBadRequest ()
	{
		var itemId = Guid.NewGuid();
		var updateDto = new InventoryUpdateRequest
		{
			MinQuantity = 10,
			MaxQuantity = 1
		};
		
		ValidateModel (updateDto);
		mockService.Setup (i => i.UpdateAsync (itemId, updateDto))
					.ThrowsAsync (new MalformedRequestException ("Max quantity must be greater than min quantity"));
		
		Func<Task> act = async () => await controller.Update (itemId, updateDto);
		
		await act.Should().ThrowAsync<MalformedRequestException>()
							.WithMessage ("Max quantity must be greater than min quantity");
		mockService.Verify (i => i.UpdateAsync (itemId, updateDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithNonExistentItemShouldReturnNotFound ()
	{
		var updateDto = new InventoryUpdateRequest
		{
			MinQuantity = 1
		};
		
		var itemId = Guid.NewGuid();
		
		ValidateModel (updateDto);
		mockService.Setup (i => i.UpdateAsync (itemId, updateDto))
					.ThrowsAsync (new EntityNotFoundException (nameof (Item), itemId));
		
		Func<Task> act = async () => await controller.Update (itemId, updateDto);

		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Item with ID {itemId} was not found");
		mockService.Verify (i => i.UpdateAsync (itemId, updateDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithInvalidMinQuantityShouldReturnBadRequest ()
	{
		var itemId = Guid.NewGuid();
		var updateDto = new InventoryUpdateRequest
		{
			MinQuantity = -1
		};
		
		ValidateModel (updateDto);
		
		var result = await controller.Update (itemId, updateDto);
		
		result.Result.Should().BeOfType<BadRequestObjectResult>();
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithInvalidMaxQuantityShouldReturnBadRequest ()
	{
		var itemId = Guid.NewGuid();
		var updateDto = new InventoryUpdateRequest
		{
			MaxQuantity = -1
		};
		
		ValidateModel (updateDto);
		
		var result = await controller.Update (itemId, updateDto);
		
		result.Result.Should().BeOfType<BadRequestObjectResult>();
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithValidDataAndNoItemsInStockShouldReturnOk ()
	{
		var itemId = Guid.NewGuid();
		var updateDto = new InventoryUpdateRequest
		{
			MaxQuantity = 10
		};
		
		ValidateModel (updateDto);
		mockService.Setup (i => i.UpdateAsync (itemId, It.IsAny<InventoryUpdateRequest>()))
					.ReturnsAsync (
						new InventoryResponse
						{
							ItemId = itemId,
							MaxQuantity = 10,
							QuantityInStock = 0,
							Status = Status.EmptyAccepted
						}
					);
		
		var result = await controller.Update (itemId, updateDto);
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returned = okResult.Value as InventoryResponse;
		returned.Should().NotBeNull();
		returned.ItemId.Should().Be (itemId);
		returned.MaxQuantity.Should().Be (10);
		returned.MinQuantity.Should().Be (0);
		returned.QuantityInStock.Should().Be (0);
		returned.Status.Should().Be (Status.EmptyAccepted);
		
		mockService.Verify (i => i.UpdateAsync (itemId, updateDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithValidDataAndExistingItemsInStockShouldReturnOkWithStatusSufficient ()
	{
		var itemId = Guid.NewGuid();
		var updateDto = new InventoryUpdateRequest
		{
			MaxQuantity = 10
		};
		
		ValidateModel (updateDto);
		mockService.Setup (i => i.UpdateAsync (itemId, It.IsAny<InventoryUpdateRequest>()))
					.ReturnsAsync (
						new InventoryResponse
						{
							ItemId = itemId,
							MaxQuantity = 10,
							QuantityInStock = 6,
							Status = Status.Sufficient
						}
					);
		
		var result = await controller.Update (itemId, updateDto);
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returned = okResult.Value as InventoryResponse;
		returned.Should().NotBeNull();
		returned.ItemId.Should().Be (itemId);
		returned.MaxQuantity.Should().Be (10);
		returned.MinQuantity.Should().Be (0);
		returned.QuantityInStock.Should().Be (6);
		returned.Status.Should().Be (Status.Sufficient);
		
		mockService.Verify (i => i.UpdateAsync (itemId, updateDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithValidDataAndExistingItemsInStockShouldReturnOkWithStatusCritical ()
	{
		var itemId = Guid.NewGuid();
		var updateDto = new InventoryUpdateRequest
		{
			MaxQuantity = 10
		};
		
		ValidateModel (updateDto);
		mockService.Setup (i => i.UpdateAsync (itemId, It.IsAny<InventoryUpdateRequest>()))
					.ReturnsAsync (
						new InventoryResponse
						{
							ItemId = itemId,
							MaxQuantity = 10,
							QuantityInStock = 2,
							Status = Status.Critical
						}
					);
		
		var result = await controller.Update (itemId, updateDto);
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returned = okResult.Value as InventoryResponse;
		returned.Should().NotBeNull();
		returned.ItemId.Should().Be (itemId);
		returned.MaxQuantity.Should().Be (10);
		returned.MinQuantity.Should().Be (0);
		returned.QuantityInStock.Should().Be (2);
		returned.Status.Should().Be (Status.Critical);
		
		mockService.Verify (i => i.UpdateAsync (itemId, updateDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
}