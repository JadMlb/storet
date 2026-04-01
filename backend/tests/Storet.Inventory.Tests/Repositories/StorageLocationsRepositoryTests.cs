using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storet.Modules.Inventory.Data;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Repositories.StorageLocations;
using Storet.Tests.Common.Repository;
using Xunit.Abstractions;

namespace Storet.Inventory.Tests.Repositories;

public class StorageLocationsRepositoryTests : SqliteRepositoryTestsBase<StoretInventoryDbContext, StorageLocationsRepository>
{
	public StorageLocationsRepositoryTests (ITestOutputHelper output) : base (output) {}

	protected override StoretInventoryDbContext InitDbContextWithOptions (DbContextOptions<StoretInventoryDbContext> options)
	{
		return new StoretInventoryDbContext (options);
	}
	
	protected override StorageLocationsRepository InitRepository ()
	{
		return new StorageLocationsRepository (context);
	}
	
	protected override void AfterDbCreated ()
	{
		using var cmd = connection.CreateCommand();
		cmd.CommandText = "drop index if exists idx_uniq_storage_location_names;";
		cmd.ExecuteNonQuery();
		
		cmd.CommandText = "create unique index idx_uniq_storage_location_names on storage_locations (name collate nocase, user_id);";
		cmd.ExecuteNonQuery();
	}
	
	[Fact]
	public async Task CreateStorageLocationShouldAddToDatabase ()
	{
		var userId = Guid.NewGuid();
		var location = new StorageLocation
		{
			UserId = userId,
			Name = "Cupboard 1"
		};
		
		var result = await repository.InsertAsync (location);
		
		result.Should().NotBeNull();
		result.UserId.Should().Be (userId);
		result.Name.Should().Be ("Cupboard 1");
		result.Description.Should().BeNull();
		
		var inDb = await context.StorageLocations
								.AsNoTracking()
								.FirstOrDefaultAsync (i => i.Id == result.Id && i.UserId == userId);
		inDb.Should().NotBeNull();
	}
	
