using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storet.Modules.ItemsCatalogue.Data;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Modules.ItemsCatalogue.Repositories.ItemsCategories;
using Storet.Tests.Common.Repository;
using Xunit.Abstractions;

namespace Storet.ItemsCatalogue.Tests.Repositories;

public class ItemsCategoriesRepositoryTests : SqliteRepositoryTestsBase<StoretItemsCatalogueDbContext, IItemsCategoriesRepository>
{
	public ItemsCategoriesRepositoryTests (ITestOutputHelper output) : base (output) {}

	protected override ItemsCategoriesRepository InitRepository ()
	{
		return new ItemsCategoriesRepository (context);
	}
	
	protected override StoretItemsCatalogueDbContext InitDbContextWithOptions (DbContextOptions<StoretItemsCatalogueDbContext> options)
	{
		return new StoretItemsCatalogueDbContext (options);
	}

	[Fact]
	public async Task InsertAsyncWithExistingItemAndCategoryShouldAddToDatabase ()
	{
		var userId = Guid.NewGuid();
		
		var category = new Category
		{
			Label = "Electronics",
			UserId = userId
		};
		await context.Categories.AddAsync (category);
		
		var laptop = new Item
		{
			Name = "Laptop",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		var phone = new Item
		{
			Name = "Phone",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddRangeAsync (laptop, phone);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = category.Id, UserId = userId},
			new () {ItemId = phone.Id, CategoryId = category.Id, UserId = userId}
		};

		var result = await repository.BulkInsertAsync (itemCategories);
		
		result.Should().Be (2);
		
		var saved = await context.ItemsCategories
									.Where (i => i.CategoryId == category.Id && i.UserId == userId)
									.ToListAsync();
		saved.Should().NotBeNull();
		saved.Should().HaveCount (2);
		saved.Should().Contain (i => i.ItemId == laptop.Id && i.CategoryId == category.Id);
		saved.Should().Contain (i => i.ItemId == phone.Id && i.CategoryId == category.Id);
	}
	
