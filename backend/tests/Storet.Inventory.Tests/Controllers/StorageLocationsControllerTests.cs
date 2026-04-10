using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Storet.Core.Exceptions;
using Storet.Modules.Inventory.Contracts.StorageLocation;
using Storet.Modules.Inventory.Controllers;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Services.StorageLocations;
using Storet.Tests.Common.Controller;

namespace Storet.Inventory.Tests.Controllers;

public class StorageLocationsControllerTests : ControllerTestsBase<StorageLocationsController, IStorageLocationsService>
{
	protected override StorageLocationsController InitControllerInstance ()
	{
		return new StorageLocationsController (mockService.Object);
	}
	
	[Fact]
	public async Task GetAllStorageLocationsWithEmptyOrNoMatchShouldReturnEmptyList ()
	{
		mockService.Setup (l => l.GetAllAsync ())
					.ReturnsAsync ([]);
										
		var result = await controller.GetAll();
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returnedLocs = okResult.Value as IEnumerable<StorageLocationResponse>;
		returnedLocs.Should().BeEmpty();
		
		mockService.Verify (l => l.GetAllAsync(), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllStorageLocationsWithMatchShouldReturnList ()
	{
		mockService.Setup (l => l.GetAllAsync())
					.ReturnsAsync ([
						new StorageLocationResponse
						{
							Id = Guid.NewGuid(),
							Name = "Cupboard"
						},
						new StorageLocationResponse
						{
							Id = Guid.NewGuid(),
							Name = "Drawer"
						},
					]);
										
		var result = await controller.GetAll();
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returnedLocs = okResult.Value as IEnumerable<StorageLocationResponse>;
		returnedLocs.Should().NotBeEmpty();
		returnedLocs.Should().HaveCount (2);
		returnedLocs.Should().Contain (l => l.Name == "Cupboard");
		returnedLocs.Should().Contain (l => l.Name == "Drawer");
		
		mockService.Verify (l => l.GetAllAsync(), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task InsertWithEmptyNameShouldThrowArgumentException ()
	{
		var insertDto = new StorageLocationInsertRequest
		{
			Name = ""
		};
		
		ValidateModel (insertDto);
		
		var result = await controller.Create (insertDto);
		var badRequestResult = result.Result;
		badRequestResult.Should().BeOfType<BadRequestObjectResult>();
		
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task InsertWithDuplicateNameShouldThrowArgumentException ()
	{
		var insertDto = new StorageLocationInsertRequest
		{
			Name = "Cupboard"
		};
		
		ValidateModel (insertDto);
		mockService.Setup (l => l.InsertAsync (insertDto))
					.ThrowsAsync (new DuplicateKeyException (nameof (StorageLocation), "Cupboard"));
		
		var result = await controller.Create (insertDto);
		var conflictResult = result.Result;
		conflictResult.Should().BeOfType<ConflictObjectResult>();
		
		mockService.Verify (l => l.InsertAsync (insertDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task InsertWithValidNameShouldReturnInserted ()
	{
		var insertDto = new StorageLocationInsertRequest
		{
			Name = "Cupboard"
		};
		
		ValidateModel (insertDto);
		
		var id = Guid.NewGuid();
		mockService.Setup (l => l.InsertAsync (It.IsAny<StorageLocationInsertRequest>()))
										.ReturnsAsync (
											(StorageLocationInsertRequest l) =>
											new StorageLocationDetailsResponse
											{
												Id = id,
												Name = l.Name,
												Description = l.Description
											}
										);
		
		var result = await controller.Create (insertDto);
		
		var createdResult = result.Result as CreatedAtActionResult;
		createdResult.Should().NotBeNull();
		createdResult.StatusCode.Should().Be (201);
		createdResult.ActionName.Should().Be (nameof (StorageLocationsController.GetOne));
		createdResult.RouteValues.Should().NotBeNull();
		createdResult.RouteValues["id"].Should().Be (id);
		
		mockService.Verify (l => l.InsertAsync (insertDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateWithEmptyNameShouldThrowArgumentException ()
	{
		var updateDto = new StorageLocationUpdateRequest
		{
			Name = ""
		};
		
		ValidateModel (updateDto);
		
		var id = Guid.NewGuid();
		mockService.Setup (l => l.UpdateAsync (id, updateDto))
					.ThrowsAsync (new ArgumentException ("Name cannot be empty"));
		
		var result = await controller.Update (id, updateDto);
		var badRequestResult = result.Result;
		badRequestResult.Should().BeOfType<BadRequestObjectResult>();
		
		mockService.Verify (l => l.UpdateAsync (id, updateDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateWithDuplicateNameShouldDuplicateKeyException ()
	{
		var updateDto = new StorageLocationUpdateRequest
		{
			Name = "Cupboard"
		};
		var id = Guid.NewGuid();
		
		ValidateModel (updateDto);
		
		mockService.Setup (l => l.UpdateAsync (id, updateDto))
					.ThrowsAsync (new DuplicateKeyException (nameof (StorageLocation), "Cupboard"));
		
		var result = await controller.Update (id, updateDto);
		var conflictResult = result.Result;
		conflictResult.Should().BeOfType<ConflictObjectResult>();
		
		mockService.Verify (l => l.UpdateAsync (id, updateDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateWithNonExistingIdShouldReturnNull ()
	{
		var updateDto = new StorageLocationUpdateRequest
		{
			Name = "Cupboard"
		};
		var id = Guid.NewGuid();
		
		ValidateModel (updateDto);
		
		mockService.Setup (l => l.UpdateAsync (id, updateDto))
					.ReturnsAsync ((StorageLocationDetailsResponse?) null);
		
		var result = await controller.Update (id, updateDto);
		var conflictResult = result.Result;
		conflictResult.Should().BeOfType<NotFoundResult>();
		
		mockService.Verify (l => l.UpdateAsync (id, updateDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateWithValidNameShouldReturnUpdated ()
	{
		var updateDto = new StorageLocationUpdateRequest
		{
			Name = "Cupboard"
		};
		ValidateModel (updateDto);
		
		var id = Guid.NewGuid();
		mockService.Setup (l => l.UpdateAsync (id, It.IsAny<StorageLocationUpdateRequest>()))
					.ReturnsAsync (
						(Guid id, StorageLocationUpdateRequest l) =>
						new StorageLocationDetailsResponse
						{
							Id = id,
							Name = l.Name!,
							Description = l.Description
						}
					);
		
		var result = await controller.Update (id, updateDto);
		
		var okResult = result.Result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult.StatusCode.Should().Be (200);

		var returnedLocation = okResult.Value as StorageLocationDetailsResponse;
		returnedLocation.Should().NotBeNull();
		returnedLocation.Name.Should().Be ("Cupboard");
		
		mockService.Verify (l => l.UpdateAsync (id, updateDto), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteUsedLocationShouldThrowDependencyException ()
	{
		var id = Guid.NewGuid();
		
		mockService.Setup (l => l.DeleteAsync (id))
					.ThrowsAsync (new EntityDependencyException (nameof (StorageLocation), id));
		
		var result = await controller.Delete (id);
		result.Should().BeOfType<ConflictObjectResult>();
		
		mockService.Verify (l => l.DeleteAsync (id), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteWithNonExistingIdShouldReturnFalse ()
	{
		var id = Guid.NewGuid();
		
		mockService.Setup (l => l.DeleteAsync (id))
					.ReturnsAsync (false);
		
		var result = await controller.Delete (id);
		result.Should().BeOfType<NotFoundResult>();
		
		mockService.Verify (l => l.DeleteAsync (id), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteWithExistingIdShouldReturnTrue ()
	{
		var id = Guid.NewGuid();
		
		mockService.Setup (l => l.DeleteAsync (id))
					.ReturnsAsync (true);
		
		var result = await controller.Delete (id);
		result.Should().BeOfType<NoContentResult>();
		
		mockService.Verify (l => l.DeleteAsync (id), Times.Once());
		mockService.VerifyNoOtherCalls();
	}
}