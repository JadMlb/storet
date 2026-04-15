using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Storet.Core.Authorization;
using Storet.Core.Exceptions;
using Storet.Core.Mappers;
using Storet.Core.Utils;
using Storet.Modules.Inventory.Contracts.InventoryMovement;
using Storet.Modules.Inventory.Mappers;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Repositories.InventoryMovementItems;
using Storet.Modules.Inventory.Repositories.InventoryMovements;
using Storet.Modules.Inventory.Repositories.StorageLocations;
using Storet.Modules.Inventory.Services.Inventory;
using Storet.Modules.Inventory.Services.InventoryMovements;
using Storet.Modules.ItemsCatalogue.Contracts.Items;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Modules.ItemsCatalogue.Services.Items;

namespace Storet.Inventory.Tests.Services;

public class InventoryMovementsServiceTests
{
	private readonly IMapper mapper;
	private readonly ILoggerFactory loggerFactory;
	private readonly Mock<ICurrentUser> mockCurrentUser;
	private readonly Mock<IInventoryMovementsRepository> mockInventoryMovementsRepository;
	private readonly Mock<IInventoryService> mockInventoryService;
	private readonly Mock<IInventoryMovementItemsRepository> mockInventoryMovementItemsRepository;
	private readonly Mock<IStorageLocationsRepository> mockStorageLocationsRepository;
	private readonly Mock<IItemsService> mockItemsService;
	private readonly Guid userId = Guid.NewGuid();
	private readonly InventoryMovementsService service;
	
	public InventoryMovementsServiceTests ()
	{
		mockCurrentUser = new Mock<ICurrentUser>();
		mockInventoryMovementsRepository = new Mock<IInventoryMovementsRepository>();
		mockInventoryService = new Mock<IInventoryService>();
		mockInventoryMovementItemsRepository = new Mock<IInventoryMovementItemsRepository>();
		mockStorageLocationsRepository = new Mock<IStorageLocationsRepository>();
		mockItemsService = new Mock<IItemsService>();
		loggerFactory = new LoggerFactory();
		
		var inventoryModificationRequestUserResolver = new CurrentUserResolver<InventoryModificationRequest, InventoryMovement> (mockCurrentUser.Object);
		var inventoryMovementNumberOfItemsValueResolver = new InventoryMovementNumberOfItemsValueResolver();
		var inventoryMovementSourceValueResolver = new InventoryMovementSourceValueResolver();
		var config = new MapperConfiguration (
			cfg =>
			{
				cfg.ConstructServicesUsing (
					type => type == typeof (CurrentUserResolver<InventoryModificationRequest, InventoryMovement>) ? inventoryModificationRequestUserResolver :
								type == typeof (InventoryMovementNumberOfItemsValueResolver) ? inventoryMovementNumberOfItemsValueResolver :
								type == typeof (InventoryMovementSourceValueResolver) ? inventoryMovementSourceValueResolver :
								null
				);
				cfg.AddProfile<MappingProfile>();
			},
			loggerFactory
		);
		mapper = config.CreateMapper();
		
		service = new InventoryMovementsService (mockInventoryMovementsRepository.Object, mockInventoryMovementItemsRepository.Object, mockStorageLocationsRepository.Object, mockInventoryService.Object, mockItemsService.Object, mockCurrentUser.Object, mapper);
		
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
		mockInventoryService.VerifyNoOtherCalls();
		mockInventoryMovementsRepository.VerifyNoOtherCalls();
		mockInventoryMovementItemsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllWithInvalidQueryShouldThrowArgumentException ()
	{
		var query = new Query<DateTimeOffset?>
		{
			PageSize = 0
		};
		
		Func<Task> act = async () => await service.GetAllAsync (query);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Invalid page size for query");
		
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllWithNoMatchShouldReturnEmptyList ()
	{
		var query = new Query<DateTimeOffset?>();
		mockInventoryMovementsRepository.Setup (m => m.GetAllAsync (query, userId))
										.ReturnsAsync ([]);
		mockInventoryMovementsRepository.Setup (m => m.GetPreviousKeyAsync (query, userId))
										.ReturnsAsync ((DateTimeOffset?) null);
		
		var result = await service.GetAllAsync (query);
		result.Data.Should().BeEmpty();
		result.HasNext.Should().BeFalse();
		result.HasPrevious.Should().BeFalse();
		result.Next.Should().BeNull();
		result.Previous.Should().BeNull();
		
		mockInventoryMovementsRepository.Verify (m => m.GetAllAsync (query, userId), Times.Once());
		mockInventoryMovementsRepository.Verify (m => m.GetPreviousKeyAsync (query, userId), Times.Once());
		VerifyUserAccessedNTimes (2);
		VerifyNoOtherCalls();
	}
	
	private void SeedRepository (DateTimeOffset referenceTimestamp)
	{
		var location = new StorageLocation
		{
			Id = Guid.NewGuid(),
			Name = "Fridge",
			UserId = userId
		};

		List<InventoryMovement> logs = [
			new ()
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				ExecutedAt = referenceTimestamp,
				Direction = MovementDirection.Out,
				NumberOfItems = 2,
				Source = MovementSource.Usage,
				StorageLocation = location,
				StorageLocationId = location.Id
			},
			new ()
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				ExecutedAt = referenceTimestamp.AddHours (-1),
				Direction = MovementDirection.In,
				NumberOfItems = 2,
				Source = MovementSource.Purchase,
				StorageLocation = location,
				StorageLocationId = location.Id
			},
			new ()
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				ExecutedAt = referenceTimestamp.AddHours(-1).AddSeconds (-1),
				Direction = MovementDirection.In,
				NumberOfItems = 1,
				Source = MovementSource.Purchase,
				StorageLocation = location,
				StorageLocationId = location.Id
			}
		];
		
