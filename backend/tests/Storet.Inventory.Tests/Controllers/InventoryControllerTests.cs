using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Storet.Core.Exceptions;
using Storet.Core.Utils;
using Storet.Modules.Inventory.Contracts.Inventory;
using Storet.Modules.Inventory.Controllers;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Services.Inventory;
using Storet.Modules.ItemsCatalogue.Contracts.Items;
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
	public async Task GetAllWithNoInventoryShouldReturnOkWithEmptyList ()
	{
		mockService.Setup (i => i.GetAllAsync())
					.ReturnsAsync ([]);
								
		var result = await controller.GetAll();
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returned = okResult.Value as IEnumerable<InventoryResponse>;
		returned.Should().BeEmpty();
		
		mockService.Verify (i => i.GetAllAsync(), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllWithDataShouldReturnOkWithList ()
	{
		Guid sugarId = Guid.NewGuid(), soapId = Guid.NewGuid(), appleId = Guid.NewGuid();
		var inventory = new List<InventoryResponse>
		{
			new () {Item = new ItemResponse {Id = sugarId, Name = "Sugar"}, QuantityInStock = 0, Status = Status.EmptyAccepted},
			new () {Item = new ItemResponse {Id = soapId, Name = "Soap"}, QuantityInStock = 3, Status = Status.Sufficient},
			new () {Item = new ItemResponse {Id = appleId, Name = "Apple"}, MaxQuantity = 10, QuantityInStock = 3, Status = Status.Critical},
		};
		
		mockService.Setup (i => i.GetAllAsync())
					.ReturnsAsync (inventory);
								
		var result = await controller.GetAll();
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returned = okResult.Value as IEnumerable<InventoryResponse>;
		returned.Should().HaveCount (3);
		returned.Should().AllSatisfy (i => i.Should().NotBeNull());
		returned.Should().AllSatisfy (i => i.Item.Should().NotBeNull());
		returned.Should().Contain (i => i.Item.Id == sugarId && i.QuantityInStock == 0 && i.Status == Status.EmptyAccepted);
		returned.Should().Contain (i => i.Item.Id == soapId && i.QuantityInStock == 3 && i.Status == Status.Sufficient);
		returned.Should().Contain (i => i.Item.Id == appleId && i.MaxQuantity == 10 && i.QuantityInStock == 3 && i.Status == Status.Critical);
		
		mockService.Verify (i => i.GetAllAsync(), Times.Once());
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
		
		var result = await controller.Update (itemId, updateDto);
		
		result.Result.Should().BeOfType<NotFoundObjectResult>();
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
					.ThrowsAsync (new ArgumentException ("Max quantity must be greater than min quantity"));
		
		var result = await controller.Update (itemId, updateDto);
		
		result.Result.Should().BeOfType<BadRequestObjectResult>();
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
		
		var result = await controller.Update (itemId, updateDto);
		
		result.Result.Should().BeOfType<NotFoundObjectResult>();
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
							Item = new ItemResponse
							{
								Id = itemId,
								Name = "Cup"
							},
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
		returned.Item.Should().NotBeNull();
		returned.Item.Id.Should().Be (itemId);
		returned.Item.Name.Should().Be ("Cup");
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
							Item = new ItemResponse
							{
								Id = itemId,
								Name = "Cup"
							},
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
		returned.Item.Should().NotBeNull();
		returned.Item.Id.Should().Be (itemId);
		returned.Item.Name.Should().Be ("Cup");
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
							Item = new ItemResponse
							{
								Id = itemId,
								Name = "Cup"
							},
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
		returned.Item.Should().NotBeNull();
		returned.Item.Id.Should().Be (itemId);
		returned.Item.Name.Should().Be ("Cup");
		returned.MaxQuantity.Should().Be (10);
		returned.MinQuantity.Should().Be (0);
		returned.QuantityInStock.Should().Be (2);
		returned.Status.Should().Be (Status.Critical);
		
		mockService.Verify (i => i.UpdateAsync (itemId, updateDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
}