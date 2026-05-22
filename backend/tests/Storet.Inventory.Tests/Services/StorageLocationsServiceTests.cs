using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Storet.Core.Authorization;
using Storet.Core.Exceptions;
using Storet.Core.Mappers;
using Storet.Modules.Inventory.Contracts.StorageLocation;
using Storet.Modules.Inventory.Mappers;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Repositories.StorageLocations;
using Storet.Modules.Inventory.Services.StorageLocations;

namespace Storet.Inventory.Tests.Services;

public class StorageLocationsServiceTests
{
	private readonly IMapper mapper;
	private readonly ILoggerFactory loggerFactory;
	private readonly StorageLocationsService service;
	private readonly Mock<IStorageLocationsRepository> mockStorageLocationsRepository;
	private readonly Mock<ICurrentUser> mockCurrentUser;
	private readonly Guid userId = Guid.NewGuid();
	
	public StorageLocationsServiceTests ()
	{
		mockCurrentUser = new Mock<ICurrentUser>();
		mockStorageLocationsRepository = new Mock<IStorageLocationsRepository>();
		loggerFactory = new LoggerFactory();
		
		var locationInsertRequestResolver = new CurrentUserResolver<StorageLocationInsertRequest, StorageLocation> (mockCurrentUser.Object);
		var locationUpdateRequestResolver = new CurrentUserResolver<StorageLocationUpdateRequest, StorageLocation> (mockCurrentUser.Object);
		var config = new MapperConfiguration (
			cfg =>
			{
				cfg.ConstructServicesUsing (
					type => type == typeof (CurrentUserResolver<StorageLocationInsertRequest, StorageLocation>) ? locationInsertRequestResolver :
								type == typeof (CurrentUserResolver<StorageLocationUpdateRequest, StorageLocation>) ? locationUpdateRequestResolver :
								null
				);
				cfg.AddProfile<MappingProfile>();
			},
			loggerFactory
		);
		mapper = config.CreateMapper();
		
		service = new StorageLocationsService (mockStorageLocationsRepository.Object, mockCurrentUser.Object, mapper);
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
	}
	
	private void VerifyUserAccessedNTimes (int n = 1)
	{
		mockCurrentUser.Verify (u => u.Id, Times.Exactly (n));
	}
	
