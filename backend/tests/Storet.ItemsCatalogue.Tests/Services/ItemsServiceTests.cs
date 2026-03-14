using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Storet.API.Core.Exceptions;
using Storet.API.Core.Utils;
using Storet.API.ItemsCatalogue.Contracts.Items;
using Storet.API.ItemsCatalogue.Mappers;
using Storet.API.ItemsCatalogue.Models;
using Storet.API.ItemsCatalogue.Repositories.Categories;
using Storet.API.ItemsCatalogue.Repositories.Items;
using Storet.API.ItemsCatalogue.Repositories.ItemsCategories;
using Storet.API.ItemsCatalogue.Repositories.ItemsCompositions;
using Storet.API.ItemsCatalogue.Services.Items;

namespace Storet.ItemsCatalogue.Tests.Services;

public class ItemsServiceTests
{
	private readonly Mock<IItemsRepository> mockItemsRepository;
	private readonly Mock<IItemsCategoriesRepository> mockItemsCategoriesRepository;
	private readonly Mock<IItemsCompositionRepository> mockItemsCompositionsRepository;
	private readonly Mock<ICategoriesRepository> mockCategoriesRepository;
	private readonly ItemsService service;
	private readonly IMapper mapper;
	private readonly ILoggerFactory loggerFactory;
	
	public ItemsServiceTests ()
	{
		mockItemsRepository = new Mock<IItemsRepository>();
		mockItemsCategoriesRepository = new Mock<IItemsCategoriesRepository>();
		mockCategoriesRepository = new Mock<ICategoriesRepository>();
		mockItemsCompositionsRepository = new Mock<IItemsCompositionRepository>();
		loggerFactory = new LoggerFactory();
		
		var config = new MapperConfiguration (cfg => cfg.AddProfile<MappingProfile>(), loggerFactory);
		mapper = config.CreateMapper();
		service = new ItemsService (mockItemsRepository.Object, mockCategoriesRepository.Object, mockItemsCategoriesRepository.Object, mockItemsCompositionsRepository.Object, mapper);
	}
	
