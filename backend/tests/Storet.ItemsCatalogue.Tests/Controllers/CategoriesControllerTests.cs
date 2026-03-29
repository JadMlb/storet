using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Storet.Modules.ItemsCatalogue.Contracts.Categories;
using Storet.Modules.ItemsCatalogue.Controllers;
using Storet.Modules.ItemsCatalogue.Services.Categories;

namespace Storet.ItemsCatalogue.Tests.Controllers;

public class CategoriesControllerTests
{
	private readonly Mock<ICategoriesService> mockService;
	private readonly CategoriesController controller;

	public CategoriesControllerTests ()
	{
		mockService = new Mock<ICategoriesService>();
		controller = new CategoriesController (mockService.Object);
	}

	[Fact]
	public async Task GetAllShouldReturnOkWithCategories ()
	{
		var categories = new List<CategoryResponseWithSubCategories>
		{
			new () {Id = 1, Label = "Electronics"}
		};

		mockService.Setup (s => s.GetAllAsync())
					.ReturnsAsync (categories);

		var result = await controller.GetAll();

		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returnedCategories = okResult.Value as IEnumerable<CategoryResponseWithSubCategories>;
		returnedCategories.Should().HaveCount (1);
	}
	
	[Fact]
	public async Task GetByIdWithExistingIdShouldReturnOkWithCategory ()
	{
		var category = new CategoryResponseWithParent {Id = 1, Label = "Electronics"};

		mockService.Setup (s => s.GetOneAsync (1))
					.ReturnsAsync (category);

		var result = await controller.GetOne (1);

		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returnedCategory = okResult.Value as CategoryResponseWithParent;
		returnedCategory.Should().NotBeNull();
		returnedCategory.Id.Should().Be (1);
	}
	
	[Fact]
	public async Task GetByIdWithNonExistingIdShouldReturnNotFound ()
	{
		mockService.Setup (s => s.GetOneAsync (999))
					.ReturnsAsync ((CategoryResponseWithParent?) null);

		var result = await controller.GetOne (999);
		var notFoundResult = result.Result;
		notFoundResult.Should().BeOfType<NotFoundResult>();
	}
	
	[Fact]
	public async Task CreateWithValidDataShouldReturnCreated ()
	{
		var createDto = new CategoryInsertRequest
		{
			Label = "Electronics"
		};

		var created = new CategoryResponseWithParent
		{
			Id = 1,
			Label = "Electronics"
		};

		mockService.Setup (s => s.InsertAsync (createDto))
					.ReturnsAsync (created);

		var result = await controller.Create (createDto);

		var createdResult = result.Result as CreatedAtActionResult;
		createdResult.Should().NotBeNull();
		createdResult.StatusCode.Should().Be (201);
		createdResult.ActionName.Should().Be (nameof (CategoriesController.GetOne));
		createdResult.RouteValues.Should().NotBeNull();
		createdResult.RouteValues["id"].Should().Be (1);
	}

	[Fact]
	public async Task UpdateWithValidDataShouldReturnOk ()
	{
		var updateDto = new CategoryUpdateRequest
		{
			Label = "Updated Electronics"
		};
		var updated = new CategoryResponseWithParent
		{
			Id = 1,
			Label = "Updated Electronics"
		};

		mockService.Setup (s => s.UpdateAsync (1, updateDto))
					.ReturnsAsync (updated);

		var result = await controller.Update (1, updateDto);

		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returnedCategory = okResult.Value as CategoryResponseWithSubCategories;
		returnedCategory.Should().NotBeNull();
		returnedCategory.Label.Should().Be ("Updated Electronics");
	}

	[Fact]
	public async Task DeleteWithExistingIdShouldReturnNoContent ()
	{
		mockService.Setup (s => s.DeleteAsync (1))
					.ReturnsAsync (true);

		var result = await controller.Delete (1);

		result.Should().BeOfType<NoContentResult>();
	}
	
	[Fact]
	public async Task DeleteWithNonExistingIdShouldReturnNotFound ()
	{
		mockService.Setup (s => s.DeleteAsync (999))
					.ReturnsAsync (false);

		var result = await controller.Delete (999);
		result.Should().BeOfType<NotFoundResult>();
	}
	
	[Fact]
	public async Task DeleteWithChildrenShouldReturnBadRequest ()
	{
		mockService.Setup (s => s.DeleteAsync (1))
					.ThrowsAsync (new InvalidOperationException ("Cannot delete category with subcategories"));

		var result = await controller.Delete (1);
		result.Should().BeOfType<BadRequestResult>();
	}
}