		mockInventoryMovementsRepository.Setup (m => m.GetAllAsync (It.IsAny<Query<DateTimeOffset?>>(), userId))
										.ReturnsAsync (
											(Query<DateTimeOffset?> query, Guid userId) =>
											{
												var filtered = logs.AsEnumerable();
												if (query.Key != null)
													filtered = filtered.Where (l => l.ExecutedAt <= query.Key);
												return filtered.Take (query.PageSize + 1).ToList();
											}
										);
		mockInventoryMovementsRepository.Setup (m => m.GetPreviousKeyAsync (It.IsAny<Query<DateTimeOffset?>>(), userId))
										.ReturnsAsync (
											(Query<DateTimeOffset?> query, Guid userId) =>
												logs.Where (m => m.ExecutedAt > query.Key)
													.OrderBy (m => m.ExecutedAt)
													.Take (query.PageSize + 1)
													.FirstOrDefault()?.ExecutedAt
										);
	}
	
	[Fact]
	public async Task GetAllWithDataShouldReturnFilledList ()
	{
		var timestamp = DateTimeOffset.Now;
		SeedRepository (timestamp);
		
		var query = new Query<DateTimeOffset?>
		{
			PageSize = 1
		};
		
		var result = await service.GetAllAsync (query);
		result.Data.Should().HaveCount (1);
		result.HasNext.Should().BeTrue();
		result.HasPrevious.Should().BeFalse();
		result.Next.Should().NotBeNull();
		result.Next.Should().Be (timestamp.AddHours (-1));
		result.Previous.Should().BeNull();
		
		mockInventoryMovementsRepository.Verify (m => m.GetAllAsync (query, userId), Times.Once());
		mockInventoryMovementsRepository.Verify (m => m.GetPreviousKeyAsync (query, userId), Times.Once());
		VerifyUserAccessedNTimes (2);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllWithDataOnSecondPageShouldReturnFilledListWithPreviousAndNext ()
	{
		var timestamp = DateTimeOffset.Now;
		SeedRepository (timestamp);
		
		var query = new Query<DateTimeOffset?>
		{
			PageSize = 1,
			Key = timestamp.AddHours (-1)
		};
		
		var result = await service.GetAllAsync (query);
		result.Data.Should().HaveCount (1);
		result.HasNext.Should().BeTrue();
		result.HasPrevious.Should().BeTrue();
		result.Next.Should().NotBeNull();
		result.Next.Should().Be (timestamp.AddHours(-1).AddSeconds (-1));
		result.Previous.Should().NotBeNull();
		result.Previous.Should().Be (timestamp);
		
		mockInventoryMovementsRepository.Verify (m => m.GetAllAsync (query, userId), Times.Once());
		mockInventoryMovementsRepository.Verify (m => m.GetPreviousKeyAsync (query, userId), Times.Once());
		VerifyUserAccessedNTimes (2);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetOneWithNonExistingIdShouldReturnNull ()
	{
		var id = Guid.NewGuid();
		
		mockInventoryMovementsRepository.Setup (m => m.GetOneAsync (id, userId))
										.ReturnsAsync ((InventoryMovement?) null);
		
		var result = await service.GetOneAsync (id);
		result.Should().BeNull();
		
		mockInventoryMovementsRepository.Verify (m => m.GetOneAsync (id, userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetOneWithExistingIdShouldReturnObjectWithItems ()
	{
		var id = Guid.NewGuid();
		var location = new StorageLocation
		{
			Id = Guid.NewGuid(),
			Name = "Fridge",
			UserId = userId
		};
		Guid beefId = Guid.NewGuid(), spicesId = Guid.NewGuid();
		
		mockInventoryMovementsRepository.Setup (m => m.GetOneAsync (id, userId))
										.ReturnsAsync (
											new InventoryMovement ()
											{
												Id = id,
												UserId = userId,
												ExecutedAt = DateTimeOffset.Now,
												Direction = MovementDirection.Out,
												NumberOfItems = 2,
												Source = MovementSource.Usage,
												StorageLocation = location,
												StorageLocationId = location.Id,
												Items = [
													new ()
													{
														ItemId = beefId,
														MovementId = id,
														Quantity = 0.5f,
														UserId = userId
													},
													new ()
													{
														ItemId = spicesId,
														MovementId = id,
														Quantity = 0.01f,
														UserId = userId
													}
												]
											}
										);
		mockItemsService.Setup (i => i.GetAllFromListWithUnitAsync (It.IsAny<IEnumerable<Guid>>()))
						.ReturnsAsync (
							new Dictionary<Guid, ItemResponseWithUnit>
							{
								{beefId, new () {Id = beefId, Name = "Beef", Quantity = 0.5f, Unit = Unit.Kilogramme}},
								{spicesId, new () {Id = spicesId, Name = "Spices", Quantity = 0.2f, Unit = Unit.Kilogramme}}
							}
						);
		
		var result = await service.GetOneAsync (id);
		result.Should().NotBeNull();
		result.Location.Should().NotBeNull();
		result.Location.Id.Should().Be (location.Id);
		result.Direction.Should().Be (MovementDirection.Out);
		result.ExecutedAt.Should().BeCloseTo (DateTimeOffset.Now, TimeSpan.FromSeconds (1));
		result.NumberOfItems.Should().Be (2);
		result.Items.Should().HaveCount (2);
		result.Items.Should().Contain (i => i.Item.Id == beefId);
		result.Items.Should().Contain (i => i.Item.Id == spicesId);
		
		mockInventoryMovementsRepository.Verify (m => m.GetOneAsync (id, userId), Times.Once());
		mockItemsService.Verify (
			i => i.GetAllFromListWithUnitAsync (
				It.Is<IEnumerable<Guid>> (
					i => i.Contains (beefId) && i.Contains (spicesId)
				)
			),
			Times.Once()
		);
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithFutureTimestampShouldThrowArgumentException ()
	{
		var dto = new InventoryModificationRequest
		{
			DestinationLocationId = Guid.NewGuid(),
			ExecutedAt = DateTimeOffset.Now.AddHours (1),
			Source = MovementSource.Purchase,
			Items = {
				{Guid.NewGuid(), 1}
			}
		};
		
		Func<Task> act = async () => await service.InsertAsync (dto);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("This transaction is set to a future date or time");
		
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithMissingSourceAndDestinationShouldThrowArgumentException ()
	{
		var dto = new InventoryModificationRequest
		{
			Source = MovementSource.Purchase,
			Items = {
				{Guid.NewGuid(), 1}
			}
		};
		
		Func<Task> act = async () => await service.InsertAsync (dto);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Neither source nor destination were specified");
		
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithNonExistingStorageLocationIdShouldThrowNotFoundException ()
	{
		var locationId = Guid.NewGuid();
		var dto = new InventoryModificationRequest
		{
			DestinationLocationId = locationId,
			Source = MovementSource.Purchase,
			Items = {
				{Guid.NewGuid(), 1}
			}
		};
		mockStorageLocationsRepository.Setup (l => l.ExistsAsync (locationId, userId))
										.ReturnsAsync (false);
		
		Func<Task> act = async () => await service.InsertAsync (dto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"StorageLocation with ID {locationId} was not found");
		
		mockStorageLocationsRepository.Verify (l => l.ExistsAsync (locationId, userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithNonExistingItemIdShouldThrowNotFoundException ()
	{
		var locationId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		var dto = new InventoryModificationRequest
		{
			DestinationLocationId = locationId,
			Source = MovementSource.Purchase,
			Items = {
				{itemId, 1}
			}
		};
		mockStorageLocationsRepository.Setup (l => l.ExistsAsync (locationId, userId))
										.ReturnsAsync (true);
		mockItemsService.Setup (i => i.GetAllFromListWithUnitAsync (It.IsAny<IEnumerable<Guid>>()))
						.ReturnsAsync ([]);
		
		Func<Task> act = async () => await service.InsertAsync (dto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Item with ID {itemId} was not found");
		
		mockStorageLocationsRepository.Verify (l => l.ExistsAsync (locationId, userId), Times.Once());
		mockItemsService.Verify (i => i.GetAllFromListWithUnitAsync (It.Is<IEnumerable<Guid>> (ids => ids.Contains (itemId))), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithExistingItemIdShouldReturnInserted ()
	{
		var movementId = Guid.NewGuid();
		var locationId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		var dto = new InventoryModificationRequest
		{
			DestinationLocationId = locationId,
			Source = MovementSource.Purchase,
			Items = {
				{itemId, 0.5f}
			}
		};
		mockStorageLocationsRepository.Setup (l => l.ExistsAsync (locationId, userId))
										.ReturnsAsync (true);
		mockItemsService.Setup (i => i.GetAllFromListWithUnitAsync (It.IsAny<IEnumerable<Guid>>()))
						.ReturnsAsync (
							new Dictionary<Guid, ItemResponseWithUnit>
							{
								{
									itemId,
									new ItemResponseWithUnit ()
									{
										Id = itemId,
										Name = "Beef",
										Quantity = 0.5f,
										Unit = Unit.Kilogramme
									}
								}
							}
						);
		mockInventoryMovementsRepository.Setup (m => m.InsertAsync (It.IsAny<InventoryMovement>()))
										.ReturnsAsync (
											(InventoryMovement m) =>
											{
												m.Id = movementId;
												m.StorageLocation = new StorageLocation
												{
													Id = locationId,
													Name = "Fridge",
													UserId = userId
												};
												return m;
											}
										);
		mockInventoryMovementItemsRepository.Setup (m => m.BulkInsertAsync (It.IsAny<IEnumerable<InventoryMovementItem>>()))
											.ReturnsAsync ([
												new InventoryMovementItem
												{
													ItemId = itemId,
													MovementId = movementId,
													Ordinal = 1,
													Quantity = 0.5f,
													UserId = userId
												}
											]);
		mockInventoryService.Setup (i => i.UpdateInventoryQuantitiesAsync (It.IsAny<Dictionary<Guid, float>>()))
							.ReturnsAsync (true);
		
		var res = await service.InsertAsync (dto);
		res.Should().NotBeNull();
		res.Should().HaveCount (1);
		res.First().Direction.Should().Be (MovementDirection.In);
		res.First().Location.Should().NotBeNull();
		res.First().NumberOfItems.Should().Be (1);
		res.First().Items.Should().HaveCount (1);
		
		mockStorageLocationsRepository.Verify (l => l.ExistsAsync (locationId, userId), Times.Once());
		mockItemsService.Verify (i => i.GetAllFromListWithUnitAsync (It.Is<IEnumerable<Guid>> (ids => ids.Contains (itemId))), Times.Once());
		mockInventoryMovementsRepository.Verify (m => m.InsertAsync (It.Is<InventoryMovement> (i => i.NumberOfItems == 1)), Times.Once());
		mockInventoryMovementItemsRepository.Verify (
			m => m.BulkInsertAsync (
				It.Is<IEnumerable<InventoryMovementItem>> (items => items.Any (i => i.ItemId == itemId))
			),
			Times.Once()
		);
		mockInventoryService.Verify (
			i => i.UpdateInventoryQuantitiesAsync (
				It.Is<Dictionary<Guid, float>> (
					q => q.ContainsKey (itemId)
				)
			),
			Times.Once()
		);
		VerifyUserAccessedNTimes (2 + dto.Items.Count);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateWithBothSourceAndDestinationShouldReturnInserted ()
	{
		Guid outMovementId = Guid.NewGuid(), inMovementId = Guid.NewGuid();
		var sourceLocation = new StorageLocation
		{
			Id = Guid.NewGuid(),
			Name = "Cupboard"
		};
		var destinationLocation = new StorageLocation
		{
			Id = Guid.NewGuid(),
			Name = "Drawer"
		};
		var itemId = Guid.NewGuid();
		var dto = new InventoryModificationRequest
		{
			SourceLocationId = sourceLocation.Id,
			DestinationLocationId = destinationLocation.Id,
			Source = MovementSource.Transfer,
			Items = {
				{itemId, 0.5f}
			}
		};
		mockStorageLocationsRepository.Setup (l => l.ExistsAsync (sourceLocation.Id, userId))
										.ReturnsAsync (true);
		mockStorageLocationsRepository.Setup (l => l.ExistsAsync (destinationLocation.Id, userId))
										.ReturnsAsync (true);
		mockItemsService.Setup (i => i.GetAllFromListWithUnitAsync (It.IsAny<IEnumerable<Guid>>()))
						.ReturnsAsync (
							new Dictionary<Guid, ItemResponseWithUnit>
							{
								{
									itemId,
									new ItemResponseWithUnit ()
									{
										Id = itemId,
										Name = "Beef",
										Quantity = 0.5f,
										Unit = Unit.Kilogramme
									}
								}
							}
						);
		mockInventoryMovementsRepository.Setup (m => m.InsertAsync (It.IsAny<InventoryMovement>()))
										.ReturnsAsync (
											(InventoryMovement m) =>
											{
												m.Id = m.Direction == MovementDirection.In ? inMovementId : outMovementId;
												m.StorageLocation = m.Direction == MovementDirection.In ?
													destinationLocation :
													sourceLocation;
												return m;
											}
										);
		mockInventoryMovementItemsRepository.Setup (m => m.BulkInsertAsync (It.IsAny<IEnumerable<InventoryMovementItem>>()))
											.ReturnsAsync (
												(IEnumerable<InventoryMovementItem> items) => items
											);
		
		var res = await service.InsertAsync (dto);
		res.Should().NotBeNull();
		res.Should().HaveCount (2);
		res.First().Direction.Should().Be (MovementDirection.Out);
		res.First().Location.Should().NotBeNull();
		res.First().Location!.Id.Should().Be (sourceLocation.Id);
		res.First().NumberOfItems.Should().Be (1);
		res.First().Items.Should().HaveCount (1);
		res.First().Items.Should().AllSatisfy (i => i.Quantity.Should().BePositive());
		res.Last().Direction.Should().Be (MovementDirection.In);
		res.Last().Location.Should().NotBeNull();
		res.Last().Location!.Id.Should().Be (destinationLocation.Id);
		res.Last().NumberOfItems.Should().Be (1);
		res.Last().Items.Should().HaveCount (1);
		res.Last().Items.Should().AllSatisfy (i => i.Quantity.Should().BePositive());
		
		mockStorageLocationsRepository.Verify (l => l.ExistsAsync (sourceLocation.Id, userId), Times.Once());
		mockStorageLocationsRepository.Verify (l => l.ExistsAsync (destinationLocation.Id, userId), Times.Once());
		mockItemsService.Verify (i => i.GetAllFromListWithUnitAsync (It.Is<IEnumerable<Guid>> (ids => ids.Contains (itemId))), Times.Once());
		mockInventoryMovementsRepository.Verify (m => m.InsertAsync (It.Is<InventoryMovement> (i => i.NumberOfItems == 1)), Times.Exactly (2));
		mockInventoryMovementItemsRepository.Verify (
			m => m.BulkInsertAsync (
				It.Is<IEnumerable<InventoryMovementItem>> (items => items.Any (i => i.ItemId == itemId))
			),
			Times.Exactly (2)
		);
		VerifyUserAccessedNTimes (2 + 2 * (1 + dto.Items.Count));
		VerifyNoOtherCalls();
	}
}