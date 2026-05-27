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
				UserId = userId,
				Direction = MovementDirection.In,
				Source = MovementSource.Purchase,
				StorageLocationId = cupboardId,
				ExecutedAt = actualReferenceTimestamp.AddDays (-1),
				NumberOfItems = 1
			},
			new ()
			{
				UserId = userId,
				Direction = MovementDirection.In,
				Source = MovementSource.Purchase,
				StorageLocationId = cupboardId,
				ExecutedAt = actualReferenceTimestamp.AddDays(-1).AddHours (1),
				NumberOfItems = 1
			},
			new ()
			{
				UserId = userId,
				Direction = MovementDirection.Out,
				Source = MovementSource.Usage,
				StorageLocationId = cupboardId,
				ExecutedAt = actualReferenceTimestamp,
				NumberOfItems = 1
			},
			new ()
			{
				UserId = userId,
				Direction = MovementDirection.Out,
				Source = MovementSource.Usage,
				StorageLocationId = cupboardId,
				ExecutedAt = actualReferenceTimestamp,
				NumberOfItems = 1
			},
			new ()
			{
				UserId = userId,
				Direction = MovementDirection.Out,
				Source = MovementSource.Usage,
				StorageLocationId = cupboardId,
				ExecutedAt = actualReferenceTimestamp.AddHours (2),
				NumberOfItems = 1
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
		result.Should().HaveCount (4);
		result.Should().AllSatisfy (m => m.UserId.Should().Be (userId));
		result.Should().AllSatisfy (m => m.ExecutedAt.Should().BeOnOrBefore (key));
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
		
		res.Should().BeNull();
	}
	
	[Fact]
	public async Task BulkInsertWithValidDataShouldReturnInsertedRow ()
	{
		var cupboardId = Guid.NewGuid();
		var cupboard = new StorageLocation
		{
			Id = cupboardId,
			Name = "Cupboard"
		};
		await context.StorageLocations.AddAsync (cupboard);
		await context.SaveChangesAsync();
		
		var log = new InventoryMovement
		{
			UserId = userId,
			Direction = MovementDirection.In,
			Source = MovementSource.Purchase,
			StorageLocationId = cupboardId,
			NumberOfItems = 2
		};
		
		var res = await repository.InsertAsync (log);
		
		res.Should().NotBeNull();
		res.Direction.Should().Be (MovementDirection.In);
		res.ExecutedAt.Should().BeCloseTo (DateTimeOffset.Now, TimeSpan.FromSeconds (1));
		res.NumberOfItems.Should().Be (2);
		res.StorageLocationId.Should().Be (cupboardId);
		res.UserId.Should().Be (userId);
		
		var inserted = await context.InventoryMovements
									.AsNoTracking()
									.Where (m => m.UserId == userId)
									.ToListAsync();
		inserted.Should().HaveCount (1);
		inserted.Should().Contain (m => m.UserId == userId && m.Direction == MovementDirection.In && m.Source == MovementSource.Purchase && m.StorageLocationId == cupboardId && m.NumberOfItems == 2);
		inserted.Should().AllSatisfy (m => m.ExecutedAt.Should().BeCloseTo (DateTimeOffset.Now, TimeSpan.FromSeconds (1)));
	}
	
	[Fact]
	public async Task BulkInsertWithMissingStorageLocationShouldThrowDbUpdatExceptionAndNotAddToDatabase ()
	{
		var cupboardId = Guid.NewGuid();
		var log = new InventoryMovement
		{
			UserId = userId,
			Direction = MovementDirection.In,
			Source = MovementSource.Purchase,
			StorageLocationId = cupboardId,
			NumberOfItems = 2
		};
		
		Func<Task> act = async () => await repository.InsertAsync (log);
		
		await act.Should().ThrowAsync<DbUpdateException>();
		
		var notInserted = await context.InventoryMovements
										.AsNoTracking()
										.Where (m => m.UserId == userId)
										.ToListAsync();
		notInserted.Should().BeEmpty();
	}
}