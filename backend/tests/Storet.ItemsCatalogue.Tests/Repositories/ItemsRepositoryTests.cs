using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storet.API.Core.Utils;
using Storet.API.ItemsCatalogue.Data;
using Storet.API.ItemsCatalogue.Models;
using Storet.API.ItemsCatalogue.Repositories.Items;
using Storet.Tests.Common.Repository;

namespace Storet.ItemsCatalogue.Tests.Repositories;

public class ItemsRepositoryTests : InMemoryRepositoryTestsBase<StoretItemsCatalogueDbContext, ItemsRepository>
{
	protected override ItemsRepository InitRepository ()
	{
		return new ItemsRepository (context);
	}
	
	protected override StoretItemsCatalogueDbContext InitDbContextWithOptions (DbContextOptions<StoretItemsCatalogueDbContext> options)
	{
		return new StoretItemsCatalogueDbContext (options);
	}
	
	[Fact]
	public async Task InsertAsyncWithValidItemShouldAddToDatabase ()
	{
		var item = new Item
		{
			Name = "Laptop",
			Description = "My laptop",
			Quantity = 1,
			Unit = Unit.Unit
		};

		var result = await repository.InsertAsync (item);

		result.Should().NotBeNull();
		result.Id.Should().NotBe (Guid.Empty);
		result.Name.Should().Be ("Laptop");
		result.Description.Should().Be ("My laptop");
		result.Quantity.Should().Be (1);
		result.Unit.Should().Be (Unit.Unit);

		var savedItem = await context.Items.FindAsync (result.Id);
		savedItem.Should().NotBeNull();
		savedItem.Name.Should().Be ("Laptop");
		savedItem.Description.Should().Be ("My laptop");
		savedItem.Quantity.Should().Be (1);
		savedItem.Unit.Should().Be (Unit.Unit);
	}

	[Fact]
	public async Task GetAllWithValidQueryShouldReturnList ()
	{
		var category = new Category
		{
			Id = 1,
			Label = "Food"
		};
		await context.Categories.AddAsync (category);

		var items = new List<Item>
		{
			new () {Name = "Apple", Quantity = 1, Unit = Unit.Unit},
			new () {Name = "Banana", Quantity = 1, Unit = Unit.Unit},
			new () {Name = "Chicken breasts", Quantity = 400, Unit = Unit.Gram},
			new () {Name = "Orange", Quantity = 1, Unit = Unit.Unit},
			new () {Name = "Rice", Quantity = 200, Unit = Unit.Gram},
			new () {Name = "Steak", Quantity = 400, Unit = Unit.Gram},
			new () {Name = "Sugar", Quantity = 100, Unit = Unit.Gram},
		};
		await context.Items.AddRangeAsync (items);
		await context.SaveChangesAsync();
		
		var query = new Query<string>
		{
			PageSize = 5
		};

		var response = await repository.GetAllAsync (query);

		response.Should().HaveCount (6);
		response.First().Name.Should().Be ("Apple");
		response.Last().Name.Should().Be ("Steak");
		response.Should().BeInAscendingOrder ((a, b) => a.Name.CompareTo (b.Name));
	}
	
	[Fact]
	public async Task GetAllWithValidQueryForNextPageShouldReturnList ()
	{
		var category = new Category
		{
			Id = 1,
			Label = "Food"
		};
		await context.Categories.AddAsync (category);

		var items = new List<Item>
		{
			new () {Name = "Apple", Quantity = 1, Unit = Unit.Unit},
			new () {Name = "Banana", Quantity = 1, Unit = Unit.Unit},
			new () {Name = "Chicken breasts", Quantity = 400, Unit = Unit.Gram},
			new () {Name = "Chocolate", Quantity = 1, Unit = Unit.Unit},
			new () {Name = "Lemon", Quantity = 1, Unit = Unit.Unit},
			new () {Name = "Orange", Quantity = 1, Unit = Unit.Unit},
			new () {Name = "Rice", Quantity = 200, Unit = Unit.Gram},
			new () {Name = "Steak", Quantity = 400, Unit = Unit.Gram},
			new () {Name = "Sugar", Quantity = 100, Unit = Unit.Gram},
			new () {Name = "Tea", Quantity = 1, Unit = Unit.Unit},
		};
		await context.Items.AddRangeAsync (items);
		await context.SaveChangesAsync();
		
		var query = new Query<string>
		{
			PageSize = 5,
			Key = "Orange"
		};

		var response = await repository.GetAllAsync (query);

		response.Should().HaveCount (5);
		response.First().Name.Should().Be ("Orange");
		response.Last().Name.Should().Be ("Tea");
		response.Should().BeInAscendingOrder ((a, b) => a.Name.CompareTo (b.Name));
	}

