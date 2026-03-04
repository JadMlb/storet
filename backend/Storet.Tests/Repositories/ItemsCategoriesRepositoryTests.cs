using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storet.API.Models;
using Storet.API.Repositories.ItemsCategories;
using Storet.Tests.Repositories.Config;

namespace Storet.Tests.Repositories;

public class ItemsCategoriesRepositoryTests : SqliteRepositoryTestsBase<IItemsCategoriesRepository>
{
	protected override ItemsCategoriesRepository InitRepository ()
	{
		return new ItemsCategoriesRepository (context);
	}

	[Fact]
	public async Task InsertAsyncWithExistingItemAndCategoryShouldAddToDatabase ()
	{
		var category = new Category
		{
			Label = "Electronics"
		};
		await context.Categories.AddAsync (category);
		
		var laptop = new Item
		{
			Name = "Laptop"
		};
		var phone = new Item
		{
			Name = "Phone"
		};
		await context.Items.AddRangeAsync (laptop, phone);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = category.Id},
			new () {ItemId = phone.Id, CategoryId = category.Id}
		};

		var result = await repository.BulkInsertAsync (itemCategories);
		
		result.Should().Be (2);
		
		var saved = await context.ItemsCategories
									.Where (i => i.CategoryId == category.Id)
									.ToListAsync();
		saved.Should().NotBeNull();
		saved.Should().HaveCount (2);
		saved.Should().Contain (i => i.ItemId == laptop.Id && i.CategoryId == category.Id);
		saved.Should().Contain (i => i.ItemId == phone.Id && i.CategoryId == category.Id);
	}
	
	[Fact]
	public async Task InsertAsyncWithNonExistingItemShouldThrowException ()
	{
		var category = new Category
		{
			Label = "Electronics"
		};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();
		
		var nonExistentItemId = Guid.NewGuid();
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = nonExistentItemId, CategoryId = category.Id}
		};

		Func<Task> act = async () => await repository.BulkInsertAsync (itemCategories);
		
		await act.Should().ThrowAsync<DbUpdateException>();
		
		var count = await context.ItemsCategories.CountAsync();
		count.Should().Be (0);
	}

	[Fact]
	public async Task InsertAsyncWithNonExistingCategoryShouldThrowException ()
	{
		var laptop = new Item
		{
			Name = "Laptop"
		};
		await context.Items.AddAsync (laptop);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = 999}
		};

		Func<Task> act = async () => await repository.BulkInsertAsync (itemCategories);
		
		await act.Should().ThrowAsync<DbUpdateException>();
		
		var count = await context.ItemsCategories.CountAsync();
		count.Should().Be (0);
	}
	
	[Fact]
	public async Task InsertAsyncWithExistingRelationshipShouldThrowException ()
	{
		var electronics = new Category
		{
			Label = "Electronics"
		};
		await context.Categories.AddAsync (electronics);
		
		var laptop = new Item
		{
			Name = "Laptop"
		};
		await context.Items.AddAsync (laptop);
		await context.SaveChangesAsync();
		
		var laptopIsElectronics = new ItemCategory
		{
			ItemId = laptop.Id,
			CategoryId = electronics.Id
		};
		await context.ItemsCategories.AddAsync (laptopIsElectronics);
		
		await context.SaveChangesAsync();
		
		Func<Task> act = async () => await repository.BulkInsertAsync ([laptopIsElectronics]);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task DeleteAllForItemAsyncWithExistingItemShouldDeleteFromDatabase ()
	{
		var electronics = new Category
		{
			Label = "Electronics"
		};
		var deskEssentials = new Category
		{
			Label = "Desk Essentials"
		};
		await context.Categories.AddRangeAsync (electronics, deskEssentials);
		
		var laptop = new Item
		{
			Name = "Laptop"
		};
		await context.Items.AddAsync (laptop);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = electronics.Id},
			new () {ItemId = laptop.Id, CategoryId = deskEssentials.Id}
		};
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		await context.SaveChangesAsync();

		var result = await repository.DeleteAllForItemAsync (laptop.Id);
		
		result.Should().Be (2);
		
		var deleted = await context.ItemsCategories
									.AsNoTracking()
									.Where (i => i.ItemId == laptop.Id)
									.ToListAsync();
		deleted.Should().NotBeNull();
		deleted.Should().HaveCount (0);
	}
	
	[Fact]
	public async Task DeleteAllForItemAsyncWithNonExistingItemShouldDoNothing ()
	{
		var electronics = new Category
		{
			Label = "Electronics"
		};
		var deskEssentials = new Category
		{
			Label = "Desk Essentials"
		};
		await context.Categories.AddRangeAsync (electronics, deskEssentials);
		
		var laptop = new Item
		{
			Name = "Laptop"
		};
		await context.Items.AddAsync (laptop);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = electronics.Id},
			new () {ItemId = laptop.Id, CategoryId = deskEssentials.Id}
		};
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		await context.SaveChangesAsync();
		
		var nonExistingId = Guid.NewGuid();

		var result = await repository.DeleteAllForItemAsync (nonExistingId);
		
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
		var electronics = new Category
		{
			Label = "Electronics"
		};
		var deskEssentials = new Category
		{
			Label = "Desk Essentials"
		};
		await context.Categories.AddRangeAsync (electronics, deskEssentials);
		
		var laptop = new Item
		{
			Name = "Laptop"
		};
		var phone = new Item
		{
			Name = "Phone"
		};
		await context.Items.AddRangeAsync (laptop, phone);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = electronics.Id},
			new () {ItemId = laptop.Id, CategoryId = deskEssentials.Id},
			new () {ItemId = phone.Id, CategoryId = electronics.Id}
		};
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		await context.SaveChangesAsync();
		
		var toBeDeleted = new List<int>
		{
			electronics.Id
		};
		
		var result = await repository.BulkDeleteForItemAsync (laptop.Id, toBeDeleted);
		
		result.Should().Be (1);
		
		var left = await context.ItemsCategories.AsNoTracking().ToListAsync();
		left.Should().HaveCount (2);
		left.Should().Contain (i => i.ItemId == laptop.Id && i.CategoryId == deskEssentials.Id);
		left.Should().Contain (i => i.ItemId == phone.Id && i.CategoryId == electronics.Id);
	}
	
	[Fact]
	public async Task DeleteAsyncWithNonExistingItemOrCategoryShouldDoNothing ()
	{
		var electronics = new Category
		{
			Label = "Electronics"
		};
		var deskEssentials = new Category
		{
			Label = "Desk Essentials"
		};
		await context.Categories.AddRangeAsync (electronics, deskEssentials);
		
		var laptop = new Item
		{
			Name = "Laptop"
		};
		var phone = new Item
		{
			Name = "Phone"
		};
		await context.Items.AddRangeAsync (laptop, phone);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = electronics.Id},
			new () {ItemId = laptop.Id, CategoryId = deskEssentials.Id},
			new () {ItemId = phone.Id, CategoryId = electronics.Id}
		};
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		await context.SaveChangesAsync();
		
		var nonExistingId = Guid.NewGuid();
		var toBeDeleted = new List<int >
		{
			electronics.Id
		};
		
		var result = await repository.BulkDeleteForItemAsync (nonExistingId, toBeDeleted);
		
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
		var electronics = new Category
		{
			Label = "Electronics"
		};
		var deskEssentials = new Category
		{
			Label = "Desk Essentials"
		};
		await context.Categories.AddRangeAsync (electronics, deskEssentials);
		
		var laptop = new Item
		{
			Name = "Laptop"
		};
		await context.Items.AddAsync (laptop);
		await context.SaveChangesAsync();
		
		var itemCategories = new List<ItemCategory>
		{
			new () {ItemId = laptop.Id, CategoryId = electronics.Id},
			new () {ItemId = laptop.Id, CategoryId = deskEssentials.Id}
		};
		await context.ItemsCategories.AddRangeAsync (itemCategories);
		await context.SaveChangesAsync();
		
		var result = await repository.GetAllForItemAsync (laptop.Id);
		
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
		var result = await repository.GetAllForItemAsync (Guid.NewGuid());
		
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}
}