using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storet.Modules.Inventory.Data;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Repositories.Inventory;
using Storet.Tests.Common.Repository;
using Xunit.Abstractions;

namespace Storet.Inventory.Tests.Repositories;

public class InventoryRepositoryTests : SqliteRepositoryTestsBase<StoretInventoryDbContext, InventoryRepository>
{
	public InventoryRepositoryTests (ITestOutputHelper output) : base (output) {}

	protected override StoretInventoryDbContext InitDbContextWithOptions (DbContextOptions<StoretInventoryDbContext> options)
	{
		return new StoretInventoryDbContext (options);
	}
	
	protected override InventoryRepository InitRepository ()
	{
		return new InventoryRepository (context);
	}
	
	[Fact]
	public async Task CreateInventoryLogShouldAddToDatabase ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		var inventory = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			QuantityInStock = 1,
			Status = Status.Sufficient
		};
		
		var result = await repository.InsertAsync (inventory);
		
		result.Should().NotBeNull();
		result.ItemId.Should().Be (itemId);
		result.UserId.Should().Be (userId);
		result.MinQuantity.Should().Be (0);
		result.MaxQuantity.Should().BeNull();
		result.QuantityInStock.Should().Be (1);
		result.Status.Should().Be (Status.Sufficient);
		
		var inDb = await context.Inventories
								.AsNoTracking()
								.FirstOrDefaultAsync (i => i.ItemId == itemId && i.UserId == userId);
		inDb.Should().NotBeNull();
	}
	
	[Fact]
	public async Task CreateInventoryLogWithDuplicateKeyShouldThrowDbUpdateException ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		var inventory = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			QuantityInStock = 1,
			Status = Status.Sufficient
		};
		await context.Inventories.AddAsync (inventory);
		await context.SaveChangesAsync();
		
		var inventoryDuplicateKey = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			QuantityInStock = 0,
			Status = Status.EmptyNotAccepted
		};
		
		Func<Task> act = async () => await repository.InsertAsync (inventoryDuplicateKey);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task CreateInventoryWithNegativeMinimumQuantityShouldThrowDbUpdateException ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var toBeInserted = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			Status = Status.Sufficient,
			QuantityInStock = 10,
			MinQuantity = -1
		};
		
		Func<Task> act = async () => await repository.InsertAsync (toBeInserted);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task CreateInventoryWithZeroMaxQuantityShouldThrowDbUpdateException ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var toBeInserted = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			Status = Status.Sufficient,
			QuantityInStock = 10,
			MaxQuantity = 0
		};
		
		Func<Task> act = async () => await repository.InsertAsync (toBeInserted);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task GetAllShouldReturnList ()
	{
		var userId = Guid.NewGuid();
		var inventories = new List<Modules.Inventory.Models.Inventory>
		{
			new () {UserId = userId, ItemId = Guid.NewGuid(), MaxQuantity = 10, MinQuantity = 1, QuantityInStock = 10, Status = Status.Full},
			new () {UserId = userId, ItemId = Guid.NewGuid(), QuantityInStock = 0, Status = Status.EmptyAccepted},
			new () {UserId = userId, ItemId = Guid.NewGuid(), QuantityInStock = 1, Status = Status.Sufficient},
			new () {UserId = userId, ItemId = Guid.NewGuid(), MinQuantity = 3, QuantityInStock = 3, Status = Status.Critical},
			new () {UserId = Guid.NewGuid(), ItemId = Guid.NewGuid(), MinQuantity = 3, QuantityInStock = 3, Status = Status.Critical},
		};
		
		await context.Inventories.AddRangeAsync (inventories);
		await context.SaveChangesAsync();
		
		var result = await repository.GetAllAsync (userId);
		result.Should().HaveCount (4);
		result.Should().AllSatisfy (i => i.UserId.Should().Be (userId));
	}
	
	[Fact]
	public async Task GetAllWithNoInventoriesForUserShouldReturnEmptyList ()
	{
		var userId = Guid.NewGuid();
		var result = await repository.GetAllAsync (userId);
		result.Should().BeEmpty();
	}
	
	[Fact]
	public async Task UpdateInventoryWithNonExistingIdShouldReturnNull ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var updatedValues = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			Status = Status.Critical,
			QuantityInStock = 0
		};
		
		var result = await repository.UpdateAsync (itemId, userId, updatedValues);
		
		result.Should().BeNull();
	}
	
	[Fact]
	public async Task UpdateInventoryWithValidDataShouldReturnInventory ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var originalValues = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			Status = Status.Sufficient,
			QuantityInStock = 10
		};
		
		await context.Inventories.AddAsync (originalValues);
		await context.SaveChangesAsync();
		
		var updatedValues = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			Status = Status.EmptyAccepted,
			QuantityInStock = 0
		};
		
		var result = await repository.UpdateAsync (itemId, userId, updatedValues);
		
		result.Should().NotBeNull();
		result.ItemId.Should().Be (itemId);
		result.UserId.Should().Be (userId);
		result.Status.Should().Be (Status.EmptyAccepted);
		result.QuantityInStock.Should().Be (0);
	}
	
	[Fact]
	public async Task UpdateInventoryWithNegativeMinimumQuantityShouldThrowDbUpdateException ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var originalValues = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			Status = Status.Sufficient,
			QuantityInStock = 10
		};
		
		await context.Inventories.AddAsync (originalValues);
		await context.SaveChangesAsync();
		
		var updatedValues = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			MinQuantity = -1
		};
		
		Func<Task> act = async () => await repository.UpdateAsync (itemId, userId, updatedValues);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task UpdateInventoryWithZeroMaxQuantityShouldThrowDbUpdateException ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var originalValues = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			Status = Status.Sufficient,
			QuantityInStock = 10
		};
		
		await context.Inventories.AddAsync (originalValues);
		await context.SaveChangesAsync();
		
		var updatedValues = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			MaxQuantity = 0
		};
		
		Func<Task> act = async () => await repository.UpdateAsync (itemId, userId, updatedValues);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task DeleteInventoryWithNonExistingUserAndItemIdShouldReturnFalse ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var result = await repository.DeleteAsync (itemId, userId);
		result.Should().BeFalse();
	}
	
	[Fact]
	public async Task DeleteInventoryWithExistingUserAndItemIdShouldReturnTrue ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var originalValues = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			Status = Status.Sufficient,
			QuantityInStock = 10
		};
		
		await context.Inventories.AddAsync (originalValues);
		await context.SaveChangesAsync();
		
		var result = await repository.DeleteAsync (itemId, userId);
		result.Should().BeTrue();
		
		var deleted = await context.Inventories.AsNoTracking().FirstOrDefaultAsync (i => i.ItemId == itemId && i.UserId == userId);
		deleted.Should().BeNull();
	}
	
	[Fact]
	public async Task CheckIfInventoryExistsWithNonExistingUserAndItemIdShouldReturnFalse ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var result = await repository.ExistsAsync (itemId, userId);
		result.Should().BeFalse();
	}
	
	[Fact]
	public async Task CheckIfInventoryExistsWithExistingUserAndItemIdShouldReturnTrue ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		
		var originalValues = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			Status = Status.Sufficient,
			QuantityInStock = 10
		};
		
		await context.Inventories.AddAsync (originalValues);
		await context.SaveChangesAsync();
		
		var result = await repository.ExistsAsync (itemId, userId);
		result.Should().BeTrue();
		
		var notDeleted = await context.Inventories.AsNoTracking().FirstOrDefaultAsync (i => i.ItemId == itemId && i.UserId == userId);
		notDeleted.Should().NotBeNull();
	}
}