using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Storet.Core.Authorization;
using Storet.Core.Exceptions;
using Storet.Core.Mappers;
using Storet.Core.Utils;
using Storet.Modules.ItemsCatalogue.Contracts.Categories;
using Storet.Modules.ItemsCatalogue.Contracts.Items;
using Storet.Modules.ItemsCatalogue.Events;
using Storet.Modules.ItemsCatalogue.Mappers;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Modules.ItemsCatalogue.Repositories.Categories;
using Storet.Modules.ItemsCatalogue.Repositories.Items;
using Storet.Modules.ItemsCatalogue.Repositories.ItemsCategories;
using Storet.Modules.ItemsCatalogue.Repositories.ItemsCompositions;
using Storet.Modules.ItemsCatalogue.Services.Items;

namespace Storet.ItemsCatalogue.Tests.Services;

public class ItemsServiceTests
{
	private readonly Mock<IItemsRepository> mockItemsRepository;
	private readonly Mock<IItemsCategoriesRepository> mockItemsCategoriesRepository;
	private readonly Mock<IItemsCompositionRepository> mockItemsCompositionsRepository;
	private readonly Mock<ICategoriesRepository> mockCategoriesRepository;
	private readonly Mock<ICurrentUser> mockCurrentUser;
	private readonly Mock<IMediator> mockMediator;
	private readonly ItemsService service;
	private readonly IMapper mapper;
	private readonly ILoggerFactory loggerFactory;
	
	public ItemsServiceTests ()
	{
		mockItemsRepository = new Mock<IItemsRepository>();
		mockItemsCategoriesRepository = new Mock<IItemsCategoriesRepository>();
		mockCategoriesRepository = new Mock<ICategoriesRepository>();
		mockItemsCompositionsRepository = new Mock<IItemsCompositionRepository>();
		mockCurrentUser = new Mock<ICurrentUser>();
		mockMediator = new Mock<IMediator>();
		loggerFactory = new LoggerFactory();
		
		var categoryInsertRequestResolver = new CurrentUserResolver<CategoryInsertRequest, Category> (mockCurrentUser.Object);
		var categoryUpdateRequestResolver = new CurrentUserResolver<CategoryUpdateRequest, Category> (mockCurrentUser.Object);
		var itemInsertRequestResolver = new CurrentUserResolver<ItemInsertRequest, Item> (mockCurrentUser.Object);
		var itemUpdateRequestResolver = new CurrentUserResolver<ItemUpdateRequest, Item> (mockCurrentUser.Object);

		var config = new MapperConfiguration (
			cfg =>
			{
				cfg.ConstructServicesUsing (
					type => type == typeof (CurrentUserResolver<CategoryInsertRequest, Category>) ? categoryInsertRequestResolver :
								type == typeof (CurrentUserResolver<CategoryUpdateRequest, Category>) ? categoryUpdateRequestResolver :
								type == typeof (CurrentUserResolver<ItemInsertRequest, Item>) ? itemInsertRequestResolver :
								type == typeof (CurrentUserResolver<ItemUpdateRequest, Item>) ? itemUpdateRequestResolver :
								null
				);
				cfg.AddProfile<MappingProfile>();
			},
			loggerFactory
		);
		mapper = config.CreateMapper();
		service = new ItemsService (mockItemsRepository.Object, mockCategoriesRepository.Object, mockItemsCategoriesRepository.Object, mockItemsCompositionsRepository.Object, mockCurrentUser.Object, mapper, mockMediator.Object);
	}
	
	private void VerifyCreatedEventEmitted (IEnumerable<Guid> expectedItemIds)
	{
		mockMediator.Verify (
			m => m.Publish (
				It.Is<ItemsCreatedEvent> (
					e => e.ItemIds.ToHashSet().SetEquals (expectedItemIds.ToHashSet())
				)
			),
			Times.Once()
		);
	}
	
	private void VerifyDeletedEventEmitted (IEnumerable<Guid> expectedItemIds)
	{
		mockMediator.Verify (
			m => m.Publish (
				It.Is<ItemsDeletedEvent> (
					e => e.ItemIds.ToHashSet().SetEquals (expectedItemIds.ToHashSet())
				)
			),
			Times.Once()
		);
	}
	
