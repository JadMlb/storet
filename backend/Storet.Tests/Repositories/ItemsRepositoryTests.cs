using FluentAssertions;
using Storet.API.Models;
using Storet.API.Repositories.Items;
using Storet.API.Utils;
using Storet.Tests.Repositories.Config;

namespace Storet.Tests.Repositories;

public class ItemsRepositoryTests : InMemoryRepositoryTestsBase<ItemsRepository>
{
	protected override ItemsRepository InitRepository ()
	{
		return new ItemsRepository (context);
	}
	
	[Fact]
	public async Task InsertAsyncWithValidItemShouldAddToDatabase ()
	{
		var item = new Item
		{
			Name = "Laptop",
			Description = "My laptop"
		};

		var result = await repository.InsertAsync (item);

		result.Should().NotBeNull();
		result.Id.Should().NotBe (Guid.Empty);
		result.Name.Should().Be ("Laptop");
		result.Description.Should().Be ("My laptop");

		var savedItem = await context.Items.FindAsync (result.Id);
		savedItem.Should().NotBeNull();
		savedItem.Name.Should().Be ("Laptop");
		savedItem.Description.Should().Be ("My laptop");
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
			new () {Name = "Apple"},
			new () {Name = "Banana"},
			new () {Name = "Chicken breasts"},
			new () {Name = "Orange"},
			new () {Name = "Rice"},
			new () {Name = "Steak"},
			new () {Name = "Sugar"},
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
			new () {Name = "Apple"},
			new () {Name = "Banana"},
			new () {Name = "Chicken breasts"},
			new () {Name = "Chocolate"},
			new () {Name = "Lemon"},
			new () {Name = "Orange"},
			new () {Name = "Rice"},
			new () {Name = "Steak"},
			new () {Name = "Sugar"},
			new () {Name = "Tea"},
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
			Description = "My laptop"
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
			Description = "My laptop"
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
			Description = "My Laptop"
		};
		await context.Items.AddAsync (item);
		await context.SaveChangesAsync();

		item.Description = null;
		item.Name = "My laptop";

		var result = await repository.UpdateAsync (item.Id, item);
		
		result.Should().NotBeNull();
		result.Description.Should().BeNull();
		result.Name.Should().Be ("My laptop");

		var updated = await context.Items.FindAsync (item.Id);
		updated.Should().NotBeNull();
		updated.Name.Should().Be ("My laptop");
		updated.Description.Should().BeNull();
	}
	
	[Fact]
	public async Task DeleteWithExistingIdShouldReturnTrue ()
	{
		var item = new Item
		{
			Name = "Laptop",
			Description = "My Laptop"
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