	[Fact]
	public async Task GetOneWithExistingIdShouldReturnItem ()
	{
		var category = new Category
		{
			Id = 1,
			Label = "Electronics"
		};
		await context.Categories.AddAsync (category);

		var item = new Item
		{
			Name = "Laptop",
			Description = "My laptop",
			Quantity = 1,
			Unit = Unit.Unit
		};
		await context.Items.AddAsync (item);
		await context.SaveChangesAsync();

		var itemsCategories = new List<ItemCategory>
		{
			new () {ItemId = item.Id, CategoryId = category.Id}
		};
		await context.ItemsCategories.AddRangeAsync (itemsCategories);
		await context.SaveChangesAsync();

		var result = await repository.GetOneAsync (item.Id);

		result.Should().NotBeNull();
		result.Id.Should().Be (item.Id);
		result.Name.Should().Be ("Laptop");
		result.Description.Should().Be ("My laptop");
		result.Quantity.Should().Be (1);
		result.Unit.Should().Be (Unit.Unit);
		result.ItemCategories.Should().NotBeEmpty();
		result.ItemCategories.Should().HaveCount (1);
		result.ItemCategories.First().Should().NotBeNull();
		result.ItemCategories.First().CategoryId.Should().Be (1);
	}
	
	[Fact]
	public async Task GetOneWithNonExistingIdShouldReturnNull ()
	{
		var result = await repository.GetOneAsync (Guid.NewGuid());

		result.Should().BeNull();
	}

	[Fact]
	public async Task ExistsWithExistingIdShouldReturnTrue ()
	{
		var category = new Category
		{
			Id = 1,
			Label = "Electronics"
		};
		await context.Categories.AddAsync (category);

		var item = new Item
		{
			Name = "Laptop",
			Description = "My laptop",
			Quantity = 1,
			Unit = Unit.Unit
		};
		await context.Items.AddAsync (item);
		await context.SaveChangesAsync();

		var result = await repository.ExistsAsync (item.Id);

		result.Should().BeTrue();
	}
	
	[Fact]
	public async Task ExistsWithNonExistingIdShouldReturnFalse ()
	{
		var result = await repository.ExistsAsync (Guid.NewGuid());

		result.Should().BeFalse();
	}
	
	[Fact]
	public async Task UpdateWithValidDataShouldUpdateRecord ()
	{
		var item = new Item
		{
			Name = "Laptop",
			Description = "My Laptop",
			Quantity = 1,
			Unit = Unit.Unit
		};
		await context.Items.AddAsync (item);
		await context.SaveChangesAsync();

		item.Description = null;
		item.Name = "My laptop";
		item.Quantity = 2;

		var result = await repository.UpdateAsync (item.Id, item);
		
		result.Should().NotBeNull();
		result.Description.Should().BeNull();
		result.Name.Should().Be ("My laptop");
		result.Quantity.Should().Be (2);

		var updated = await context.Items.FindAsync (item.Id);
		updated.Should().NotBeNull();
		updated.Name.Should().Be ("My laptop");
		updated.Description.Should().BeNull();
		updated.Quantity.Should().Be (2);
	}
	
	[Fact]
	public async Task DeleteWithExistingIdShouldReturnTrue ()
	{
		var item = new Item
		{
			Name = "Laptop",
			Description = "My Laptop",
			Quantity = 1,
			Unit = Unit.Unit
		};
		await context.Items.AddAsync (item);
		await context.SaveChangesAsync();

		var result = await repository.DeleteAsync (item.Id);
		
		result.Should().BeTrue();

		var deleted = await context.Items.FindAsync (item.Id);
		deleted.Should().BeNull();
	}
	
	[Fact]
	public async Task DeleteWithNonExistingIdShouldReturnFalse ()
	{
		var result = await repository.DeleteAsync (Guid.NewGuid());
		
		result.Should().BeFalse();
	}
}