	private void VerifyNoOtherCalls ()
	{
		mockCurrentUser.VerifyNoOtherCalls();
		mockStorageLocationsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllStorageLocationsWithEmptyOrNoMatchShouldReturnEmptyList ()
	{
		mockStorageLocationsRepository.Setup (l => l.GetAllAsync (userId))
										.ReturnsAsync ([]);
										
		var result = await service.GetAllAsync();
		result.Should().BeEmpty();
		
		mockStorageLocationsRepository.Verify (l => l.GetAllAsync (userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllStorageLocationsWithMatchShouldReturnList ()
	{
		mockStorageLocationsRepository.Setup (l => l.GetAllAsync (userId))
										.ReturnsAsync ([
											new StorageLocation
											{
												Id = Guid.NewGuid(),
												UserId = userId,
												Name = "Cupboard"
											},
											new StorageLocation
											{
												Id = Guid.NewGuid(),
												UserId = userId,
												Name = "Drawer"
											},
										]);
										
		var result = await service.GetAllAsync();
		result.Should().NotBeEmpty();
		result.Should().HaveCount (2);
		result.Should().Contain (l => l.Name == "Cupboard");
		result.Should().Contain (l => l.Name == "Drawer");
		
		mockStorageLocationsRepository.Verify (l => l.GetAllAsync (userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task InsertWithEmptyNameShouldThrowMalformedRequestException ()
	{
		var insertDto = new StorageLocationInsertRequest
		{
			Name = ""
		};
		
		Func<Task> act = async () => await service.InsertAsync (insertDto);
		
		await act.Should().ThrowAsync<MalformedRequestException>()
							.WithMessage ("Name cannot be empty");
		
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task InsertWithDuplicateNameShouldThrowMalformedRequestException ()
	{
		var insertDto = new StorageLocationInsertRequest
		{
			Name = "Cupboard"
		};
		
		mockStorageLocationsRepository.Setup (l => l.InsertAsync (It.IsAny<StorageLocation>()))
										.ThrowsAsync (new DbUpdateException());
		
		Func<Task> act = async () => await service.InsertAsync (insertDto);
		
		await act.Should().ThrowAsync<DuplicateKeyException>()
							.WithMessage ($"StorageLocation with ID Cupboard already exists");
		
		mockStorageLocationsRepository.Verify (l => l.InsertAsync (It.Is<StorageLocation> (i => i.Name == "Cupboard")), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task InsertWithValidNameShouldReturnInserted ()
	{
		var insertDto = new StorageLocationInsertRequest
		{
			Name = "Cupboard"
		};
		
		var id = Guid.NewGuid();
		mockStorageLocationsRepository.Setup (l => l.InsertAsync (It.IsAny<StorageLocation>()))
										.ReturnsAsync (
											(StorageLocation l) =>
											{
												l.Id = id;
												return l;
											}
										);
		
		var res = await service.InsertAsync (insertDto);
		
		res.Should().NotBeNull();
		res.Id.Should().Be (id);
		res.Name.Should().Be ("Cupboard");
		
		mockStorageLocationsRepository.Verify (l => l.InsertAsync (It.Is<StorageLocation> (i => i.Name == "Cupboard")), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateWithEmptyNameShouldThrowMalformedRequestException ()
	{
		var updateDto = new StorageLocationUpdateRequest
		{
			Name = ""
		};
		
		Func<Task> act = async () => await service.UpdateAsync (Guid.NewGuid(), updateDto);
		
		await act.Should().ThrowAsync<MalformedRequestException>()
							.WithMessage ("Name cannot be empty");
		
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateWithDuplicateNameShouldDuplicateKeyException ()
	{
		var updateDto = new StorageLocationUpdateRequest
		{
			Name = "Cupboard"
		};
		var id = Guid.NewGuid();
		
		mockStorageLocationsRepository.Setup (l => l.GetOneAsync (id, userId))
										.ReturnsAsync (
											new StorageLocation
											{
												Id = id,
												Name = "cupbOard"
											}
										);
		mockStorageLocationsRepository.Setup (l => l.UpdateAsync (id, userId, It.IsAny<StorageLocation>()))
										.ThrowsAsync (new DbUpdateException());
		
		Func<Task> act = async () => await service.UpdateAsync (id, updateDto);
		
		await act.Should().ThrowAsync<DuplicateKeyException>()
							.WithMessage ($"StorageLocation with ID Cupboard already exists");
		
		mockStorageLocationsRepository.Verify (l => l.GetOneAsync (id, userId), Times.Once());
		mockStorageLocationsRepository.Verify (l => l.UpdateAsync (id, userId, It.Is<StorageLocation> (i => i.Name == "Cupboard")), Times.Once());
		VerifyUserAccessedNTimes (3);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateWithNonExistingIdShouldReturnNull ()
	{
		var updateDto = new StorageLocationUpdateRequest
		{
			Name = "Cupboard"
		};
		var id = Guid.NewGuid();
		
		mockStorageLocationsRepository.Setup (l => l.GetOneAsync (id, userId))
										.ReturnsAsync ((StorageLocation?) null);
		
		var res = await service.UpdateAsync (id, updateDto);
		
		res.Should().BeNull();
		
		mockStorageLocationsRepository.Verify (l => l.GetOneAsync (id, userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateWithValidNameShouldReturnUpdated ()
	{
		var updateDto = new StorageLocationUpdateRequest
		{
			Name = "Cupboard"
		};
		
		var id = Guid.NewGuid();
		mockStorageLocationsRepository.Setup (l => l.GetOneAsync (id, userId))
										.ReturnsAsync (
											new StorageLocation
											{
												Id = id,
												Name = "cupbOard"
											}
										);
		mockStorageLocationsRepository.Setup (l => l.UpdateAsync (id, userId, It.IsAny<StorageLocation>()))
										.ReturnsAsync (
											(Guid id, Guid userId, StorageLocation l) =>
											{
												l.Id = id;
												return l;
											}
										);
		
		var res = await service.UpdateAsync (id, updateDto);
		
		res.Should().NotBeNull();
		res.Id.Should().Be (id);
		res.Name.Should().Be ("Cupboard");
		
		mockStorageLocationsRepository.Verify (l => l.GetOneAsync (id, userId), Times.Once());
		mockStorageLocationsRepository.Verify (l => l.UpdateAsync (id, userId, It.Is<StorageLocation> (i => i.Name == "Cupboard")), Times.Once());
		VerifyUserAccessedNTimes (3);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteUsedLocationShouldThrowDependencyException ()
	{
		var id = Guid.NewGuid();
		
		mockStorageLocationsRepository.Setup (l => l.DeleteAsync (id, userId))
										.ThrowsAsync (new DbUpdateException());
		
		Func<Task> act = async () => await service.DeleteAsync (id);
		
		await act.Should().ThrowAsync<EntityDependencyException>()
							.WithMessage ($"Cannot delete StorageLocation with ID {id} because others depend on it");
		
		mockStorageLocationsRepository.Verify (l => l.DeleteAsync (id, userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteWithNonExistingIdShouldReturnFalse ()
	{
		var id = Guid.NewGuid();
		
		mockStorageLocationsRepository.Setup (l => l.DeleteAsync (id, userId))
										.ReturnsAsync (false);
		
		var result = await service.DeleteAsync (id);
		
		result.Should().Be (false);
		
		mockStorageLocationsRepository.Verify (l => l.DeleteAsync (id, userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteWithExistingIdShouldReturnTrue ()
	{
		var id = Guid.NewGuid();
		
		mockStorageLocationsRepository.Setup (l => l.DeleteAsync (id, userId))
										.ReturnsAsync (true);
		
		var result = await service.DeleteAsync (id);
		
		result.Should().Be (true);
		
		mockStorageLocationsRepository.Verify (l => l.DeleteAsync (id, userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
}