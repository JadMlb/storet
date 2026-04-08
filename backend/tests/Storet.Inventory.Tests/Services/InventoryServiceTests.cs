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
		
		var inventoryInsertRequestResolver = new CurrentUserResolver<InventoryInsertRequest, Modules.Inventory.Models.Inventory> (mockCurrentUser.Object);
		var inventoryUpdateRequestResolver = new CurrentUserResolver<InventoryUpdateRequest, Modules.Inventory.Models.Inventory> (mockCurrentUser.Object);
		var config = new MapperConfiguration (
			cfg =>
			{
				cfg.ConstructServicesUsing (
					type => type == typeof (CurrentUserResolver<InventoryInsertRequest, Modules.Inventory.Models.Inventory>) ? inventoryInsertRequestResolver :
								type == typeof (CurrentUserResolver<InventoryUpdateRequest, Modules.Inventory.Models.Inventory>) ? inventoryUpdateRequestResolver :
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
	public async Task GetAllWithNoInventoryShouldReturnEmptyList ()
	{
		mockInventoryRepository.Setup (i => i.GetAllAsync (userId))
								.ReturnsAsync ([]);
								
		var result = await service.GetAllAsync();
		
		result.Should().BeEmpty();
		
		mockInventoryRepository.Verify (i => i.GetAllAsync (userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllWithDataShouldReturnList ()
	{
		Guid sugarId = Guid.NewGuid(), soapId = Guid.NewGuid(), appleId = Guid.NewGuid();
		var inventory = new List<Modules.Inventory.Models.Inventory>
		{
			new () {ItemId = sugarId, UserId = userId, QuantityInStock = 0, Status = Status.EmptyAccepted},
			new () {ItemId = soapId, UserId = userId, QuantityInStock = 3, Status = Status.Sufficient},
			new () {ItemId = appleId, UserId = userId, MaxQuantity = 10, QuantityInStock = 3, Status = Status.Critical},
		};
		
		var items = new Dictionary<Guid, ItemResponse>
		{
			{sugarId, new ItemResponse {Id = sugarId, Name = "Sugar"}},
			{soapId, new ItemResponse {Id = soapId, Name = "Soap"}},
			{appleId, new ItemResponse {Id = appleId, Name = "Apple"}}
		};
		
		mockInventoryRepository.Setup (i => i.GetAllAsync (userId))
								.ReturnsAsync (inventory);
		mockItemsService.Setup (i => i.GetAllFromListAsync (It.IsAny<IEnumerable<Guid>>()))
								.ReturnsAsync (items);
								
		var result = await service.GetAllAsync();
		
		result.Should().HaveCount (3);
		result.Should().AllSatisfy (i => i.Should().NotBeNull());
		result.Should().AllSatisfy (i => i.Item.Should().NotBeNull());
		result.Should().Contain (i => i.Item.Id == sugarId && i.QuantityInStock == 0 && i.Status == Status.EmptyAccepted);
		result.Should().Contain (i => i.Item.Id == soapId && i.QuantityInStock == 3 && i.Status == Status.Sufficient);
		result.Should().Contain (i => i.Item.Id == appleId && i.MaxQuantity == 10 && i.QuantityInStock == 3 && i.Status == Status.Critical);
		
		mockInventoryRepository.Verify (i => i.GetAllAsync (userId), Times.Once());
		mockItemsService.Verify (i => i.GetAllFromListAsync (It.IsAny<IEnumerable<Guid>>()), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateInventoryWithInvalidMinQuantityInRequestShouldThrowArgumentException ()
	{
		var insertDto = new InventoryInsertRequest
		{
			ItemId = Guid.NewGuid(),
			MinQuantity = -1
		};
		
		Func<Task> act = async () => await service.InsertAsync (insertDto);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Minimum quantity must be positive (>= 0)");
							
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateInventoryWithInvalidMaxQuantityInRequestShouldThrowArgumentException ()
	{
		var insertDto = new InventoryInsertRequest
		{
			ItemId = Guid.NewGuid(),
			MaxQuantity = 0
		};
		
		Func<Task> act = async () => await service.InsertAsync (insertDto);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Max quantity must be positive (> 0)");
							
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateInventoryWithMaxQuantityLessThanMinQuantityInRequestShouldThrowArgumentException ()
	{
		var insertDto = new InventoryInsertRequest
		{
			ItemId = Guid.NewGuid(),
			MinQuantity = 10,
			MaxQuantity = 1
		};
		
		Func<Task> act = async () => await service.InsertAsync (insertDto);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Max quantity must be greater than min quantity");
							
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateInventoryWithNonExistingItemIdShouldThrowNotFoundException ()
	{
		var itemId = Guid.NewGuid();
		var insertDto = new InventoryInsertRequest
		{
			ItemId = itemId,
			MaxQuantity = 10
		};
		
		mockItemsService.Setup (i => i.CheckIfExistsAndGetMetadataAsync (itemId))
						.ReturnsAsync ((ItemResponse?) null);
		
		Func<Task> act = async () => await service.InsertAsync (insertDto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Item with ID {itemId} was not found");
						
		mockItemsService.Verify (i => i.CheckIfExistsAndGetMetadataAsync (itemId), Times.Once());
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateInventoryWithValidDataShouldReturnInventoryWithItem ()
	{
		var itemId = Guid.NewGuid();
		var insertDto = new InventoryInsertRequest
		{
			ItemId = itemId
		};
		
		var item = new ItemResponse
		{
			Id = itemId,
			Name = "Sugar"
		};
		
		var inventory = new Modules.Inventory.Models.Inventory
		{
			ItemId = itemId,
			UserId = userId,
			MinQuantity = 0,
			QuantityInStock = 0,
			Status = Status.EmptyAccepted
		};
		
		mockItemsService.Setup (i => i.CheckIfExistsAndGetMetadataAsync (itemId))
						.ReturnsAsync (item);
		mockInventoryRepository.Setup (i => i.InsertAsync (It.IsAny<Modules.Inventory.Models.Inventory>()))
						.ReturnsAsync (inventory);
		
		var result = await service.InsertAsync (insertDto);
		
		result.Should().NotBeNull();
		result.Item.Should().NotBeNull();
		result.Item.Id.Should().Be (itemId);
		result.Item.Name.Should().Be ("Sugar");
		result.Item.Description.Should().BeNull();
		result.MaxQuantity.Should().BeNull();
		result.MinQuantity.Should().Be (0);
		result.QuantityInStock.Should().Be (0);
		result.Status.Should().Be (Status.EmptyAccepted);
						
		mockItemsService.Verify (i => i.CheckIfExistsAndGetMetadataAsync (itemId), Times.Once());
		mockInventoryRepository.Verify (
			i => i.InsertAsync (
				It.Is<Modules.Inventory.Models.Inventory> (
					i => i.ItemId == itemId
						&& i.MaxQuantity == null
						&& i.MinQuantity == 0
						&& i.Status == Status.EmptyAccepted
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
		mockItemsService.Setup (i => i.CheckIfExistsAndGetMetadataAsync (itemId))
						.ReturnsAsync ((ItemResponse?) null);
						
		Func<Task> act = async () => await service.UpdateAsync (itemId, updateDto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Item with ID {itemId} was not found");
		
		mockInventoryRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		mockItemsService.Verify (i => i.CheckIfExistsAndGetMetadataAsync (itemId), Times.Once());
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
		mockItemsService.Setup (i => i.CheckIfExistsAndGetMetadataAsync (itemId))
						.ReturnsAsync (
							new ItemResponse
							{
								Id = itemId,
								Name = "Cup"
							}
						);
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
		res.Item.Should().NotBeNull();
		res.Item.Id.Should().Be (itemId);
		res.Item.Name.Should().Be ("Cup");
		res.MaxQuantity.Should().Be (10);
		res.MinQuantity.Should().Be (0);
		res.QuantityInStock.Should().Be (0);
		res.Status.Should().Be (Status.EmptyAccepted);
		
		mockInventoryRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		mockItemsService.Verify (i => i.CheckIfExistsAndGetMetadataAsync (itemId), Times.Once());
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
		mockItemsService.Setup (i => i.CheckIfExistsAndGetMetadataAsync (itemId))
						.ReturnsAsync (
							new ItemResponse
							{
								Id = itemId,
								Name = "Cup"
							}
						);
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
		res.Item.Should().NotBeNull();
		res.Item.Id.Should().Be (itemId);
		res.Item.Name.Should().Be ("Cup");
		res.MaxQuantity.Should().Be (10);
		res.MinQuantity.Should().Be (0);
		res.QuantityInStock.Should().Be (5);
		res.Status.Should().Be (Status.Sufficient);
		
		mockInventoryRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		mockItemsService.Verify (i => i.CheckIfExistsAndGetMetadataAsync (itemId), Times.Once());
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
		mockItemsService.Setup (i => i.CheckIfExistsAndGetMetadataAsync (itemId))
						.ReturnsAsync (
							new ItemResponse
							{
								Id = itemId,
								Name = "Cup"
							}
						);
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
		res.Item.Should().NotBeNull();
		res.Item.Id.Should().Be (itemId);
		res.Item.Name.Should().Be ("Cup");
		res.MaxQuantity.Should().Be (10);
		res.MinQuantity.Should().Be (0);
		res.QuantityInStock.Should().Be (3);
		res.Status.Should().Be (Status.Critical);
		
		mockInventoryRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		mockItemsService.Verify (i => i.CheckIfExistsAndGetMetadataAsync (itemId), Times.Once());
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
		mockItemsService.Setup (i => i.GetAllFromListAsync (itemIds))
						.ReturnsAsync ([]);
						
		Func<Task> act = async () => await service.UpdateInventoryQuantitiesAsync (newQuantities);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Item with ID {itemId} was not found");
		
		mockInventoryRepository.Verify (i => i.GetAllFromListAsync (userId, itemIds), Times.Once());
		mockItemsService.Verify (i => i.GetAllFromListAsync (itemIds), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}

	[Fact]
	public async Task UpdateQuantityWithInvalidQuantityShouldThrowArgumentException ()
	{
		var newQuantities = new Dictionary<Guid, float>
		{
			{Guid.NewGuid(), -1}
		};
		Func<Task> act = async () => await service.UpdateInventoryQuantitiesAsync (newQuantities);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Inventory quantity must be >= 0");
		
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
		mockItemsService.Setup (i => i.GetAllFromListAsync (itemIds))
						.ReturnsAsync (
							new Dictionary<Guid, ItemResponse>
							{
								{itemId, new ItemResponse {Id = itemId, Name = "Cup"}
							}
						});
		mockInventoryRepository.Setup (i => i.BulkUpdateAsync (userId, It.IsAny<IEnumerable<Modules.Inventory.Models.Inventory>>()))
								.ReturnsAsync (1);
		
		var res = await service.UpdateInventoryQuantitiesAsync (newQuantities);
		
		res.Should().BeTrue();
		
		mockInventoryRepository.Verify (i => i.GetAllFromListAsync (userId, itemIds), Times.Once());
		mockItemsService.Verify (i => i.GetAllFromListAsync (itemIds), Times.Once());
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
		var itemId = Guid.NewGuid();
		mockInventoryRepository.Setup (i => i.DeleteAsync (itemId, userId))
								.ReturnsAsync (false);
		
		var res = await service.DeleteAsync (itemId);
		res.Should().BeFalse();
		
		mockInventoryRepository.Verify (i => i.DeleteAsync (itemId, userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteWithExistingItemShouldReturnTrue ()
	{
		var itemId = Guid.NewGuid();
		mockInventoryRepository.Setup (i => i.DeleteAsync (itemId, userId))
								.ReturnsAsync (true);
		
		var res = await service.DeleteAsync (itemId);
		res.Should().BeTrue();
		
		mockInventoryRepository.Verify (i => i.DeleteAsync (itemId, userId), Times.Once());
		VerifyUserAccessedNTimes();
		VerifyNoOtherCalls();
	}
}