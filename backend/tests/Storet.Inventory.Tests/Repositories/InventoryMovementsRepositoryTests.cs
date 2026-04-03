using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storet.Core.Utils;
using Storet.Modules.Inventory.Data;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Repositories.InventoryMovements;
using Storet.Tests.Common.Repository;
using Xunit.Abstractions;

namespace Storet.Inventory.Tests.Repositories;

public class InventoryMovementsRepositoryTests : SqliteRepositoryTestsBase<StoretInventoryDbContext, InventoryMovementsRepository>
{
	private readonly Guid userId = Guid.NewGuid();
	
	public InventoryMovementsRepositoryTests (ITestOutputHelper output) : base (output) {}

	protected override StoretInventoryDbContext InitDbContextWithOptions (DbContextOptions<StoretInventoryDbContext> options)
	{
		return new StoretInventoryDbContext (options);
	}

	protected override InventoryMovementsRepository InitRepository ()
	{
		return new InventoryMovementsRepository (context);
	}
	
	private async Task SeedContextWithData (DateTimeOffset? referenceTimestamp = null)
	{
		var actualReferenceTimestamp = referenceTimestamp ?? DateTimeOffset.Now;
		
		Guid sugarId = Guid.NewGuid(),
			teaBagId = Guid.NewGuid();
		
		var cupboardId = Guid.NewGuid();
		var cupboard = new StorageLocation
		{
			Id = cupboardId,
			Name = "Cupboard"
		};
		await context.StorageLocations.AddAsync (cupboard);
		
		var logs = new List<InventoryMovement>
		{
			new ()
			{
				ItemId = sugarId,
				UserId = userId,
				Direction = MovementDirection.In,
				Source = MovementSource.Purchase,
				StorageLocationId = cupboardId,
				ExecutedAt = actualReferenceTimestamp.AddDays (-1),
				Quantity = 100
			},
			new ()
			{
				ItemId = teaBagId,
				UserId = userId,
				Direction = MovementDirection.In,
				Source = MovementSource.Purchase,
				StorageLocationId = cupboardId,
				ExecutedAt = actualReferenceTimestamp.AddDays (-1),
				Quantity = 10
			},
			new ()
			{
				ItemId = teaBagId,
				UserId = userId,
				Direction = MovementDirection.Out,
				Source = MovementSource.Usage,
				StorageLocationId = cupboardId,
				ExecutedAt = actualReferenceTimestamp,
				Quantity = 1
			},
			new ()
			{
				ItemId = sugarId,
				UserId = userId,
				Direction = MovementDirection.Out,
				Source = MovementSource.Usage,
				StorageLocationId = cupboardId,
				ExecutedAt = actualReferenceTimestamp,
				Quantity = 2
			},
			new ()
			{
				ItemId = sugarId,
				UserId = userId,
				Direction = MovementDirection.Out,
				Source = MovementSource.Usage,
				StorageLocationId = cupboardId,
				ExecutedAt = actualReferenceTimestamp.AddHours (2),
				Quantity = 1
			}
		};
		await context.AddRangeAsync (logs);
		await context.SaveChangesAsync();
	}
	
	[Fact]
	public async Task GetAllAsyncShouldReturnPaginatedList ()
	{
		await SeedContextWithData();
		
		var query = new Query<DateTimeOffset?>();
		
		var result = await repository.GetAllAsync (query, userId);
		
		result.Should().NotBeEmpty();
		result.Should().HaveCount (5);
		result.Should().AllSatisfy (m => m.UserId.Should().Be (userId));
	}
	
	[Fact]
	public async Task GetAllAsyncWithQueryKeyShouldReturnPaginatedList ()
	{
		var key = DateTimeOffset.Now;
		await SeedContextWithData (key);
		
		var query = new Query<DateTimeOffset?>
		{
			PageSize = 5,
			Key = key
		};
		
		var result = await repository.GetAllAsync (query, userId);
		
		result.Should().NotBeEmpty();
		result.Should().HaveCount (3);
		result.Should().AllSatisfy (m => m.UserId.Should().Be (userId));
		result.Should().AllSatisfy (m => m.ExecutedAt.Should().BeOnOrAfter (key));
	}
	
