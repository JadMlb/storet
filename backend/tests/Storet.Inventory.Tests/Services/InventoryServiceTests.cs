using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Storet.Core.Authorization;
using Storet.Core.Exceptions;
using Storet.Core.Mappers;
using Storet.Modules.Inventory.Contracts.Inventory;
using Storet.Modules.Inventory.Mappers;
using Storet.Modules.Inventory.Models;
using Storet.Modules.Inventory.Queries;
using Storet.Modules.Inventory.Repositories.Inventory;
using Storet.Modules.Inventory.Services.Inventory;
using Storet.Modules.ItemsCatalogue.Contracts.Items;
using Storet.Modules.ItemsCatalogue.Services.Items;

namespace Storet.Inventory.Tests.Services;

public class InventoryServiceTests
{
	private readonly IMapper mapper;
	private readonly ILoggerFactory loggerFactory;
	private readonly InventoryService service;
	private readonly Mock<IInventoryRepository> mockInventoryRepository;
	private readonly Mock<IItemsService> mockItemsService;
	private readonly Mock<ICurrentUser> mockCurrentUser;
	private readonly Guid userId = Guid.NewGuid();
	
	public InventoryServiceTests ()
	{
		mockInventoryRepository = new Mock<IInventoryRepository>();
		mockItemsService = new Mock<IItemsService>();
		mockCurrentUser = new Mock<ICurrentUser>();
		loggerFactory = new LoggerFactory();
		
		var inventoryUpdateRequestResolver = new CurrentUserResolver<InventoryUpdateRequest, Modules.Inventory.Models.Inventory> (mockCurrentUser.Object);
		var config = new MapperConfiguration (
			cfg =>
			{
				cfg.ConstructServicesUsing (
					type => type == typeof (CurrentUserResolver<InventoryUpdateRequest, Modules.Inventory.Models.Inventory>) ? inventoryUpdateRequestResolver :
								null
				);
				cfg.AddProfile<MappingProfile>();
			},
			loggerFactory
		);
		mapper = config.CreateMapper();
		
		service = new InventoryService (mockInventoryRepository.Object, mockItemsService.Object, mockCurrentUser.Object, mapper);
		
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
		mockInventoryRepository.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetOneWithNonExistingInventoryShouldReturnEmptyDictionary ()
	{
		mockItemsService.Setup (i => i.GetComponentIdsForItemsAsync (It.IsAny<IEnumerable<Guid>>()))
						.ReturnsAsync ([]);
		mockInventoryRepository.Setup (i => i.GetAllFromListAsync (userId, It.IsAny<IEnumerable<Guid>>()))
								.ReturnsAsync ([]);

		var itemId = Guid.NewGuid();
		var result = await service.GetOneAsync (itemId);

		result.Should().BeEmpty();

		mockItemsService.Verify (
			i => i.GetComponentIdsForItemsAsync (
				It.Is<IEnumerable<Guid>> (ids => ids.Count() == 1 && ids.Contains (itemId))
			),
			Times.Once()
		);
		mockInventoryRepository.Verify (
			i => i.GetAllFromListAsync (
				userId,
				It.Is<IEnumerable<Guid>> (ids => ids.Count() == 1 && ids.Contains (itemId))
			),
			Times.Once()
		);
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetOneWithComponentsShouldReturnList ()
	{
		mockItemsService.Setup (i => i.GetComponentIdsForItemsAsync (It.IsAny<IEnumerable<Guid>>()))
						.ReturnsAsync ([]);
		mockInventoryRepository.Setup (i => i.GetAllFromListAsync (userId, It.IsAny<IEnumerable<Guid>>()))
								.ReturnsAsync (
									(Guid userId, IEnumerable<Guid> ids) =>
									ids.ToDictionary (
										id => id,
										id => new Modules.Inventory.Models.Inventory
										{
											ItemId = id,
											UserId = userId,
											QuantityInStock = new Random().NextSingle() * 10 + 1,
											Status = Status.Sufficient
										}
									)
								);

		var itemId = Guid.NewGuid();
		var result = await service.GetOneAsync (itemId);

		result.Should().NotBeEmpty();

		mockItemsService.Verify (
			i => i.GetComponentIdsForItemsAsync (
				It.Is<IEnumerable<Guid>> (
					ids => ids.Count() == 1 && ids.Contains (itemId)
				)
			),
			Times.Once()
		);
		mockInventoryRepository.Verify (
			i => i.GetAllFromListAsync (
				userId,
				It.Is<IEnumerable<Guid>> (
					ids => ids.Count() == 1 && ids.Contains (itemId)
				)
			),
			Times.Once()
		);
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllWithEmptyStringShouldThrowArgumentException ()
	{
		InventoryFilterQuery query = new ()
		{
			Items = ""
		};
		
		Func<Task> act = async () => await service.GetAllAsync (query);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Cannot fetch inventories for empty items list");
		
		VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetAllWithInvalidGuidStringShouldThrowArgumentException ()
	{
		InventoryFilterQuery query = new ()
		{
			Items = "123-4qdskjsd-kaskas"
		};
		
		Func<Task> act = async () => await service.GetAllAsync (query);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("One or more provided ID is not a valid Guid");
		
		VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetAllWithNoInventoryShouldReturnDictionaryWithEmptyArray ()
	{
		var itemId = Guid.NewGuid();
		InventoryFilterQuery query = new ()
		{
			Items = itemId.ToString()
		};

		mockItemsService.Setup (i => i.GetComponentIdsForItemsAsync (It.IsAny<IEnumerable<Guid>>()))
						.ReturnsAsync (
							(IEnumerable<Guid> ids) => ids.ToDictionary (
								id => id,
								_ => new List<Guid>()
							)
						);
		mockInventoryRepository.Setup (i => i.GetAllFromListAsync (userId, It.IsAny<IEnumerable<Guid>>()))
								.ReturnsAsync (
									(Guid userId, IEnumerable<Guid> ids) => ids.ToDictionary (
										id => id,
										id => new Modules.Inventory.Models.Inventory
										{
											ItemId = id,
											QuantityInStock = new Random().NextSingle() * 10 + 0.1f,
											Status = Status.Sufficient,
											UserId = userId
										}
									)
								);

		var result = await service.GetAllAsync (query);

		result.Should().NotBeEmpty();
		var resultForItemId = result.GetValueOrDefault (itemId);
		resultForItemId.Should().NotBeNull();
		resultForItemId.Should().HaveCount (1);
		resultForItemId.Should().Contain (inv => inv.ItemId == itemId);

		mockItemsService.Verify (
			i => i.GetComponentIdsForItemsAsync (
				It.Is<IEnumerable<Guid>> (
					ids => ids.Contains (itemId) && ids.Count() == 1
				)
			),
			Times.Once()
		);
		mockInventoryRepository.Verify (
			i => i.GetAllFromListAsync (
				userId,
				It.Is<IEnumerable<Guid>> (
					ids => ids.Contains (itemId) && ids.Count() == 1
				)
			),
			Times.Once()
		);
		VerifyUserAccessedNTimes (1);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllWithDataShouldReturnList ()
	{
		Guid sugarBoxId = Guid.NewGuid(), sugarId = Guid.NewGuid(), soapId = Guid.NewGuid(), appleId = Guid.NewGuid();
		InventoryFilterQuery query = new ()
		{
			Items = $"{sugarBoxId},{soapId}"
		};
		var inventory = new Dictionary<Guid, Modules.Inventory.Models.Inventory>
		{
			{sugarBoxId, new () {ItemId = sugarId, UserId = userId, QuantityInStock = 0, Status = Status.EmptyAccepted}},
			{soapId, new () {ItemId = soapId, UserId = userId, QuantityInStock = 3, Status = Status.Sufficient}},
			{appleId, new () {ItemId = appleId, UserId = userId, MaxQuantity = 10, QuantityInStock = 3, Status = Status.Critical}},
		};
		
		mockItemsService.Setup (i => i.GetComponentIdsForItemsAsync (It.IsAny<IEnumerable<Guid>>()))
						.ReturnsAsync (
							(IEnumerable<Guid> ids) => ids.ToDictionary (
								id => id,
								id => new List<Guid> {id == sugarBoxId ? sugarBoxId : id}
							)
						);
		mockInventoryRepository.Setup (i => i.GetAllFromListAsync (userId, It.IsAny<IEnumerable<Guid>>()))
								.ReturnsAsync (
									(Guid userId, IEnumerable<Guid> itemIds) =>
									inventory.Where (kv => itemIds.Contains (kv.Key))
												.ToDictionary (kv => kv.Key, kv => kv.Value)
								);
		
		var result = await service.GetAllAsync (query);
		
		result.Should().HaveCount (2);
		result.Should().AllSatisfy (i => i.Should().NotBeNull());
		result.Should().Contain (i =>
			i.Key == sugarBoxId
			&& i.Value.Count() == 1
			&& i.Value.First().ItemId == sugarId
			&& i.Value.First().QuantityInStock == 0
			&& i.Value.First().Status == Status.EmptyAccepted
		);
		result.Should().Contain (i =>
			i.Key == soapId
			&& i.Value.Count() == 1
			&& i.Value.First().ItemId == soapId
			&& i.Value.First().QuantityInStock == 3
			&& i.Value.First().Status == Status.Sufficient
		);

		mockItemsService.Verify (
			i => i.GetComponentIdsForItemsAsync (
				It.Is<IEnumerable<Guid>> (
					ids => ids.Count() == 2 && ids.Contains (sugarBoxId) && ids.Contains (soapId)
				)
			),
			Times.Once()
		);
		mockInventoryRepository.Verify (
			i => i.GetAllFromListAsync (
				userId, It.Is<IEnumerable<Guid>> (
					ids => ids.Count() == 2 && ids.Contains (sugarBoxId) && ids.Contains (soapId)
				)
			),
			Times.Once()
		);
		
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateInventoryWithValidDataShouldInsertSilently ()
	{
		var itemId = Guid.NewGuid();
		
		var inventory = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			MinQuantity = 0,
			QuantityInStock = 0,
			Status = Status.EmptyAccepted
		};
		
		mockInventoryRepository.Setup (i => i.BulkInsertAsync (It.IsAny<IEnumerable<Modules.Inventory.Models.Inventory>>()))
						.ReturnsAsync (1);
		
		await service.BulkInsertAsync ([itemId]);
		
		mockInventoryRepository.Verify (
			i => i.BulkInsertAsync (
				It.Is<IEnumerable<Modules.Inventory.Models.Inventory>> (
					records => records.Any (
						inventory => inventory.ItemId == itemId
								&& inventory.MaxQuantity == null
								&& inventory.MinQuantity == 0
								&& inventory.Status == Status.EmptyAccepted
					)
				)
			),
			Times.Once()
		);
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithNonExistentInventoryEntryShouldThrowNotFoundException ()
	{
		var updateDto = new InventoryUpdateRequest
		{
			MinQuantity = 1
		};
		
		var itemId = Guid.NewGuid();
		mockInventoryRepository.Setup (i => i.GetOneAsync (itemId, userId))
								.ReturnsAsync ((Modules.Inventory.Models.Inventory?) null);
						
		Func<Task> act = async () => await service.UpdateAsync (itemId, updateDto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Inventory with ID {itemId} was not found");
		
		mockInventoryRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithMaxQuantityLessThanMinQuantityInRequestShouldThrowArgumentException ()
	{
		var itemId = Guid.NewGuid();
		var insertDto = new InventoryUpdateRequest
		{
			MinQuantity = 10,
			MaxQuantity = 1
		};
		
		mockInventoryRepository.Setup (i => i.GetOneAsync (itemId, userId))
								.ReturnsAsync (
									new Modules.Inventory.Models.Inventory
									{
										ItemId = itemId,
										UserId = userId,
										MinQuantity = 1,
										MaxQuantity = 10,
										QuantityInStock = 2,
										Status = Status.Critical
									}
								);
		
		Func<Task> act = async () => await service.UpdateAsync (itemId, insertDto);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Max quantity must be greater than min quantity");
		
		mockInventoryRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithNonExistentItemShouldThrowNotFoundException ()
	{
		var updateDto = new InventoryUpdateRequest
		{
			MinQuantity = 1
		};
		
		var itemId = Guid.NewGuid();
		mockInventoryRepository.Setup (i => i.GetOneAsync (itemId, userId))
								.ReturnsAsync (
									new Modules.Inventory.Models.Inventory
									{
										ItemId = itemId,
										UserId = userId,
										MinQuantity = 1,
										MaxQuantity = 10,
										QuantityInStock = 2,
										Status = Status.Critical
									}
								);
		mockItemsService.Setup (i => i.ExistsAsync (itemId))
						.ReturnsAsync (false);
						
		Func<Task> act = async () => await service.UpdateAsync (itemId, updateDto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Item with ID {itemId} was not found");
		
		mockInventoryRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		mockItemsService.Verify (i => i.ExistsAsync (itemId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithInvalidMinQuantityShouldThrowArgumentException ()
	{
		var itemId = Guid.NewGuid();
		var updateDto = new InventoryUpdateRequest
		{
			MinQuantity = -1
		};
		
		mockInventoryRepository.Setup (i => i.GetOneAsync (itemId, userId))
								.ReturnsAsync (
									new Modules.Inventory.Models.Inventory
									{
										ItemId = itemId,
										UserId = userId,
										MinQuantity = 1,
										MaxQuantity = 10,
										QuantityInStock = 2,
										Status = Status.Critical
									}
								);
		
		Func<Task> act = async () => await service.UpdateAsync (itemId, updateDto);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Minimum quantity must be positive (>= 0)");
		
		mockInventoryRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithValidDataAndNoItemsInStockShouldUpdateAndReturn ()
	{
		var itemId = Guid.NewGuid();
		var updateDto = new InventoryUpdateRequest
		{
			MaxQuantity = 10
		};
		
		mockInventoryRepository.Setup (i => i.GetOneAsync (itemId, userId))
								.ReturnsAsync (
									new Modules.Inventory.Models.Inventory
									{
										ItemId = itemId,
										UserId = userId,
										MinQuantity = 1,
										MaxQuantity = 10,
										QuantityInStock = 2,
										Status = Status.Critical
									}
								);
		mockItemsService.Setup (i => i.ExistsAsync (itemId))
						.ReturnsAsync (true);
		mockInventoryRepository.Setup (i => i.UpdateAsync (itemId, userId, It.IsAny<Modules.Inventory.Models.Inventory>()))
								.ReturnsAsync (
									new Modules.Inventory.Models.Inventory
									{
										ItemId = itemId,
										MaxQuantity = 10,
										UserId = userId,
										QuantityInStock = 0,
										Status = Status.EmptyAccepted
									}
								);
		
		var res = await service.UpdateAsync (itemId, updateDto);
		
		res.Should().NotBeNull();
		res.ItemId.Should().Be (itemId);
		res.MaxQuantity.Should().Be (10);
		res.MinQuantity.Should().Be (0);
		res.QuantityInStock.Should().Be (0);
		res.Status.Should().Be (Status.EmptyAccepted);
		
		mockInventoryRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		mockItemsService.Verify (i => i.ExistsAsync (itemId), Times.Once());
		mockInventoryRepository.Verify (i => i.UpdateAsync (itemId, userId, It.Is<Modules.Inventory.Models.Inventory> (i => i.MaxQuantity == 10)), Times.Once());
		VerifyUserAccessedNTimes (3);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithValidDataAndExistingItemsInStockShouldUpdateAndReturnWithStatusSufficient ()
	{
		var itemId = Guid.NewGuid();
		var updateDto = new InventoryUpdateRequest
		{
			MaxQuantity = 10
		};
		
		mockInventoryRepository.Setup (i => i.GetOneAsync (itemId, userId))
								.ReturnsAsync (
									new Modules.Inventory.Models.Inventory
									{
										ItemId = itemId,
										UserId = userId,
										MinQuantity = 1,
										MaxQuantity = 10,
										QuantityInStock = 2,
										Status = Status.Critical
									}
								);
		mockItemsService.Setup (i => i.ExistsAsync (itemId))
						.ReturnsAsync (true);
		mockInventoryRepository.Setup (i => i.UpdateAsync (itemId, userId, It.IsAny<Modules.Inventory.Models.Inventory>()))
								.ReturnsAsync (
									new Modules.Inventory.Models.Inventory
									{
										ItemId = itemId,
										MaxQuantity = 10,
										UserId = userId,
										QuantityInStock = 5,
										Status = Status.Sufficient
									}
								);
		
		var res = await service.UpdateAsync (itemId, updateDto);
		
		res.Should().NotBeNull();
		res.ItemId.Should().Be (itemId);
		res.MaxQuantity.Should().Be (10);
		res.MinQuantity.Should().Be (0);
		res.QuantityInStock.Should().Be (5);
		res.Status.Should().Be (Status.Sufficient);
		
		mockInventoryRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		mockItemsService.Verify (i => i.ExistsAsync (itemId), Times.Once());
		mockInventoryRepository.Verify (i => i.UpdateAsync (itemId, userId, It.Is<Modules.Inventory.Models.Inventory> (i => i.MaxQuantity == 10)), Times.Once());
		VerifyUserAccessedNTimes (3);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateInventoryLimitsWithValidDataAndExistingItemsInStockShouldUpdateAndReturnWithStatusCritical ()
	{
		var itemId = Guid.NewGuid();
		var updateDto = new InventoryUpdateRequest
		{
			MaxQuantity = 10
		};
		
		mockInventoryRepository.Setup (i => i.GetOneAsync (itemId, userId))
								.ReturnsAsync (
									new Modules.Inventory.Models.Inventory
									{
										ItemId = itemId,
										UserId = userId,
										MinQuantity = 1,
										MaxQuantity = 10,
										QuantityInStock = 2,
										Status = Status.Critical
									}
								);
		mockItemsService.Setup (i => i.ExistsAsync (itemId))
						.ReturnsAsync (true);
		mockInventoryRepository.Setup (i => i.UpdateAsync (itemId, userId, It.IsAny<Modules.Inventory.Models.Inventory>()))
								.ReturnsAsync (
									new Modules.Inventory.Models.Inventory
									{
										ItemId = itemId,
										MaxQuantity = 10,
										UserId = userId,
										QuantityInStock = 3,
										Status = Status.Critical
									}
								);
		
		var res = await service.UpdateAsync (itemId, updateDto);
		
		res.Should().NotBeNull();
		res.ItemId.Should().Be (itemId);
		res.MaxQuantity.Should().Be (10);
		res.MinQuantity.Should().Be (0);
		res.QuantityInStock.Should().Be (3);
		res.Status.Should().Be (Status.Critical);
		
		mockInventoryRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		mockItemsService.Verify (i => i.ExistsAsync (itemId), Times.Once());
		mockInventoryRepository.Verify (i => i.UpdateAsync (itemId, userId, It.Is<Modules.Inventory.Models.Inventory> (i => i.MaxQuantity == 10)), Times.Once());
		VerifyUserAccessedNTimes (3);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateQuantityWithNonExistentInventoryRecordShouldThrowNotFoundException ()
	{
		var itemId = Guid.NewGuid();
		var newQuantities = new Dictionary<Guid, float>
		{
			{itemId, 1}
		};
		var itemIds = new List<Guid> {itemId};
		
		mockInventoryRepository.Setup (i => i.GetAllFromListAsync (userId, itemIds))
								.ReturnsAsync ([]);
						
		Func<Task> act = async () => await service.UpdateInventoryQuantitiesAsync (newQuantities);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Inventory with ID {itemId} was not found");
		
		mockInventoryRepository.Verify (i => i.GetAllFromListAsync (userId, itemIds), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateQuantityWithNonExistentItemShouldThrowNotFoundException ()
	{
		var itemId = Guid.NewGuid();
		var newQuantities = new Dictionary<Guid, float>
		{
			{itemId, 1}
		};
		var itemIds = new List<Guid> {itemId};
		
		mockInventoryRepository.Setup (i => i.GetAllFromListAsync (userId, itemIds))
								.ReturnsAsync (
									new Dictionary<Guid, Modules.Inventory.Models.Inventory>
									{
										{
											itemId,
											new Modules.Inventory.Models.Inventory
											{
												ItemId = itemId,
												UserId = userId,
												QuantityInStock = 0,
												Status = Status.EmptyAccepted
											}
										}
									}
								);
		mockItemsService.Setup (i => i.AllExistAsync (itemIds))
						.ReturnsAsync (false);
						
		Func<Task> act = async () => await service.UpdateInventoryQuantitiesAsync (newQuantities);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Item with ID {itemId} was not found");
		
		mockInventoryRepository.Verify (i => i.GetAllFromListAsync (userId, itemIds), Times.Once());
		mockItemsService.Verify (i => i.AllExistAsync (itemIds), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}

	[Fact]
	public async Task UpdateQuantityWithValidQuantityShouldReturnTrue ()
	{
		var itemId = Guid.NewGuid();
		var newQuantities = new Dictionary<Guid, float>
		{
			{itemId, 1}
		};
		var itemIds = new List<Guid> {itemId};
		
		mockInventoryRepository.Setup (i => i.GetAllFromListAsync (userId, itemIds))
								.ReturnsAsync (
									new Dictionary<Guid, Modules.Inventory.Models.Inventory>
									{
										{
											itemId,
											new Modules.Inventory.Models.Inventory
											{
												ItemId = itemId,
												UserId = userId,
												QuantityInStock = 0,
												Status = Status.EmptyAccepted
											}
										}
									}
								);
		mockItemsService.Setup (i => i.AllExistAsync (itemIds))
						.ReturnsAsync (true);
		mockInventoryRepository.Setup (i => i.BulkUpdateAsync (userId, It.IsAny<IEnumerable<Modules.Inventory.Models.Inventory>>()))
								.ReturnsAsync (1);
		
		var res = await service.UpdateInventoryQuantitiesAsync (newQuantities);
		
		res.Should().BeTrue();
		
		mockInventoryRepository.Verify (i => i.GetAllFromListAsync (userId, itemIds), Times.Once());
		mockItemsService.Verify (i => i.AllExistAsync (itemIds), Times.Once());
		mockInventoryRepository.Verify (
			i => i.BulkUpdateAsync (
				userId,
				It.Is<IEnumerable<Modules.Inventory.Models.Inventory>> (
					inventories => inventories.Any (i => i.ItemId == itemId && i.QuantityInStock == 1)
				)
			),
			Times.Once()
		);
		VerifyUserAccessedNTimes (1 + newQuantities.Count);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteWithNonExistentItemShouldReturnFalse ()
	{
		IEnumerable<Guid> itemIds = [Guid.NewGuid()];
		mockInventoryRepository.Setup (i => i.BulkDeleteAsync (userId, itemIds))
								.ReturnsAsync (false);
		
		var res = await service.BulkDeleteAsync (itemIds);
		res.Should().BeFalse();
		
		mockInventoryRepository.Verify (i => i.BulkDeleteAsync (userId, itemIds), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteWithExistingItemShouldReturnTrue ()
	{
		IEnumerable<Guid> itemIds = [Guid.NewGuid()];
		mockInventoryRepository.Setup (i => i.BulkDeleteAsync (userId, itemIds))
								.ReturnsAsync (true);
		
		var res = await service.BulkDeleteAsync (itemIds);
		res.Should().BeTrue();
		
		mockInventoryRepository.Verify (i => i.BulkDeleteAsync (userId, itemIds), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
}