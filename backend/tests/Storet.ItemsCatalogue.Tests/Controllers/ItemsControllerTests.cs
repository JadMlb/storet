using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Storet.API.Core.Exceptions;
using Storet.API.Core.Utils;
using Storet.API.ItemsCatalogue.Contracts.Items;
using Storet.API.ItemsCatalogue.Controllers;
using Storet.API.ItemsCatalogue.Models;
using Storet.API.ItemsCatalogue.Services.Items;

namespace Storet.ItemsCatalogue.Tests.Controllers;

public class ItemsControllerTests
{
	private readonly Mock<IItemsService> mockService;
	private readonly ItemsController controller;
	
	public ItemsControllerTests ()
	{
		mockService = new Mock<IItemsService>();
		controller = new ItemsController (mockService.Object);
	}
	
	private void ValidateModel<T> (T model)
	{
		var validationContext = new ValidationContext (model!);
		var validationResults = new List<ValidationResult>();
		
		Validator.TryValidateObject (model!, validationContext, validationResults, true);
		
		foreach (var result in validationResults)
			foreach (var memberName in result.MemberNames)
				controller.ModelState.AddModelError (memberName, result.ErrorMessage ?? "");
	}
	
	[Fact]
	public async Task GetAllShouldReturnOkWithPaginatedList ()
	{
		var paginatedList = new PaginatedResponse<ItemResponse, string>
		{
			Data = [
				new () {Id = Guid.NewGuid(), Name = "Item1"},
				new () {Id = Guid.NewGuid(), Name = "Item2"},
				new () {Id = Guid.NewGuid(), Name = "Item3"}
			]
		};
		
		var query = new Query<string>();
		
		mockService.Setup (s => s.GetAllAsync (It.IsAny<Query<string>>()))
					.ReturnsAsync (paginatedList);
		
		var result = await controller.GetAll (query);
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);
		
		var page = okResult.Value as PaginatedResponse<ItemResponse, string>;
		page.Should().NotBeNull();
		page.Next.Should().BeNull();
		page.Previous.Should().BeNull();
		page.HasNext.Should().BeFalse();
		page.HasPrevious.Should().BeFalse();
		page.Data.Should().HaveCount (3);
		