	[Fact]
	public async Task CreateLocationWithDuplicateNameShouldThrowDbUpdateException ()
	{
		var userId = Guid.NewGuid();
		var location = new StorageLocation
		{
			UserId = userId,
			Name = "Cupboard 1"
		};
		await context.StorageLocations.AddAsync (location);
		await context.SaveChangesAsync();
		
		var locationDuplicateName = new StorageLocation
		{
			UserId = userId,
			Name = "cupboard 1",
			Description = "Testing duplicate names"
		};
		
		Func<Task> act = async () => await repository.InsertAsync (locationDuplicateName);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task GetAllShouldReturnList ()
	{
		var userId = Guid.NewGuid();
		var locations = new List<StorageLocation>
		{
			new () {UserId = userId, Name = "Cupboard above sink"},
			new () {UserId = userId, Name = "Drawer"},
			new () {UserId = Guid.NewGuid(), Name = "Cupboard Above Sink"},
		};
		
		await context.StorageLocations.AddRangeAsync (locations);
		await context.SaveChangesAsync();
		
		var result = await repository.GetAllAsync (userId);
		result.Should().HaveCount (2);
		result.Should().AllSatisfy (i => i.UserId.Should().Be (userId));
	}
	
	[Fact]
	public async Task GetAllWithNoLocationsForUserShouldReturnEmptyList ()
	{
		var userId = Guid.NewGuid();
		var result = await repository.GetAllAsync (userId);
		result.Should().BeEmpty();
	}
	
	[Fact]
	public async Task UpdateLocationWithNonExistingIdShouldReturnNull ()
	{
		var userId = Guid.NewGuid();
		var id = Guid.NewGuid();
		
		var updatedValues = new StorageLocation
		{
			Id = id,
			UserId = userId,
			Name = "Drawer",
			Description = "Drawer next to the bed"
		};
		
		var result = await repository.UpdateAsync (id, userId, updatedValues);
		
		result.Should().BeNull();
	}
	
	[Fact]
	public async Task UpdateInventoryWithValidDataShouldReturnInventory ()
	{
		var userId = Guid.NewGuid();
		var id = Guid.NewGuid();
		
		var originalValues = new StorageLocation
		{
			Id = id,
			UserId = userId,
			Name = "Drawer",
		};
		
		await context.StorageLocations.AddAsync (originalValues);
		await context.SaveChangesAsync();
		
		var updatedValues = new StorageLocation
		{
			Id = id,
			UserId = userId,
			Name = "Drawerr",
			Description = "Drawer next to the bed"
		};
		
		var result = await repository.UpdateAsync (id, userId, updatedValues);
		
		result.Should().NotBeNull();
		result.Id.Should().Be (id);
		result.UserId.Should().Be (userId);
		result.Name.Should().Be ("Drawerr");
		result.Description.Should().Be ("Drawer next to the bed");
		
		var inDb = await context.StorageLocations.AsNoTracking().FirstOrDefaultAsync (s => s.Id == id && s.UserId == userId);
		
		inDb.Should().NotBeNull();
		inDb.Id.Should().Be (id);
		inDb.UserId.Should().Be (userId);
		inDb.Name.Should().Be ("Drawerr");
		inDb.Description.Should().Be ("Drawer next to the bed");
	}
	
	[Fact]
	public async Task UpdateLocationWithExistingNameShouldThrowDbUpdateException ()
	{
		var userId = Guid.NewGuid();
		var id = Guid.NewGuid();
		
		var originalValues = new StorageLocation
		{
			Id = id,
			UserId = userId,
			Name = "Drawer",
		};
		
		var otherLocation = new StorageLocation
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Name = "Drawerr",
		};
		
		await context.StorageLocations.AddRangeAsync (originalValues, otherLocation);
		await context.SaveChangesAsync();
		
		var updatedValues = new StorageLocation
		{
			Id = id,
			UserId = userId,
			Name = "Drawerr",
			Description = "Drawer next to the bed"
		};
		
		Func<Task> act = async () => await repository.UpdateAsync (id, userId, updatedValues);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task DeleteLocationWithNonExistingUserAndIdShouldReturnFalse ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var result = await repository.DeleteAsync (itemId, userId);
		result.Should().BeFalse();
	}
	
	[Fact]
	public async Task DeleteLocationWithExistingUserAndIdShouldReturnTrue ()
	{
		var userId = Guid.NewGuid();
		var id = Guid.NewGuid();
		
		var originalValues = new StorageLocation
		{
			Id = id,
			UserId = userId,
			Name = "Drawer",
		};
		
		await context.StorageLocations.AddAsync (originalValues);
		await context.SaveChangesAsync();
		
		var result = await repository.DeleteAsync (id, userId);
		result.Should().BeTrue();
		
		var deleted = await context.StorageLocations.AsNoTracking().FirstOrDefaultAsync (i => i.Id == id && i.UserId == userId);
		deleted.Should().BeNull();
	}
	
	[Fact]
	public async Task CheckIfLocationExistsWithNonExistingUserAndIdShouldReturnFalse ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var result = await repository.ExistsAsync (itemId, userId);
		result.Should().BeFalse();
	}
	
	[Fact]
	public async Task CheckIfLocationExistsWithExistingUserAndIdShouldReturnTrue ()
	{
		var userId = Guid.NewGuid();
		var id = Guid.NewGuid();
		
		var originalValues = new StorageLocation
		{
			Id = id,
			UserId = userId,
			Name = "Drawer",
		};
		
		await context.StorageLocations.AddAsync (originalValues);
		await context.SaveChangesAsync();
		
		var result = await repository.ExistsAsync (id, userId);
		result.Should().BeTrue();
		
		var notDeleted = await context.StorageLocations.AsNoTracking().FirstOrDefaultAsync (i => i.Id == id && i.UserId == userId);
		notDeleted.Should().NotBeNull();
	}
}