	[Fact]
	public async Task GetPreviousKeyWithEmptyQueryKeyShouldReturnNull ()
	{
		var query = new Query<DateTimeOffset?>();
		
		var res = await repository.GetPreviousKeyAsync (query, userId);
		
		res.Should().BeNull();
	}
	
	[Fact]
	public async Task GetPreviousKeyWithQueryKeyShouldReturnPreviousKey ()
	{
		var key = DateTimeOffset.Now;
		await SeedContextWithData (key);
		
		var query = new Query<DateTimeOffset?>
		{
			Key = key.AddHours (2),
			PageSize = 2
		};
		
		var res = await repository.GetPreviousKeyAsync (query, userId);
		
		res.Should().BeBefore (key);
		res.Should().Be (key.AddDays (-1));
	}
	
	[Fact]
	public async Task BulkInsertWithValidDataShouldReturnNumberOfRowsInserted ()
	{
		var cupboardId = Guid.NewGuid();
		var cupboard = new StorageLocation
		{
			Id = cupboardId,
			Name = "Cupboard"
		};
		await context.StorageLocations.AddAsync (cupboard);
		await context.SaveChangesAsync();
		
		var logs = new List<InventoryMovement>
		{
			new ()
			{
				ItemId = Guid.NewGuid(),
				UserId = userId,
				Direction = MovementDirection.In,
				Source = MovementSource.Purchase,
				StorageLocationId = cupboardId,
				Quantity = 100
			},
			new ()
			{
				ItemId = Guid.NewGuid(),
				UserId = userId,
				Direction = MovementDirection.In,
				Source = MovementSource.Purchase,
				StorageLocationId = cupboardId,
				Quantity = 10
			},
		};
		
		var res = await repository.BulkInsertAsync (logs);
		
		res.Should().Be (2);
		
		var inserted = await context.InventoryMovements
									.AsNoTracking()
									.Where (m => m.UserId == userId)
									.ToListAsync();
		inserted.Should().HaveCount (2);
		inserted.Should().Contain (m => m.UserId == userId && m.Direction == MovementDirection.In && m.Source == MovementSource.Purchase && m.StorageLocationId == cupboardId && m.Quantity == 100);
		inserted.Should().Contain (m => m.UserId == userId && m.Direction == MovementDirection.In && m.Source == MovementSource.Purchase && m.StorageLocationId == cupboardId && m.Quantity == 10);
		inserted.Should().AllSatisfy (m => m.ExecutedAt.Should().BeCloseTo (DateTimeOffset.Now, TimeSpan.FromSeconds (1)));
	}
	
	[Fact]
	public async Task BulkInsertWithMissingStorageLocationShouldThrowDbUpdatExceptionAndNotAddToDatabase ()
	{
		var cupboardId = Guid.NewGuid();
		var cupboard = new StorageLocation
		{
			Id = cupboardId,
			Name = "Cupboard"
		};
		await context.StorageLocations.AddAsync (cupboard);
		await context.SaveChangesAsync();
		
		var logs = new List<InventoryMovement>
		{
			new ()
			{
				ItemId = Guid.NewGuid(),
				UserId = userId,
				Direction = MovementDirection.In,
				Source = MovementSource.Purchase,
				StorageLocationId = cupboardId,
				Quantity = 100
			},
			new ()
			{
				ItemId = Guid.NewGuid(),
				UserId = userId,
				Direction = MovementDirection.In,
				Source = MovementSource.Purchase,
				StorageLocationId = Guid.NewGuid(),
				Quantity = 10
			},
		};
		
		Func<Task> act = async () => await repository.BulkInsertAsync (logs);
		
		await act.Should().ThrowAsync<DbUpdateException>();
		
		var notInserted = await context.InventoryMovements
										.AsNoTracking()
										.Where (m => m.UserId == userId)
										.ToListAsync();
		notInserted.Should().BeEmpty();
	}
}