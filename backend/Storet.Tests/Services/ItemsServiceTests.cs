using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Storet.API.Contracts.Items;
using Storet.API.Exceptions;
using Storet.API.Mappers;
using Storet.API.Models;
using Storet.API.Repositories.Categories;
using Storet.API.Repositories.Items;
using Storet.API.Repositories.ItemsCategories;
using Storet.API.Services.Items;
using Storet.API.Utils;

namespace Storet.Tests.Services;

public class ItemsServiceTests
{
	private readonly Mock<IItemsRepository> mockItemsRepository;
	private readonly Mock<IItemsCategoriesRepository> mockItemsCategoriesRepository;
	private readonly Mock<ICategoriesRepository> mockCategoriesRepository;
	private readonly ItemsService service;
	private readonly IMapper mapper;
	private readonly ILoggerFactory loggerFactory;
	
	public ItemsServiceTests ()
	{
		mockItemsRepository = new Mock<IItemsRepository>();
		mockItemsCategoriesRepository = new Mock<IItemsCategoriesRepository>();
		mockCategoriesRepository = new Mock<ICategoriesRepository>();
		loggerFactory = new LoggerFactory();
		
		var config = new MapperConfiguration (cfg => cfg.AddProfile<MappingProfile>(), loggerFactory);
		mapper = config.CreateMapper();
		service = new ItemsService (mockItemsRepository.Object, mockCategoriesRepository.Object, mockItemsCategoriesRepository.Object, mapper);
	}
	