	private void VerifyNoOtherCalls ()
	{
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
		mockMediator.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateItemWithZeroOrLessQuantityShouldThrowArgumentException ()
	{
		var item = new ItemInsertRequest
		{
			Name = "Chicken breasts",
			Categories = [1],
			Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme
		};
		
		Func<Task> act = async () => await service.InsertAsync (item);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Item must have a positive quantity");
		
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateItemWithValidDataShouldCreateItem ()
	{
		var userId = Guid.NewGuid();
		var categoryIds = new List<int> {1};
		
		var itemCreateDto = new ItemInsertRequest
		{
			Name = "Tea Box",
			Quantity = 1,
			Unit = Modules.ItemsCatalogue.Models.Unit.Unit,
			Categories = [1],
			Components = [
				new ()
				{
					Name = "Tea Bag",
					Quantity = 10
				}
			]
		};
		
		var category = new Category
		{
			Id = 1,
			Label = "Food",
			UserId = userId
		};
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockCategoriesRepository.Setup (c => c.AllExistAsync (userId, categoryIds))
								.ReturnsAsync (true);
		Guid generatedItemId = Guid.NewGuid();
		mockItemsRepository.Setup (i => i.InsertAsync (It.IsAny<Item>()))
							.ReturnsAsync (
								(Item i) =>
								{
									i.Id = generatedItemId;
									i.UserId = userId;
									return i;
								}
							);
		mockItemsCategoriesRepository.Setup (i => i.BulkInsertAsync (It.IsAny<IEnumerable<ItemCategory>>()))
									.ReturnsAsync (
										(IEnumerable<ItemCategory> itemCategories) => itemCategories.Count()
									);
		var teaBagId = Guid.NewGuid();
		mockItemsRepository.Setup (i => i.BulkInsertAsync (It.IsAny<IEnumerable<Item>>()))
							.ReturnsAsync (
								[
									new Item
									{
										Id = teaBagId,
										Name = "Tea Bag",
										Quantity = 1,
										Unit = Modules.ItemsCatalogue.Models.Unit.Unit,
										UserId = userId
									}
								]
							);
		mockItemsCompositionsRepository.Setup (c => c.BulkInsertAsync (It.IsAny<IEnumerable<ItemComposition>>()))
										.ReturnsAsync (1);
		mockItemsRepository.Setup (i => i.GetOneAsync (generatedItemId, userId))
							.ReturnsAsync (
								new Item
								{
									Id = generatedItemId,
									ItemCategories = [
										new ()
										{
											CategoryId = 1,
											Category = category,
											ItemId = generatedItemId,
											UserId = userId
										}
									],
									Name = "Tea Box",
									UserId = userId
								}
							);
		
		var result = await service.InsertAsync (itemCreateDto);
		result.Should().NotBeNull();
		result.Id.Should().NotBeEmpty();
		result.Name.Should().Be ("Tea Box");
		
		mockCurrentUser.Verify (u => u.Id, Times.Exactly (3 + itemCreateDto.Categories.Count + itemCreateDto.Components.Count * 2 + 1));
		mockCategoriesRepository.Verify (
			c => c.AllExistAsync (userId, categoryIds),
			Times.Once()
		);
		mockItemsRepository.Verify (
			i => i.InsertAsync (It.Is<Item> (i => i.Name == "Tea Box")),
			Times.Once()
		);
		mockItemsCategoriesRepository.Verify (
			i => i.BulkInsertAsync (
				It.Is<IEnumerable<ItemCategory>> (
					c => c.Count() == 1 && c.Any (c => c.CategoryId == 1 && c.ItemId == generatedItemId)
				)
			),
			Times.Once()
		);
		mockItemsRepository.Verify (
			i => i.BulkInsertAsync (It.Is<IEnumerable<Item>> (items => items.Count() == 1)),
			Times.Once()
		);
		mockItemsCompositionsRepository.Verify (
			c => c.BulkInsertAsync (It.Is<IEnumerable<ItemComposition>>(items => items.Count() == 1)),
			Times.Once()
		);
		mockItemsRepository.Verify (
			i => i.GetOneAsync (generatedItemId, userId),
			Times.Once()
		);
		VerifyCreatedEventEmitted ([teaBagId]);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateItemWithNonExistentCategoryShouldThrowNotFoundException ()
	{
		var userId = Guid.NewGuid();
		var categoryIds = new List<int> {999};
		var itemCreateDto = new ItemInsertRequest
		{
			Name = "Steak",
			Quantity = 400,
			Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme,
			Categories = [999]
		};
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockCategoriesRepository.Setup (c => c.AllExistAsync (userId, categoryIds))
								.ReturnsAsync (false);
								
		Func<Task> act = async () => await service.InsertAsync (itemCreateDto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ("Category with ID 999 was not found");
							
		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockCategoriesRepository.Verify (c => c.AllExistAsync (userId, categoryIds), Times.Once());
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateItemWithInvalidNumberOfCategoriesShouldThrowArgumentException ()
	{
		var itemCreateDto = new ItemInsertRequest
		{
			Name = "Steak",
			Quantity = 400,
			Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme,
			Categories = []
		};
		
		Func<Task> act = async () => await service.InsertAsync (itemCreateDto);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Item must be created with at least 1 category");
							
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateItemWithNonExistingComponentShouldThrowNotFoundException ()
	{
		var nonExistentComponentId = Guid.NewGuid();
		
		var itemCreateDto = new ItemInsertRequest
		{
			Name = "Tea Box",
			Quantity = 1,
			Unit = Modules.ItemsCatalogue.Models.Unit.Unit,
			Categories = [1],
			Components = [
				new () {Id = nonExistentComponentId, Quantity = 10}
			]
		};
		
		var userId = Guid.NewGuid();
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockCategoriesRepository.Setup (c => c.AllExistAsync (userId, It.IsAny<IEnumerable<int>>()))
								.ReturnsAsync (true);
		mockItemsRepository.Setup (i => i.AllExistAsync (userId, It.IsAny<IEnumerable<Guid>>()))
							.ReturnsAsync (false);
		
		Func<Task> act = async () => await service.InsertAsync (itemCreateDto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Item with ID {nonExistentComponentId} was not found");
							
		mockCurrentUser.Verify (u => u.Id, Times.Exactly (2));
		mockCategoriesRepository.Verify (
			c => c.AllExistAsync (userId, It.Is<IEnumerable<int>> (c => c.Contains (1))),
			Times.Once()
		);
		mockItemsRepository.Verify (
			i => i.AllExistAsync (userId, It.Is<IEnumerable<Guid>> (i => i.Contains (nonExistentComponentId))),
			Times.Once()
		);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllItemsWithInvalidQueryShouldThrowException ()
	{
		var query = new Query<string>
		{
			PageSize = 0
		};
		
		Func<Task> act = async () => await service.GetAllAsync (query);
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Invalid page size for query");
							
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllItemsWithValidQueryOnFirstPageShouldReturnList ()
	{
		var query = new Query<string>
		{
			PageSize = 5
		};
		
		var userId = Guid.NewGuid();
		var items = new List<Item>
		{
			new () {Id = Guid.NewGuid(), Name = "Apple", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Banana", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Chicken breasts", Quantity = 500, Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Chocolate", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Lemon", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Orange", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Rice", Quantity = 100, Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Steak", Quantity = 500, Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Sugar", Quantity = 100, Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Tea", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
		};
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockItemsRepository.Setup (i => i.GetAllAsync (It.IsAny<Query<string>>(), userId))
							.ReturnsAsync (items.Take (query.PageSize + 1));
		mockItemsRepository.Setup (i => i.GetPreviousKeyAsync (It.IsAny<Query<string>>(), userId))
							.ReturnsAsync ((string?) null);
		
		var result = await service.GetAllAsync (query);
		
		result.Should().NotBeNull();
		result.HasNext.Should().BeTrue();
		result.HasPrevious.Should().BeFalse();
		result.Next.Should().Be ("Orange");
		result.Previous.Should().BeNull();
		result.Data.Should().HaveCount (5);
		result.Data.Should().BeInAscendingOrder (i => i.Name);
		
		mockCurrentUser.Verify (u => u.Id, Times.Exactly (2));
		mockItemsRepository.Verify (
			i => i.GetAllAsync (It.Is<Query<string>> (q => q.Key == null && q.PageSize == 5), userId),
			Times.Once()
		);
		mockItemsRepository.Verify (
			i => i.GetPreviousKeyAsync (It.Is<Query<string>> (q => q.Key == null && q.PageSize == 5), userId),
			Times.Once()
		);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllItemsWithValidQueryOnSecondPageShouldReturnList ()
	{
		var query = new Query<string>
		{
			PageSize = 5,
			Key = "Orange"
		};
		
		var userId = Guid.NewGuid();
		var items = new List<Item>
		{
			new () {Id = Guid.NewGuid(), Name = "Apple", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Banana", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Chicken breasts", Quantity = 500, Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Chocolate", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Lemon", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Orange", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Rice", Quantity = 100, Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Steak", Quantity = 500, Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Sugar", Quantity = 100, Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme, UserId = userId},
			new () {Id = Guid.NewGuid(), Name = "Tea", Quantity = 1, Unit = Modules.ItemsCatalogue.Models.Unit.Unit, UserId = userId},
		};
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockItemsRepository.Setup (i => i.GetAllAsync (It.IsAny<Query<string>>(), userId))
							.ReturnsAsync (items.Where(i => string.Compare (i.Name, query.Key) >= 0).Take (query.PageSize + 1));
		mockItemsRepository.Setup (i => i.GetPreviousKeyAsync (It.IsAny<Query<string>>(), userId))
							.ReturnsAsync ("Apple");
		
		var result = await service.GetAllAsync (query);
		
		result.Should().NotBeNull();
		result.HasNext.Should().BeFalse();
		result.HasPrevious.Should().BeTrue();
		result.Next.Should().BeNull();
		result.Previous.Should().Be ("Apple");
		result.Data.Should().HaveCount (5);
		result.Data.Should().BeInAscendingOrder (i => i.Name);
		
		mockCurrentUser.Verify (u => u.Id, Times.Exactly (2));
		mockItemsRepository.Verify (
			i => i.GetAllAsync (It.Is<Query<string>> (q => q.Key == "Orange" && q.PageSize == 5), userId),
			Times.Once()
		);
		mockItemsRepository.Verify (
			i => i.GetPreviousKeyAsync (It.Is<Query<string>> (q => q.Key == "Orange" && q.PageSize == 5), userId),
			Times.Once()
		);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllFromListAsyncWithNonExistentIdShouldThrowNotFoundException ()
	{
		var userId = Guid.NewGuid();
		var itemIds = new List<Guid> {Guid.NewGuid()};
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockItemsRepository.Setup (i => i.GetAllFromListAsync (userId, It.IsAny<IEnumerable<Guid>>()))
							.ReturnsAsync ([]);
							
		Func<Task> act = async () => await service.GetAllFromListAsync (itemIds);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Item with ID {itemIds[0]} was not found");
		
		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockItemsRepository.Verify (i => i.GetAllFromListAsync (userId, It.Is<IEnumerable<Guid>> (ids => ids.SequenceEqual (itemIds))), Times.Once());
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllFromListAsyncWithExistingIdShouldReturnList ()
	{
		var userId = Guid.NewGuid();
		var category = new Category
		{
			Id = 1,
			Label = "Food"
		};
		var itemId = Guid.NewGuid();
		var item = new Item
		{
			Id = itemId,
			Name = "Sugar Cube",
			Quantity = 0.1f,
			Unit = Modules.ItemsCatalogue.Models.Unit.Kilogramme,
			UserId = userId,
			ItemCategories = [
				new ()
				{
					ItemId = itemId,
					CategoryId = 1,
					UserId = userId
				}
			]
		};
		var itemIds = new List<Guid> {itemId};
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockItemsRepository.Setup (i => i.GetAllFromListAsync (userId, It.IsAny<IEnumerable<Guid>>()))
							.ReturnsAsync ([item]);
							
		var res = await service.GetAllFromListAsync (itemIds);
		
		res.Should().HaveCount (1);
		res.Should().Contain (i => i.Key == itemId);
		
		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockItemsRepository.Verify (i => i.GetAllFromListAsync (userId, It.Is<IEnumerable<Guid>> (ids => ids.SequenceEqual (itemIds))), Times.Once());
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetItemWithNonExistentIdShouldReturnNull ()
	{
		var userId = Guid.NewGuid();
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		var nonExistentId = Guid.NewGuid();
		mockItemsRepository.Setup (i => i.GetOneAsync (nonExistentId, userId))
							.ReturnsAsync ((Item?) null);
		
		var result = await service.GetOneAsync (nonExistentId);
		
		result.Should().BeNull();
		
		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockItemsRepository.Verify (i => i.GetOneAsync (nonExistentId, userId), Times.Once());
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetItemWithExistingIdShouldReturnItemWithCategories ()
	{
		var itemId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		var item = new Item
		{
			Id = itemId,
			Name = "Laptop",
			Quantity = 1,
			Unit = Modules.ItemsCatalogue.Models.Unit.Unit,
			ItemCategories = [
				new ()
				{
					ItemId = itemId,
					CategoryId = 1,
					Category = new Category
					{
						Id = 1,
						Label = "Electronics"
					}
				}
			],
			UserId = userId
		};
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockItemsRepository.Setup (i => i.GetOneAsync (itemId, userId))
							.ReturnsAsync (item);
		
		var result = await service.GetOneAsync (itemId);
		
		result.Should().NotBeNull();
		result.Name.Should().Be ("Laptop");
		result.Quantity.Should().Be (1);
		result.Unit.Should().Be (Modules.ItemsCatalogue.Models.Unit.Unit);
		result.Categories.Should().HaveCount (1);
		result.Categories.First().Id.Should().Be (1);
		result.Categories.First().Label.Should().Be ("Electronics");
		
		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockItemsRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateItemWithNonExistingItemShouldReturnNull ()
	{
		var itemUpdateDto = new ItemUpdateRequest
		{
			Name = "New Laptop",
			Description = "My new laptop",
			Categories = [2, 3]
		};
		
		var nonExistentItemId = Guid.NewGuid();
		
		var userId = Guid.NewGuid();
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockItemsRepository.Setup (i => i.ExistsAsync (nonExistentItemId, userId))
							.ReturnsAsync (false);
		
		var result = await service.UpdateAsync (nonExistentItemId, itemUpdateDto);
		
		result.Should().BeNull();
		
		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockItemsRepository.Verify (i => i.ExistsAsync (nonExistentItemId, userId), Times.Once());
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateItemWithNonExistingCategoryShouldThrowArgumentException ()
	{
		var itemUpdateDto = new ItemUpdateRequest
		{
			Name = "New Laptop",
			Description = "My new laptop",
			Categories = [1, 2]
		};
		
		var itemId = Guid.NewGuid();
		var newCategoriesIds = new List<int> {1, 2};
		
		var userId = Guid.NewGuid();
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockItemsRepository.Setup (i => i.ExistsAsync (itemId, userId))
							.ReturnsAsync (true);
		mockCategoriesRepository.Setup (c => c.AllExistAsync (userId, newCategoriesIds))
								.ReturnsAsync (false);
		
		Func<Task> act = async () => await service.UpdateAsync (itemId, itemUpdateDto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ("Category with ID 1,2 was not found");
		
		mockCurrentUser.Verify (u => u.Id, Times.Exactly (2));
		mockItemsRepository.Verify (i => i.ExistsAsync (itemId, userId), Times.Once());
		mockCategoriesRepository.Verify (
			i => i.AllExistAsync (
				userId,
				It.Is<IEnumerable<int>> (
					ids => ids.Contains (1) && ids.Contains (2)
				)
			),
			Times.Once()
		);
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateItemWithValidDataShouldUpdateAndReturnItemWithCategories ()
	{
		var itemUpdateDto = new ItemUpdateRequest
		{
			Name = "New Laptop",
			Description = "My new laptop",
			Categories = [2, 3]
		};
		
		var userId = Guid.NewGuid();
		
		var schoolEssentials = new Category
		{
			Id = 1,
			Label = "School essentials",
			UserId = userId
		};
		
		var itemId = Guid.NewGuid();
		var item = new Item
		{
			Id = itemId,
			Name = "Laptop",
			ItemCategories = [
				new ()
				{
					ItemId = itemId,
					CategoryId = 1,
					Category = schoolEssentials,
					UserId = userId
				}
			],
			UserId = userId
		};
		
		var deskEssentials = new Category
		{
			Id = 2,
			Label = "Desk Essentials",
			UserId = userId
		};
		
		var electronics = new Category
		{
			Id = 3,
			Label = "Electronics",
			UserId = userId
		};
		
		var newCategoriesIds = new List<int> {2, 3};
		var oldCategoriesIds = new List<int> {1};
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockItemsRepository.Setup (c => c.ExistsAsync (itemId, userId))
							.ReturnsAsync (true);
		mockCategoriesRepository.Setup (c => c.AllExistAsync (userId, newCategoriesIds))
								.ReturnsAsync (true);
		mockItemsRepository.Setup (c => c.UpdateAsync (itemId, userId, It.IsAny<Item>()))
							.ReturnsAsync (
								(Guid itemId, Guid userId, Item item) =>
								{
									item.Id = itemId;
									item.Name = itemUpdateDto.Name;
									item.Description = itemUpdateDto.Description;
									item.Quantity = 1;
									item.Unit = Modules.ItemsCatalogue.Models.Unit.Unit;
									item.UserId = userId;
									return item;
								}
							);
		mockItemsCategoriesRepository.Setup (ic => ic.GetAllForItemAsync (itemId, userId))
										.ReturnsAsync ([
											new ItemCategory
											{
												ItemId = itemId,
												CategoryId = 1,
												Category = schoolEssentials,
												UserId = userId
											}
										]);
		mockItemsCategoriesRepository.Setup (ic => ic.BulkDeleteForItemAsync (itemId, userId, oldCategoriesIds))
										.ReturnsAsync (1);
		mockItemsCategoriesRepository.Setup (ic => ic.BulkInsertAsync (It.IsAny<IEnumerable<ItemCategory>>()))
										.ReturnsAsync (2);
		mockItemsRepository.Setup (i => i.GetOneAsync (itemId, userId))
							.ReturnsAsync (
								new Item
								{
									Id = itemId,
									Name = "New Laptop",
									Description = "My new laptop",
									Quantity = 1,
									Unit = Modules.ItemsCatalogue.Models.Unit.Unit,
									ItemCategories = [
										new ()
										{
											ItemId = itemId,
											CategoryId = 2,
											Category = deskEssentials,
											UserId = userId
										},
										new ()
										{
											ItemId = itemId,
											CategoryId = 3,
											Category = electronics,
											UserId = userId
										}
									],
									UserId = userId
								}
							);
							
		var result = await service.UpdateAsync (itemId, itemUpdateDto);
		
		result.Should().NotBeNull();
		result.Name.Should().Be ("New Laptop");
		result.Description.Should().Be ("My new laptop");
		result.Categories.Should().HaveCount (2);
		result.Categories.Should().Contain (c => c.Id == 2 && c.Label == "Desk Essentials");
		result.Categories.Should().Contain (c => c.Id == 3 && c.Label == "Electronics");
		
		mockCurrentUser.Verify (u => u.Id, Times.Exactly (7 + itemUpdateDto.Categories.Count));
		mockItemsRepository.Verify (c => c.ExistsAsync (itemId, userId), Times.Once());
		mockCategoriesRepository.Verify (c => c.AllExistAsync (userId, newCategoriesIds), Times.Once());
		mockItemsRepository.Verify (
			c => c.UpdateAsync (
				itemId, userId, It.Is<Item> (i => i.Name == "New Laptop" && i.Description == "My new laptop")
			),
			Times.Once()
		);
		mockItemsCategoriesRepository.Verify (ic => ic.GetAllForItemAsync (itemId, userId), Times.Once());
		mockItemsCategoriesRepository.Verify (ic => ic.BulkDeleteForItemAsync (itemId, userId, oldCategoriesIds), Times.Once());
		mockItemsCategoriesRepository.Verify (
			ic => ic.BulkInsertAsync (
				It.Is<IEnumerable<ItemCategory>> (
					categories => categories.Count() == 2
									&& categories.Any (c => c.ItemId == itemId && c.CategoryId == 2)
									&& categories.Any (c => c.ItemId == itemId && c.CategoryId == 3)
				)
			),
			Times.Once()
		);
		mockItemsRepository.Verify (i => i.GetOneAsync (itemId, userId), Times.Once());
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteItemWithNonExistingItemIdShouldReturnFalse ()
	{
		var userId = Guid.NewGuid();
		var nonExistentItemId = Guid.NewGuid();
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockItemsCategoriesRepository.Setup (ic => ic.DeleteAllForItemAsync (nonExistentItemId, userId))
									.ReturnsAsync (0);
		mockItemsCompositionsRepository.Setup (ic => ic.DeleteAllForItemAsync (nonExistentItemId, userId))
									.ReturnsAsync ([]);
		mockItemsRepository.Setup (i => i.DeleteAsync (nonExistentItemId, userId))
							.ReturnsAsync (false);
							
		var result = await service.DeleteAsync (nonExistentItemId);
		
		result.Should().BeFalse();
		
		mockCurrentUser.Verify (u => u.Id, Times.Exactly (3));
		mockItemsRepository.Verify (i => i.DeleteAsync (nonExistentItemId, userId), Times.Once());
		mockItemsCategoriesRepository.Verify (i => i.DeleteAllForItemAsync (nonExistentItemId, userId), Times.Once());
		mockItemsCompositionsRepository.Verify (i => i.DeleteAllForItemAsync (nonExistentItemId, userId), Times.Once());
		VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteItemWithExistingItemIdShouldReturnTrue ()
	{
		var itemId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		List<Guid> componentIds = [Guid.NewGuid(), Guid.NewGuid()];
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockItemsCategoriesRepository.Setup (ic => ic.DeleteAllForItemAsync (itemId, userId))
									.ReturnsAsync (2);
		mockItemsCompositionsRepository.Setup (ic => ic.DeleteAllForItemAsync (itemId, userId))
										.ReturnsAsync (componentIds);
		mockItemsRepository.Setup (i => i.DeleteAsync (itemId, userId))
							.ReturnsAsync (true);
		mockItemsCompositionsRepository.Setup (c => c.GetNotUsedByAnyAsync (It.IsAny<IEnumerable<Guid>>(), userId))
										.ReturnsAsync (
											(IEnumerable<Guid> items, Guid userId) => items
										);
		mockItemsRepository.Setup (i => i.BulkDeleteAsync (It.IsAny<IEnumerable<Guid>>(), userId))
							.ReturnsAsync (true);
		
		var result = await service.DeleteAsync (itemId);
		
		result.Should().BeTrue();
		
		mockCurrentUser.Verify (u => u.Id, Times.Exactly (6));
		mockItemsRepository.Verify (i => i.DeleteAsync (itemId, userId), Times.Once());
		mockItemsCategoriesRepository.Verify (i => i.DeleteAllForItemAsync (itemId, userId), Times.Once());
		mockItemsCompositionsRepository.Verify (i => i.DeleteAllForItemAsync (itemId, userId), Times.Once());
		mockItemsCompositionsRepository.Verify (
			c => c.GetNotUsedByAnyAsync (
				It.Is<IEnumerable<Guid>> (
					ids => ids.ToHashSet().SetEquals (componentIds)
				),
				userId
			),
			Times.Once()
		);
		mockItemsRepository.Verify (
			i => i.BulkDeleteAsync (
				It.Is<IEnumerable<Guid>> (
					ids => ids.ToHashSet().SetEquals (componentIds)
				),
				userId
			),
			Times.Once()
		);
		
		VerifyDeletedEventEmitted (componentIds);
		VerifyNoOtherCalls();
	}
}