	[Fact]
	public async Task CreateItemWithZeroOrLessQuantityShouldThrowArgumentException ()
	{
		var item = new ItemInsertRequest
		{
			Name = "Chicken breasts",
			Unit = Unit.Gram
		};
		
		Func<Task> act = async () => await service.InsertAsync (item);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Item must have a positive quantity");
		
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateItemWithValidDataShouldCreateItem ()
	{
		var categoryIds = new List<int> {1};
		
		var itemCreateDto = new ItemInsertRequest
		{
			Name = "Tea Box",
			Quantity = 1,
			Unit = Unit.Unit,
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
			Label = "Food"
		};
		
		mockCategoriesRepository.Setup (c => c.AllExistAsync (categoryIds))
								.ReturnsAsync (true);
		Guid generatedItemId = Guid.NewGuid();
		mockItemsRepository.Setup (i => i.InsertAsync (It.IsAny<Item>()))
							.ReturnsAsync (
								(Item i) =>
								{
									i.Id = generatedItemId;
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
										Unit = Unit.Unit
									}
								]
							);
		mockItemsCompositionsRepository.Setup (c => c.BulkInsertAsync (It.IsAny<IEnumerable<ItemComposition>>()))
										.ReturnsAsync (1);
		mockItemsRepository.Setup (i => i.GetOneAsync (generatedItemId))
							.ReturnsAsync (
								new Item
								{
									Id = generatedItemId,
									ItemCategories = [
										new ()
										{
											CategoryId = 1,
											Category = category,
											ItemId = generatedItemId
										}
									],
									Name = "Tea Box"
								}
							);
		
		var result = await service.InsertAsync (itemCreateDto);
		result.Should().NotBeNull();
		result.Id.Should().NotBeEmpty();
		result.Name.Should().Be ("Tea Box");
		
		mockCategoriesRepository.Verify (
			c => c.AllExistAsync (categoryIds),
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
			i => i.GetOneAsync (generatedItemId),
			Times.Once()
		);
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateItemWithNonExistentCategoryShouldThrowNotFoundException ()
	{
		var categoryIds = new List<int> {999};
		var itemCreateDto = new ItemInsertRequest
		{
			Name = "Steak",
			Quantity = 400,
			Unit = Unit.Gram,
			Categories = [999]
		};
		
		mockCategoriesRepository.Setup (c => c.AllExistAsync (categoryIds))
								.ReturnsAsync (false);
								
		Func<Task> act = async () => await service.InsertAsync (itemCreateDto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ("Category with ID 999 was not found");
							
		mockCategoriesRepository.Verify (c => c.AllExistAsync (categoryIds), Times.Once());
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateItemWithInvalidNumberOfCategoriesShouldThrowArgumentException ()
	{
		var itemCreateDto = new ItemInsertRequest
		{
			Name = "Steak",
			Quantity = 400,
			Unit = Unit.Gram,
			Categories = []
		};
		
		Func<Task> act = async () => await service.InsertAsync (itemCreateDto);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Item must be created with at least 1 category");
							
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateItemWithNonExistingComponentShouldThrowNotFoundException ()
	{
		var nonExistentComponentId = Guid.NewGuid();
		
		var itemCreateDto = new ItemInsertRequest
		{
			Name = "Tea Box",
			Quantity = 1,
			Unit = Unit.Unit,
			Categories = [1],
			Components = [
				new () {Id = nonExistentComponentId, Quantity = 10}
			]
		};
		
		mockCategoriesRepository.Setup (c => c.AllExistAsync (It.IsAny<IEnumerable<int>>()))
								.ReturnsAsync (true);
		mockItemsRepository.Setup (i => i.AllExistAsync (It.IsAny<IEnumerable<Guid>>()))
							.ReturnsAsync (false);
		
		Func<Task> act = async () => await service.InsertAsync (itemCreateDto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ($"Item with ID {nonExistentComponentId} was not found");
							
		mockCategoriesRepository.Verify (
			c => c.AllExistAsync (It.Is<IEnumerable<int>> (c => c.Contains (1))),
			Times.Once()
		);
		mockItemsRepository.Verify (
			i => i.AllExistAsync (It.Is<IEnumerable<Guid>> (i => i.Contains (nonExistentComponentId))),
			Times.Once()
		);
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
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
							
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllItemsWithValidQueryOnFirstPageShouldReturnList ()
	{
		var query = new Query<string>
		{
			PageSize = 5
		};
		
		var items = new List<Item>
		{
			new () {Id = Guid.NewGuid(), Name = "Apple", Quantity = 1, Unit = Unit.Unit},
			new () {Id = Guid.NewGuid(), Name = "Banana", Quantity = 1, Unit = Unit.Unit},
			new () {Id = Guid.NewGuid(), Name = "Chicken breasts", Quantity = 500, Unit = Unit.Gram},
			new () {Id = Guid.NewGuid(), Name = "Chocolate", Quantity = 1, Unit = Unit.Unit},
			new () {Id = Guid.NewGuid(), Name = "Lemon", Quantity = 1, Unit = Unit.Unit},
			new () {Id = Guid.NewGuid(), Name = "Orange", Quantity = 1, Unit = Unit.Unit},
			new () {Id = Guid.NewGuid(), Name = "Rice", Quantity = 100, Unit = Unit.Gram},
			new () {Id = Guid.NewGuid(), Name = "Steak", Quantity = 500, Unit = Unit.Gram},
			new () {Id = Guid.NewGuid(), Name = "Sugar", Quantity = 100, Unit = Unit.Gram},
			new () {Id = Guid.NewGuid(), Name = "Tea", Quantity = 1, Unit = Unit.Unit},
		};
		
		mockItemsRepository.Setup (i => i.GetAllAsync (It.IsAny<Query<string>>()))
							.ReturnsAsync (items.Take (query.PageSize + 1));
		mockItemsRepository.Setup (i => i.GetPreviousKeyAsync (It.IsAny<Query<string>>()))
							.ReturnsAsync ((string?) null);
		
		var result = await service.GetAllAsync (query);
		
		result.Should().NotBeNull();
		result.HasNext.Should().BeTrue();
		result.HasPrevious.Should().BeFalse();
		result.Next.Should().Be ("Orange");
		result.Previous.Should().BeNull();
		result.Data.Should().HaveCount (5);
		result.Data.Should().BeInAscendingOrder (i => i.Name);
		
		mockItemsRepository.Verify (
			i => i.GetAllAsync (It.Is<Query<string>> (q => q.Key == null && q.PageSize == 5)),
			Times.Once()
		);
		mockItemsRepository.Verify (
			i => i.GetPreviousKeyAsync (It.Is<Query<string>> (q => q.Key == null && q.PageSize == 5)),
			Times.Once()
		);
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetAllItemsWithValidQueryOnSecondPageShouldReturnList ()
	{
		var query = new Query<string>
		{
			PageSize = 5,
			Key = "Orange"
		};
		
		var items = new List<Item>
		{
			new () {Id = Guid.NewGuid(), Name = "Apple", Quantity = 1, Unit = Unit.Unit},
			new () {Id = Guid.NewGuid(), Name = "Banana", Quantity = 1, Unit = Unit.Unit},
			new () {Id = Guid.NewGuid(), Name = "Chicken breasts", Quantity = 500, Unit = Unit.Gram},
			new () {Id = Guid.NewGuid(), Name = "Chocolate", Quantity = 1, Unit = Unit.Unit},
			new () {Id = Guid.NewGuid(), Name = "Lemon", Quantity = 1, Unit = Unit.Unit},
			new () {Id = Guid.NewGuid(), Name = "Orange", Quantity = 1, Unit = Unit.Unit},
			new () {Id = Guid.NewGuid(), Name = "Rice", Quantity = 100, Unit = Unit.Gram},
			new () {Id = Guid.NewGuid(), Name = "Steak", Quantity = 500, Unit = Unit.Gram},
			new () {Id = Guid.NewGuid(), Name = "Sugar", Quantity = 100, Unit = Unit.Gram},
			new () {Id = Guid.NewGuid(), Name = "Tea", Quantity = 1, Unit = Unit.Unit},
		};
		
		mockItemsRepository.Setup (i => i.GetAllAsync (It.IsAny<Query<string>>()))
							.ReturnsAsync (items.Where(i => String.Compare (i.Name, query.Key) >= 0).Take (query.PageSize + 1));
		mockItemsRepository.Setup (i => i.GetPreviousKeyAsync (It.IsAny<Query<string>>()))
							.ReturnsAsync ("Apple");
		
		var result = await service.GetAllAsync (query);
		
		result.Should().NotBeNull();
		result.HasNext.Should().BeFalse();
		result.HasPrevious.Should().BeTrue();
		result.Next.Should().BeNull();
		result.Previous.Should().Be ("Apple");
		result.Data.Should().HaveCount (5);
		result.Data.Should().BeInAscendingOrder (i => i.Name);
		
		mockItemsRepository.Verify (
			i => i.GetAllAsync (It.Is<Query<string>> (q => q.Key == "Orange" && q.PageSize == 5)),
			Times.Once()
		);
		mockItemsRepository.Verify (
			i => i.GetPreviousKeyAsync (It.Is<Query<string>> (q => q.Key == "Orange" && q.PageSize == 5)),
			Times.Once()
		);
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task GetItemWithNonExistentIdShouldReturnNull ()
	{
		var nonExistentId = Guid.NewGuid();
		mockItemsRepository.Setup (i => i.GetOneAsync (nonExistentId))
							.ReturnsAsync ((Item?) null);
		
		var result = await service.GetOneAsync (nonExistentId);
		
		result.Should().BeNull();
	}
	
	[Fact]
	public async Task GetItemWithExistingIdShouldReturnItemWithCategories ()
	{
		var itemId = Guid.NewGuid();
		var item = new Item
		{
			Id = itemId,
			Name = "Laptop",
			Quantity = 1,
			Unit = Unit.Unit,
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
			]
		};
		
		mockItemsRepository.Setup (i => i.GetOneAsync (itemId))
							.ReturnsAsync (item);
		
		var result = await service.GetOneAsync (itemId);
		
		result.Should().NotBeNull();
		result.Name.Should().Be ("Laptop");
		result.Quantity.Should().Be (1);
		result.Unit.Should().Be (Unit.Unit);
		result.Categories.Should().HaveCount (1);
		result.Categories.First().Id.Should().Be (1);
		result.Categories.First().Label.Should().Be ("Electronics");
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
		
		mockItemsRepository.Setup (i => i.ExistsAsync (nonExistentItemId))
							.ReturnsAsync (false);
		
		var result = await service.UpdateAsync (nonExistentItemId, itemUpdateDto);
		
		result.Should().BeNull();
		
		mockItemsRepository.Verify (i => i.ExistsAsync (nonExistentItemId), Times.Once());
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
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
		
		mockItemsRepository.Setup (i => i.ExistsAsync (itemId))
							.ReturnsAsync (true);
		mockCategoriesRepository.Setup (c => c.AllExistAsync (newCategoriesIds))
								.ReturnsAsync (false);
		
		Func<Task> act = async () => await service.UpdateAsync (itemId, itemUpdateDto);
		
		await act.Should().ThrowAsync<EntityNotFoundException>()
							.WithMessage ("Category with ID 1,2 was not found");
		
		mockItemsRepository.Verify (i => i.ExistsAsync (itemId), Times.Once());
		mockCategoriesRepository.Verify (
			i => i.AllExistAsync (
				It.Is<IEnumerable<int>> (
					ids => ids.Contains (1) && ids.Contains (2)
				)
			),
			Times.Once()
		);
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateItemWithValidDataShouldUpdateAndReturnItemWithCategories ()
	{
		var itemUpdateDto = new ItemUpdateRequest
		{
			Name = "New Laptop",
			Description = "My new laptop",
			Quantity = 1,
			Unit = Unit.Unit,
			Categories = [2, 3]
		};
		
		var schoolEssentials = new Category
		{
			Id = 1,
			Label = "School essentials"
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
					Category = schoolEssentials
				}
			]
		};
		
		var deskEssentials = new Category
		{
			Id = 2,
			Label = "Desk Essentials"
		};
		
		var electronics = new Category
		{
			Id = 3,
			Label = "Electronics"
		};
		
		var newCategoriesIds = new List<int> {2, 3};
		var oldCategoriesIds = new List<int> {1};
		
		mockItemsRepository.Setup (c => c.ExistsAsync (itemId))
							.ReturnsAsync (true);
		mockCategoriesRepository.Setup (c => c.AllExistAsync (newCategoriesIds))
								.ReturnsAsync (true);
		mockItemsCompositionsRepository.Setup (c => c.GetAllForItemAsync (itemId))
										.ReturnsAsync ([]);
		mockItemsRepository.Setup (c => c.UpdateAsync (itemId, It.IsAny<Item>()))
							.ReturnsAsync (
								(Guid itemId, Item item) =>
								{
									item.Id = itemId;
									item.Name = itemUpdateDto.Name;
									item.Description = itemUpdateDto.Description;
									item.Quantity = 1;
									item.Unit = Unit.Unit;
									return item;
								}
							);
		mockItemsCategoriesRepository.Setup (ic => ic.GetAllForItemAsync (itemId))
										.ReturnsAsync ([
											new ItemCategory
											{
												ItemId = itemId,
												CategoryId = 1,
												Category = schoolEssentials
											}
										]);
		mockItemsCategoriesRepository.Setup (ic => ic.BulkDeleteForItemAsync (itemId, oldCategoriesIds))
										.ReturnsAsync (1);
		mockItemsCategoriesRepository.Setup (ic => ic.BulkInsertAsync (It.IsAny<IEnumerable<ItemCategory>>()))
										.ReturnsAsync (2);
		mockItemsRepository.Setup (i => i.GetOneAsync (itemId))
							.ReturnsAsync (
								new Item
								{
									Id = itemId,
									Name = "New Laptop",
									Description = "My new laptop",
									Quantity = 1,
									Unit = Unit.Unit,
									ItemCategories = [
										new ()
										{
											ItemId = itemId,
											CategoryId = 2,
											Category = deskEssentials
										},
										new ()
										{
											ItemId = itemId,
											CategoryId = 3,
											Category = electronics
										}
									]
								}
							);
							
		var result = await service.UpdateAsync (itemId, itemUpdateDto);
		
		result.Should().NotBeNull();
		result.Name.Should().Be ("New Laptop");
		result.Description.Should().Be ("My new laptop");
		result.Categories.Should().HaveCount (2);
		result.Categories.Should().Contain (c => c.Id == 2 && c.Label == "Desk Essentials");
		result.Categories.Should().Contain (c => c.Id == 3 && c.Label == "Electronics");
		
		mockItemsRepository.Verify (c => c.ExistsAsync (itemId), Times.Once());
		mockCategoriesRepository.Verify (c => c.AllExistAsync (newCategoriesIds), Times.Once());
		mockItemsCompositionsRepository.Verify (c => c.GetAllForItemAsync (itemId), Times.Once());
		mockItemsRepository.Verify (
			c => c.UpdateAsync (
				itemId, It.Is<Item> (i => i.Name == "New Laptop" && i.Description == "My new laptop")
			),
			Times.Once()
		);
		mockItemsCategoriesRepository.Verify (ic => ic.GetAllForItemAsync (itemId), Times.Once());
		mockItemsCategoriesRepository.Verify (ic => ic.BulkDeleteForItemAsync (itemId, oldCategoriesIds), Times.Once());
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
		mockItemsRepository.Verify (i => i.GetOneAsync (itemId), Times.Once());
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateItemWithNewComponentsShouldUpdateAndReturnItemWithDetails ()
	{
		var sugarCubeId = Guid.NewGuid();
		var teaBagId = Guid.NewGuid();
		var teaBag = new Item
		{
			Id = teaBagId,
			Name = "Tea Bag",
			Quantity = 1,
			Unit = Unit.Unit,
		};
		
		var itemUpdateDto = new ItemUpdateRequest
		{
			Components = [
				new ()
				{
					Id = teaBagId,
					Quantity = 10
				}
			]
		};
		
		var food = new Category
		{
			Id = 1,
			Label = "Food"
		};
		
		var itemId = Guid.NewGuid();
		
		mockItemsRepository.Setup (i => i.ExistsAsync (itemId))
							.ReturnsAsync (true);
		mockItemsCompositionsRepository.Setup (c => c.GetAllForItemAsync (It.IsAny<Guid>()))
										.ReturnsAsync ([
											new ItemComposition
											{
												ParentItemId = itemId,
												ComponentItemId = sugarCubeId,
												Quantity = 10
											}
										]);
		mockItemsRepository.Setup (i => i.AllExistAsync (It.IsAny<IEnumerable<Guid>>()))
							.ReturnsAsync (true);
		mockItemsRepository.Setup (c => c.UpdateAsync (itemId, It.IsAny<Item>()))
							.ReturnsAsync (
								(Guid itemId, Item item) =>
								{
									item.Id = itemId;
									item.Quantity = 1;
									item.Unit = Unit.Unit;
									return item;
								}
							);
		mockItemsCompositionsRepository.Setup (c => c.BulkDeleteForItemAsync (It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
										.ReturnsAsync (1);
		mockItemsCompositionsRepository.Setup (c => c.BulkInsertAsync (It.IsAny<IEnumerable<ItemComposition>>()))
										.ReturnsAsync (1);
		mockItemsRepository.Setup (i => i.GetOneAsync (itemId))
							.ReturnsAsync (
								new Item
								{
									Id = itemId,
									Name = "Tea Box",
									Quantity = 1,
									Unit = Unit.Unit,
									ItemCategories = [
										new ()
										{
											ItemId = itemId,
											CategoryId = 1,
											Category = food
										}
									],
									Components = [
										new ()
										{
											ParentItemId = itemId,
											ComponentItemId = teaBagId,
											ComponentItem = teaBag,
											Quantity = 10
										}
									]
								}
							);
		
		var result = await service.UpdateAsync (itemId, itemUpdateDto);
		
		result.Should().NotBeNull();
		result.Name.Should().Be ("Tea Box");
		result.Quantity.Should().Be (1);
		result.Unit.Should().Be (Unit.Unit);
		result.Components.Should().HaveCount (1);
		result.Components.Should().Contain (c => c.Id == teaBagId);
		
		mockItemsRepository.Verify (i => i.ExistsAsync (itemId), Times.Once());
		mockItemsCompositionsRepository.Verify (c => c.GetAllForItemAsync (It.Is<Guid> (id => id == itemId)), Times.Once());
		mockItemsRepository.Verify (i => i.AllExistAsync (It.Is<IEnumerable<Guid>> (ids => ids.Contains (teaBagId))), Times.Once());
		mockItemsRepository.Verify (c => c.UpdateAsync (itemId, It.IsAny<Item>()), Times.Once());
		mockItemsCompositionsRepository.Verify (c => c.BulkDeleteForItemAsync (It.IsAny<Guid>(), It.Is<IEnumerable<Guid>> (c => c.Count() == 1)), Times.Once());
		mockItemsCompositionsRepository.Verify (c => c.BulkInsertAsync (It.Is<IEnumerable<ItemComposition>> (c => c.Count() == 1)), Times.Once());
		mockItemsRepository.Verify (i => i.GetOneAsync (itemId), Times.Once());
		
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateItemWithUpdatedComponentsQuantitiesOrUnitsShouldUpdateAndReturnItemWithDetails ()
	{
		var teaBagId = Guid.NewGuid();
		var teaBag = new Item
		{
			Id = teaBagId,
			Name = "Tea Bag",
			Quantity = 1,
			Unit = Unit.Unit
		};
		
		var itemUpdateDto = new ItemUpdateRequest
		{
			Components = [
				new ()
				{
					Id = teaBagId,
					Quantity = 20
				}
			]
		};
		
		var food = new Category
		{
			Id = 1,
			Label = "Food"
		};
		
		var itemId = Guid.NewGuid();
		
		mockItemsRepository.Setup (i => i.ExistsAsync (itemId))
							.ReturnsAsync (true);
		mockItemsCompositionsRepository.Setup (c => c.GetAllForItemAsync (It.IsAny<Guid>()))
										.ReturnsAsync ([
											new ItemComposition
											{
												ParentItemId = itemId,
												ComponentItemId = teaBagId,
												Quantity = 10
											}
										]);
		mockItemsRepository.Setup (i => i.AllExistAsync (It.IsAny<IEnumerable<Guid>>()))
							.ReturnsAsync (true);
		mockItemsRepository.Setup (c => c.UpdateAsync (itemId, It.IsAny<Item>()))
							.ReturnsAsync (
								(Guid itemId, Item item) =>
								{
									item.Id = itemId;
									item.Quantity = 1;
									item.Unit = Unit.Unit;
									return item;
								}
							);
		mockItemsCompositionsRepository.Setup (c => c.UpdateAsync (itemId, teaBagId, 20))
										.ReturnsAsync (true);
		mockItemsRepository.Setup (i => i.GetOneAsync (itemId))
							.ReturnsAsync (
								new Item
								{
									Id = itemId,
									Name = "Tea Box",
									Quantity = 1,
									Unit = Unit.Unit,
									ItemCategories = [
										new ()
										{
											ItemId = itemId,
											CategoryId = 1,
											Category = food
										}
									],
									Components = [
										new ()
										{
											ParentItemId = itemId,
											ComponentItemId = teaBagId,
											ComponentItem = teaBag,
											Quantity = 20
										}
									]
								}
							);
		
		var result = await service.UpdateAsync (itemId, itemUpdateDto);
		
		result.Should().NotBeNull();
		result.Name.Should().Be ("Tea Box");
		result.Quantity.Should().Be (1);
		result.Unit.Should().Be (Unit.Unit);
		result.Components.Should().HaveCount (1);
		result.Components.Should().Contain (c => c.Id == teaBagId && c.Quantity == 20);
		
		mockItemsRepository.Verify (i => i.ExistsAsync (itemId), Times.Once());
		mockItemsCompositionsRepository.Verify (c => c.GetAllForItemAsync (It.Is<Guid> (id => id == itemId)), Times.Once());
		mockItemsRepository.Verify (i => i.AllExistAsync (It.Is<IEnumerable<Guid>> (ids => ids.Contains (teaBagId))), Times.Once());
		mockItemsRepository.Verify (c => c.UpdateAsync (itemId, It.IsAny<Item>()), Times.Once());
		mockItemsCompositionsRepository.Verify (c => c.UpdateAsync (itemId, teaBagId, 20), Times.Once());
		mockItemsRepository.Verify (i => i.GetOneAsync (itemId), Times.Once());
		
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteItemWithNonExistingItemIdShouldReturnFalse ()
	{
		var nonExistentItemId = Guid.NewGuid();
		mockItemsCategoriesRepository.Setup (ic => ic.DeleteAllForItemAsync (nonExistentItemId))
									.ReturnsAsync (0);
		mockItemsCompositionsRepository.Setup (ic => ic.DeleteAllForItemAsync (nonExistentItemId))
									.ReturnsAsync (0);
		mockItemsRepository.Setup (i => i.DeleteAsync (nonExistentItemId))
							.ReturnsAsync (false);
							
		var result = await service.DeleteAsync (nonExistentItemId);
		
		result.Should().BeFalse();
		
		mockItemsRepository.Verify (i => i.DeleteAsync (nonExistentItemId), Times.Once());
		mockItemsCategoriesRepository.Verify (i => i.DeleteAllForItemAsync (nonExistentItemId), Times.Once());
		mockItemsCompositionsRepository.Verify (i => i.DeleteAllForItemAsync (nonExistentItemId), Times.Once());
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteItemWithExistingItemIdShouldReturnTrue ()
	{
		var itemId = Guid.NewGuid();
		mockItemsCategoriesRepository.Setup (ic => ic.DeleteAllForItemAsync (itemId))
									.ReturnsAsync (2);
		mockItemsCompositionsRepository.Setup (ic => ic.DeleteAllForItemAsync (itemId))
										.ReturnsAsync (2);
		mockItemsRepository.Setup (i => i.DeleteAsync (itemId))
							.ReturnsAsync (true);
							
		var result = await service.DeleteAsync (itemId);
		
		result.Should().BeTrue();
		
		mockItemsRepository.Verify (i => i.DeleteAsync (itemId), Times.Once());
		mockItemsCategoriesRepository.Verify (i => i.DeleteAllForItemAsync (itemId), Times.Once());
		mockItemsCompositionsRepository.Verify (i => i.DeleteAllForItemAsync (itemId), Times.Once());
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsCompositionsRepository.VerifyNoOtherCalls();
	}
}