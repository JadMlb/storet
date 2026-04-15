using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Storet.Modules.Inventory.Data;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Repositories.InventoryMovementItems;
using Storet.Tests.Common.Repository;
using Xunit.Abstractions;

namespace Storet.Inventory.Tests.Repositories;

public class InventoryMovementItemsRepositoryTests : SqliteRepositoryTestsBase<StoretInventoryDbContext, InventoryMovementItemsRepository>
{
	public InventoryMovementItemsRepositoryTests (ITestOutputHelper output) : base (output) {}

	protected override StoretInventoryDbContext InitDbContextWithOptions (DbContextOptions<StoretInventoryDbContext> options)
	{
		return new StoretInventoryDbContext (options);
	}
	
	protected override InventoryMovementItemsRepository InitRepository ()
	{
		return new InventoryMovementItemsRepository (context);
	}
	
	[Fact]
	public async Task BulkInsertWithDuplicateKeyShouldThrowDbConcurrencyException ()
	{
		var userId = Guid.NewGuid();
		var location = new StorageLocation
		{
			Id = Guid.NewGuid(),
			Name = "Cupboard"
		};
		await context.StorageLocations.AddAsync (location);
		var movement = new InventoryMovement
		{
			Id = Guid.NewGuid(),
			Direction = MovementDirection.In,
			UserId = userId,
			NumberOfItems = 1,
			Source = MovementSource.Purchase,
			StorageLocationId = location.Id
		};
		await context.InventoryMovements.AddAsync (movement);
		var items = new InventoryMovementItem
		{
			MovementId = movement.Id,
			UserId = userId,
			ItemId = Guid.NewGuid(),
			Quantity = 1
		};
		await context.InventoryMovementItems.AddAsync (items);
		await context.SaveChangesAsync();
		context.Entry(items).State = EntityState.Detached;
		
		var toBeInserted = new List<InventoryMovementItem>
		{
			new ()
			{
				MovementId = movement.Id,
				UserId = userId,
				ItemId = Guid.NewGuid(),
				Quantity = 2
			}
		};
		
		Func<Task> act = async () => await repository.BulkInsertAsync (toBeInserted);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task BulkInsertWithNonExistingKeyShouldThrowDbConcurrencyException ()
	{
		var userId = Guid.NewGuid();
		var toBeInserted = new List<InventoryMovementItem>
		{
			new ()
			{
				MovementId = Guid.NewGuid(),
				UserId = userId,
				ItemId = Guid.NewGuid(),
				Quantity = 2
			}
		};
		
		Func<Task> act = async () => await repository.BulkInsertAsync (toBeInserted);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task BulkInsertWithNegativeQuantityShouldThrowDbConcurrencyException ()
	{
		var movementId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		var toBeInserted = new List<InventoryMovementItem>
		{
			new ()
			{
				MovementId = movementId,
				UserId = userId,
				ItemId = Guid.NewGuid(),
				Quantity = 0
			}
		};
		
		Func<Task> act = async () => await repository.BulkInsertAsync (toBeInserted);
		
		await act.Should().ThrowAsync<DbUpdateException>();
		
		var notInserted = await context.InventoryMovementItems.AsNoTracking().FirstOrDefaultAsync (i => i.MovementId == movementId);
		notInserted.Should().BeNull();
	}

	[Fact]
	public async Task BulkInsertWithValidDataShouldAddToDatabase ()
	{
		var userId = Guid.NewGuid();
		var location = new StorageLocation
		{
			Id = Guid.NewGuid(),
			Name = "Cupboard"
		};
		await context.StorageLocations.AddAsync (location);
		var movement = new InventoryMovement
		{
			Id = Guid.NewGuid(),
			Direction = MovementDirection.In,
			UserId = userId,
			NumberOfItems = 1,
			Source = MovementSource.Purchase,
			StorageLocationId = location.Id
		};
		await context.InventoryMovements.AddAsync (movement);
		await context.SaveChangesAsync();
		
		var itemId = Guid.NewGuid();
		var toBeInserted = new List<InventoryMovementItem>
		{
			new ()
			{
				MovementId = movement.Id,
				UserId = userId,
				ItemId = itemId,
				Quantity = 1,
				Ordinal = 1
			}
		};
		
		var res = await repository.BulkInsertAsync (toBeInserted);
		res.Should().NotBeNull();
		res.Should().Contain (i => i.ItemId == itemId && i.Quantity == 1);
		
		var inserted = await context.InventoryMovementItems.AsNoTracking().FirstOrDefaultAsync (i => i.MovementId == movement.Id);
		inserted.Should().NotBeNull();
		inserted.ItemId.Should().Be (itemId);
		inserted.Quantity.Should().Be (1);
	}
}