	[Fact]
	public async Task CreateItemWithValidDataShouldCreateItem ()
	{
		var categoryIds = new List<int> {1};
		
		var itemCreateDto = new ItemInsertRequest
		{
			Name = "Steak",
			Categories = [1]
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
		mockItemsRepository.Setup (i => i.GetOneAsync (generatedItemId))
							.ReturnsAsync (
								(Item i) =>
								{
									i.Id = generatedItemId;
									i.ItemCategories = [
										new ()
										{
											CategoryId = 1,
											Category = category,
											ItemId = generatedItemId
										}
									];
									i.Name = "Steak";
									return i;
								}
							);
		
		var result = await service.InsertAsync (itemCreateDto);
		result.Should().NotBeNull();
		result.Id.Should().NotBeEmpty();
		result.Name.Should().Be ("Steak");
		
		mockCategoriesRepository.Verify (
			c => c.AllExistAsync (categoryIds),
			Times.Once()
		);
		mockItemsRepository.Verify (
			i => i.InsertAsync (It.Is<Item> (i => i.Name == "Steak")),
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
			i => i.GetOneAsync (generatedItemId),
			Times.Once()
		);
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateItemWithNonExistentCategoryShouldThrowNotFoundException ()
	{
		var categoryIds = new List<int> {999};
		var itemCreateDto = new ItemInsertRequest
		{
			Name = "Steak",
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
	}
	
	[Fact]
	public async Task CreateItemWithInvalidNumberOfCategoriesShouldThrowArgumentException ()
	{
		var itemCreateDto = new ItemInsertRequest
		{
			Name = "Steak",
			Categories = []
		};
		
		Func<Task> act = async () => await service.InsertAsync (itemCreateDto);
		
		await act.Should().ThrowAsync<ArgumentException>()
							.WithMessage ("Item must be created with at least 1 category");
							
		mockCategoriesRepository.VerifyNoOtherCalls();
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
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
			new () {Id = Guid.NewGuid(), Name = "Apple"},
			new () {Id = Guid.NewGuid(), Name = "Banana"},
			new () {Id = Guid.NewGuid(), Name = "Chicken breasts"},
			new () {Id = Guid.NewGuid(), Name = "Chocolate"},
			new () {Id = Guid.NewGuid(), Name = "Lemon"},
			new () {Id = Guid.NewGuid(), Name = "Orange"},
			new () {Id = Guid.NewGuid(), Name = "Rice"},
			new () {Id = Guid.NewGuid(), Name = "Steak"},
			new () {Id = Guid.NewGuid(), Name = "Sugar"},
			new () {Id = Guid.NewGuid(), Name = "Tea"},
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
	}
	
	[Fact]
	public async Task GetAllItemsWithValidQueryOnSecondPageShouldReturnList ()
	{
		var query = new Query<string>
		{
			PageSize = 5,
			Key = "Lemon"
		};
		
		var items = new List<Item>
		{
			new () {Id = Guid.NewGuid(), Name = "Apple"},
			new () {Id = Guid.NewGuid(), Name = "Banana"},
			new () {Id = Guid.NewGuid(), Name = "Chicken breasts"},
			new () {Id = Guid.NewGuid(), Name = "Chocolate"},
			new () {Id = Guid.NewGuid(), Name = "Lemon"},
			new () {Id = Guid.NewGuid(), Name = "Orange"},
			new () {Id = Guid.NewGuid(), Name = "Rice"},
			new () {Id = Guid.NewGuid(), Name = "Steak"},
			new () {Id = Guid.NewGuid(), Name = "Sugar"},
			new () {Id = Guid.NewGuid(), Name = "Tea"},
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
		var newCategories = new List<ItemCategory>
		{
			new () {ItemId = itemId, CategoryId = 2},
			new () {ItemId = itemId, CategoryId = 3}
		};
		var oldCategoriesIds = new List<int> {1};
		
		mockItemsRepository.Setup (c => c.ExistsAsync (itemId))
							.ReturnsAsync (true);
		mockCategoriesRepository.Setup (c => c.AllExistAsync (newCategoriesIds))
								.ReturnsAsync (true);
		mockItemsRepository.Setup (c => c.UpdateAsync (itemId, It.IsAny<Item>()))
							.ReturnsAsync (
								(Guid itemId, Item item) =>
								{
									item.Id = itemId;
									item.Name = itemUpdateDto.Name;
									item.Description = itemUpdateDto.Description;
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
		mockItemsCategoriesRepository.Setup (ic => ic.BulkInsertAsync (newCategories))
										.ReturnsAsync (2);
		mockItemsRepository.Setup (i => i.GetOneAsync (itemId))
							.ReturnsAsync (
								(Item item) =>
								{
									item.Id = itemId;
									item.Name = "Laptop";
									item.Name = "New Laptop";
									item.Description = "My new laptop";
									item.ItemCategories = [
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
									];
									return item;
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
		mockItemsRepository.Verify (
			c => c.UpdateAsync (
				itemId, It.Is<Item> (i => i.Name == "New Laptop" && i.Description == "My new laptop")
			),
			Times.Once()
		);
		mockItemsCategoriesRepository.Verify (ic => ic.GetAllForItemAsync (itemId), Times.Once());
		mockItemsCategoriesRepository.Verify (ic => ic.BulkDeleteForItemAsync (itemId, oldCategoriesIds), Times.Once());
		mockItemsCategoriesRepository.Verify (ic => ic.BulkInsertAsync (newCategories), Times.Once());
		mockItemsRepository.Verify (i => i.GetOneAsync (itemId), Times.Once());
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteItemWithNonExistingItemIdShouldReturnFalse ()
	{
		var nonExistentItemId = Guid.NewGuid();
		mockItemsCategoriesRepository.Setup (ic => ic.DeleteAllForItemAsync (nonExistentItemId))
									.ReturnsAsync (0);
		mockItemsRepository.Setup (i => i.DeleteAsync (nonExistentItemId))
							.ReturnsAsync (false);
							
		var result = await service.DeleteAsync (nonExistentItemId);
		
		result.Should().BeFalse();
		
		mockItemsRepository.Verify (i => i.DeleteAsync (nonExistentItemId), Times.Once());
		mockItemsCategoriesRepository.Verify (i => i.DeleteAllForItemAsync (nonExistentItemId), Times.Once());
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteItemWithExistingItemIdShouldReturnTrue ()
	{
		var itemId = Guid.NewGuid();
		mockItemsCategoriesRepository.Setup (ic => ic.DeleteAllForItemAsync (itemId))
									.ReturnsAsync (2);
		mockItemsRepository.Setup (i => i.DeleteAsync (itemId))
							.ReturnsAsync (true);
							
		var result = await service.DeleteAsync (itemId);
		
		result.Should().BeTrue();
		
		mockItemsRepository.Verify (i => i.DeleteAsync (itemId), Times.Once());
		mockItemsCategoriesRepository.Verify (i => i.DeleteAllForItemAsync (itemId), Times.Once());
		mockItemsRepository.VerifyNoOtherCalls();
		mockItemsCategoriesRepository.VerifyNoOtherCalls();
		mockCategoriesRepository.VerifyNoOtherCalls();
	}
}