		mockService.Verify (s => s.GetAllAsync (It.Is<Query<string>> (q => q.Key == null)), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetOneWithExistsingIdShouldReturnOkWithItem ()
	{
		var id = Guid.NewGuid();
		var item = new ItemResponseDetails
		{
			Id = id,
			Name = "Laptop",
			Quantity = 1,
			Unit = Unit.Unit,
			Categories = [
				new () {Id = 1, Label = "Electronics"}
			]
		};
		
		mockService.Setup (s => s.GetOneAsync (id))
					.ReturnsAsync (item);
		
		var result = await controller.GetOne (id);
		var okResult = result.Result as OkObjectResult;
		
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);
		
		var itemResponse = okResult.Value as ItemResponseDetails;
		itemResponse.Should().NotBeNull();
		itemResponse.Id.Should().Be (id);
		itemResponse.Name.Should().Be ("Laptop");
		itemResponse.Quantity.Should().Be (1);
		itemResponse.Unit.Should().Be (Unit.Unit);
		itemResponse.Categories.Should().HaveCount (1);
		itemResponse.Categories.Should().Contain (c => c.Id == 1 && c.Label == "Electronics");
		
		mockService.Verify (s => s.GetOneAsync (id), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetOneWithNonExistsingIdShouldReturnNotFound ()
	{
		var id = Guid.NewGuid();
		mockService.Setup (s => s.GetOneAsync (id))
					.ReturnsAsync ((ItemResponseDetails?) null);
		
		var result = await controller.GetOne (id);
		var notFoundResult = result.Result;
		notFoundResult.Should().BeOfType<NotFoundResult>();
		
		mockService.Verify (s => s.GetOneAsync (id), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithNonExistsingCategoryIdShouldReturnNotFound ()
	{
		var item = new ItemInsertRequest
		{
			Name = "Item",
			Quantity = 1,
			Unit = Unit.Unit,
			Categories = [999]
		};
		
		mockService.Setup (s => s.InsertAsync (It.IsAny<ItemInsertRequest>()))
					.ThrowsAsync (new EntityNotFoundException (nameof (Category), new List<int> {999}));
		
		var result = await controller.Create (item);
		var notFoundResult = result.Result;
		notFoundResult.Should().BeOfType<NotFoundObjectResult>();
		
		mockService.Verify (s => s.InsertAsync (It.Is<ItemInsertRequest> (i => i.Categories.Contains (999))), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithEmptyCategoryListIdShouldReturnBadRequest ()
	{
		var item = new ItemInsertRequest
		{
			Name = "Item",
			Quantity = 1,
			Unit = Unit.Unit,
		};
		
		// manually validate since pipeline is not implemented in tests
		ValidateModel (item);
		
		var result = await controller.Create (item);
		var badRequestResult = result.Result;
		badRequestResult.Should().BeOfType<BadRequestObjectResult>();
		
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithoutUnitShouldReturnBadRequest ()
	{
		var item = new ItemInsertRequest
		{
			Name = "Item",
			Quantity = 1,
		};
		
		// manually validate since pipeline is not implemented in tests
		ValidateModel (item);
		
		var result = await controller.Create (item);
		var badRequestResult = result.Result;
		badRequestResult.Should().BeOfType<BadRequestObjectResult>();
		
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithNonPositiveQuantityShouldReturnBadRequest ()
	{
		var item = new ItemInsertRequest
		{
			Name = "Item",
			Quantity = -1,
			Unit = Unit.Unit
		};
		
		// manually validate since pipeline is not implemented in tests
		ValidateModel (item);
		
		var result = await controller.Create (item);
		var badRequestResult = result.Result;
		badRequestResult.Should().BeOfType<BadRequestObjectResult>();
		
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithInvalidComponentToCreateShouldReturnBadRequest ()
	{
		var item = new ItemInsertRequest
		{
			Name = "Item",
			Quantity = 1,
			Unit = Unit.Unit,
			Components = [
				new ()
				{
					Description = "Test"
				}
			]
		};
		
		// manually validate since pipeline is not implemented in tests
		ValidateModel (item);
		
		var result = await controller.Create (item);
		var badRequestResult = result.Result;
		badRequestResult.Should().BeOfType<BadRequestObjectResult>();
		
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithValidDataShouldReturnCreatedAtWithCreatedItem ()
	{
		var itemDto = new ItemInsertRequest
		{
			Name = "Item",
			Categories = [1],
			Quantity = 1,
			Unit = Unit.Unit
		};
		
		var item = new ItemResponseDetails
		{
			Id = Guid.NewGuid(),
			Name = "Item",
			Quantity = 1,
			Unit = Unit.Unit,
			Categories = [
				new () {Id = 1, Label = "Category"}
			]
		};
		
		// manually validate since pipeline is not implemented in tests
		ValidateModel (itemDto);
		
		mockService.Setup (s => s.InsertAsync (It.IsAny<ItemInsertRequest>()))
					.ReturnsAsync (item);
		
		var result = await controller.Create (itemDto);
		var createdResult = result.Result as CreatedAtActionResult;
	
		createdResult.Should().NotBeNull();
		createdResult.StatusCode.Should().Be (201);
		createdResult.ActionName.Should().Be (nameof (ItemsController.GetOne));
		createdResult.RouteValues.Should().NotBeNull();
		createdResult.RouteValues["id"].Should().Be (item.Id);
		createdResult.Value.Should().NotBeNull();
		
		mockService.Verify (
			s => s.InsertAsync (
				It.Is<ItemInsertRequest> (
					i => i.Name == "Item" && i.Categories.Count > 0 && i.Categories.Contains (1)
				)
			),
			Times.Once()
		);
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateItemWithNonExistingItemShouldReturnNotFound ()
	{
		var nonExistentItemId = Guid.NewGuid();
		var updatedValues = new ItemUpdateRequest
		{
			Description = "This is a test"
		};
		mockService.Setup (s => s.UpdateAsync (nonExistentItemId, updatedValues))
					.ReturnsAsync ((ItemResponseDetails?) null);
		
		ValidateModel (updatedValues);
		var result = await controller.Update (nonExistentItemId, updatedValues);
		result.Result.Should().BeOfType<NotFoundResult>();
		
		mockService.Verify (s => s.UpdateAsync (nonExistentItemId, updatedValues), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateItemWithNonExistingCategoryShouldReturnNotFound ()
	{
		var itemId = Guid.NewGuid();
		var updatedValues = new ItemUpdateRequest
		{
			Description = "This is a test",
			Categories = [2]
		};
		mockService.Setup (s => s.UpdateAsync (itemId, updatedValues))
					.ThrowsAsync (new EntityNotFoundException (nameof (Category), new List<int> {2}));
		
		ValidateModel (updatedValues);
		var result = await controller.Update (itemId, updatedValues);
		result.Result.Should().BeOfType<NotFoundObjectResult>();
		
		mockService.Verify (s => s.UpdateAsync (itemId, updatedValues), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateItemWithNonExistingItemComponentShouldReturnNotFound ()
	{
		var itemId = Guid.NewGuid();
		var componentId = Guid.NewGuid();
		var updatedValues = new ItemUpdateRequest
		{
			Description = "This is a test",
			Components = [
				new ()
				{
					Id = componentId,
					Quantity = 1
				}
			],
		};
		mockService.Setup (s => s.UpdateAsync (itemId, updatedValues))
					.ThrowsAsync (new EntityNotFoundException (nameof (Item), new List<Guid> {componentId}));
		
		ValidateModel (updatedValues);
		var result = await controller.Update (itemId, updatedValues);
		result.Result.Should().BeOfType<NotFoundObjectResult>();
		
		mockService.Verify (s => s.UpdateAsync (itemId, updatedValues), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateItemWithValidDataShouldReturnOkWithUpdatedItem ()
	{
		var itemId = Guid.NewGuid();
		var updatedValues = new ItemUpdateRequest
		{
			Description = "This is a test",
			Quantity = 2,
			Categories = [2]
		};
		var item = new ItemResponseDetails
		{
			Id = itemId,
			Name = "Item",
			Description = "This is a test",
			Quantity = 2,
			Unit = Unit.Unit,
			Categories = [
				new () {Id = 2, Label = "Category"}
			]
		};
		
		mockService.Setup (s => s.UpdateAsync (itemId, updatedValues))
					.ReturnsAsync (item);
		
		ValidateModel (updatedValues);
		var result = await controller.Update (itemId, updatedValues);
		var okResult = result.Result as OkObjectResult;
		
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);
		okResult.Value.Should().NotBeNull();
		
		mockService.Verify (s => s.UpdateAsync (itemId, updatedValues), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteItemWithNonExistingItemIdShouldReturnNotFound ()
	{
		var nonExistentItemId = Guid.NewGuid();
		mockService.Setup (s => s.DeleteAsync (nonExistentItemId))
					.ReturnsAsync (false);
		
		var result = await controller.Delete (nonExistentItemId);
		result.Should().BeOfType<NotFoundResult>();
		
		mockService.Verify (s => s.DeleteAsync (nonExistentItemId), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteItemWithExistingItemIdShouldReturnNoContent ()
	{
		var nonExistentItemId = Guid.NewGuid();
		mockService.Setup (s => s.DeleteAsync (nonExistentItemId))
					.ReturnsAsync (true);
		
		var result = await controller.Delete (nonExistentItemId);
		result.Should().BeOfType<NoContentResult>();
		
		mockService.Verify (s => s.DeleteAsync (nonExistentItemId), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
}