	[Fact]
	public async Task InsertAsyncWithNonExistingItemShouldThrowException ()
	{
		var userId = Guid.NewGuid();
		
		var category = new Category
		{
			Label = "Electronics",
			UserId = userId
		};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();
		
		var nonExistentItemId = Guid.NewGuid();
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = nonExistentItemId, CategoryId = category.Id, UserId = userId}
		};

		Func<Task> act = async () => await repository.BulkInsertAsync (itemCategories);
		
		await act.Should().ThrowAsync<DbUpdateException>();
		
		var count = await context.ItemsCategories.CountAsync();
		count.Should().Be (0);
	}

	[Fact]
	public async Task InsertAsyncWithNonExistingCategoryShouldThrowException ()
	{
		var userId = Guid.NewGuid();
		
		var laptop = new Item
		{
			Name = "Laptop",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddAsync (laptop);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = 999, UserId = userId}
		};

		Func<Task> act = async () => await repository.BulkInsertAsync (itemCategories);
		
		await act.Should().ThrowAsync<DbUpdateException>();
		
		var count = await context.ItemsCategories.CountAsync();
		count.Should().Be (0);
	}
	
	[Fact]
	public async Task InsertAsyncWithExistingRelationshipShouldThrowException ()
	{
		var userId = Guid.NewGuid();
		var electronics = new Category
		{
			Label = "Electronics",
			UserId = userId
		};
		await context.Categories.AddAsync (electronics);
		
		var laptop = new Item
		{
			Name = "Laptop",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddAsync (laptop);
		await context.SaveChangesAsync();
		
		var laptopIsElectronics = new ItemCategory
		{
			ItemId = laptop.Id,
			CategoryId = electronics.Id,
			UserId = userId
		};
		await context.ItemsCategories.AddAsync (laptopIsElectronics);
		
		await context.SaveChangesAsync();
		
		Func<Task> act = async () => await repository.BulkInsertAsync ([laptopIsElectronics]);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task DeleteAllForItemAsyncWithExistingItemShouldDeleteFromDatabase ()
	{
		var userId = Guid.NewGuid();
		
		var electronics = new Category
		{
			Label = "Electronics",
			UserId = userId
		};
		var deskEssentials = new Category
		{
			Label = "Desk Essentials",
			UserId = userId
		};
		await context.Categories.AddRangeAsync (electronics, deskEssentials);
		
		var laptop = new Item
		{
			Name = "Laptop",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddAsync (laptop);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = electronics.Id, UserId = userId},
			new () {ItemId = laptop.Id, CategoryId = deskEssentials.Id, UserId = userId}
		};
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		await context.SaveChangesAsync();

		var result = await repository.DeleteAllForItemAsync (laptop.Id, userId);
		
		result.Should().Be (2);
		
		var deleted = await context.ItemsCategories
									.AsNoTracking()
									.Where (i => i.ItemId == laptop.Id && i.UserId == userId)
									.ToListAsync();
		deleted.Should().NotBeNull();
		deleted.Should().HaveCount (0);
	}
	
	[Fact]
	public async Task DeleteAllForItemAsyncWithNonExistingItemShouldDoNothing ()
	{
		var userId = Guid.NewGuid();
		
		var electronics = new Category
		{
			Label = "Electronics",
			UserId = userId
		};
		var deskEssentials = new Category
		{
			Label = "Desk Essentials",
			UserId = userId
		};
		await context.Categories.AddRangeAsync (electronics, deskEssentials);
		
		var laptop = new Item
		{
			Name = "Laptop",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddAsync (laptop);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = electronics.Id, UserId = userId},
			new () {ItemId = laptop.Id, CategoryId = deskEssentials.Id, UserId = userId}
		};
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		await context.SaveChangesAsync();
		
		var nonExistingId = Guid.NewGuid();

		var result = await repository.DeleteAllForItemAsync (nonExistingId, userId);
		
		result.Should().Be (0);
		
		var deleted = await context.ItemsCategories
									.AsNoTracking()
									.ToListAsync();
		deleted.Should().NotBeNull();
		deleted.Should().HaveCount (2);
	}
	
	[Fact]
	public async Task DeleteAsyncWithExistingItemAndCategoryShouldDeleteFromDatabase ()
	{
		var userId = Guid.NewGuid();
		
		var electronics = new Category
		{
			Label = "Electronics",
			UserId = userId
		};
		var deskEssentials = new Category
		{
			Label = "Desk Essentials",
			UserId = userId
		};
		await context.Categories.AddRangeAsync (electronics, deskEssentials);
		
		var laptop = new Item
		{
			Name = "Laptop",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		var phone = new Item
		{
			Name = "Phone",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddRangeAsync (laptop, phone);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = electronics.Id, UserId = userId},
			new () {ItemId = laptop.Id, CategoryId = deskEssentials.Id, UserId = userId},
			new () {ItemId = phone.Id, CategoryId = electronics.Id, UserId = userId}
		};
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		await context.SaveChangesAsync();
		
		var toBeDeleted = new List<int>
		{
			electronics.Id
		};
		
		var result = await repository.BulkDeleteForItemAsync (laptop.Id, userId, toBeDeleted);
		
		result.Should().Be (1);
		
		var left = await context.ItemsCategories.AsNoTracking().ToListAsync();
		left.Should().HaveCount (2);
		left.Should().Contain (i => i.ItemId == laptop.Id && i.CategoryId == deskEssentials.Id);
		left.Should().Contain (i => i.ItemId == phone.Id && i.CategoryId == electronics.Id);
	}
	
	[Fact]
	public async Task DeleteAsyncWithNonExistingItemOrCategoryShouldDoNothing ()
	{
		var userId = Guid.NewGuid();
		
		var electronics = new Category
		{
			Label = "Electronics",
			UserId = userId
		};
		var deskEssentials = new Category
		{
			Label = "Desk Essentials",
			UserId = userId
		};
		await context.Categories.AddRangeAsync (electronics, deskEssentials);
		
		var laptop = new Item
		{
			Name = "Laptop",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		var phone = new Item
		{
			Name = "Phone",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddRangeAsync (laptop, phone);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = electronics.Id, UserId = userId},
			new () {ItemId = laptop.Id, CategoryId = deskEssentials.Id, UserId = userId},
			new () {ItemId = phone.Id, CategoryId = electronics.Id, UserId = userId}
		};
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		await context.SaveChangesAsync();
		
		var nonExistingId = Guid.NewGuid();
		var toBeDeleted = new List<int >
		{
			electronics.Id
		};
		
		var result = await repository.BulkDeleteForItemAsync (nonExistingId, userId, toBeDeleted);
		
		result.Should().Be (0);
		
		var left = await context.ItemsCategories.AsNoTracking().ToListAsync();
		left.Should().HaveCount (3);
		left.Should().Contain (i => i.ItemId == laptop.Id && i.CategoryId == electronics.Id);
		left.Should().Contain (i => i.ItemId == laptop.Id && i.CategoryId == deskEssentials.Id);
		left.Should().Contain (i => i.ItemId == phone.Id && i.CategoryId == electronics.Id);
	}
	
	[Fact]
	public async Task GetAllForItemAsyncWithExistingItemShouldReturnNonEmptyList ()
	{
		var userId = Guid.NewGuid();
		
		var electronics = new Category
		{
			Label = "Electronics",
			UserId = userId
		};
		var deskEssentials = new Category
		{
			Label = "Desk Essentials",
			UserId = userId
		};
		await context.Categories.AddRangeAsync (electronics, deskEssentials);
		
		var laptop = new Item
		{
			Name = "Laptop",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddAsync (laptop);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = electronics.Id, UserId = userId},
			new () {ItemId = laptop.Id, CategoryId = deskEssentials.Id, UserId = userId}
		};
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		await context.SaveChangesAsync();
		
		var result = await repository.GetAllForItemAsync (laptop.Id, userId);
		
		result.Should().NotBeNull();
		result.Should().HaveCount (2);
		result.Should().Contain (i => i.ItemId == laptop.Id && i.CategoryId == electronics.Id);
		result.Should().Contain (i => i.ItemId == laptop.Id && i.CategoryId == deskEssentials.Id);
		result.First().Category.Should().NotBeNull();
		result.Last().Category.Should().NotBeNull();
	}
	
	[Fact]
	public async Task GetAllForItemAsyncWithNonExistingItemShouldReturnEmptyList ()
	{
		var result = await repository.GetAllForItemAsync (Guid.NewGuid(), Guid.NewGuid());
		
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}
}