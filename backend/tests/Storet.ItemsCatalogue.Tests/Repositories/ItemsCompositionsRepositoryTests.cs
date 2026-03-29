using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storet.Modules.ItemsCatalogue.Data;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Modules.ItemsCatalogue.Repositories.ItemsCompositions;
using Storet.Tests.Common.Repository;
using Xunit.Abstractions;

namespace Storet.ItemsCatalogue.Tests.Repositories;

public class ItemsCompositionsRepositoryTests : SqliteRepositoryTestsBase<StoretItemsCatalogueDbContext, IItemsCompositionRepository>
{
	public ItemsCompositionsRepositoryTests (ITestOutputHelper output) : base (output) {}

	protected override StoretItemsCatalogueDbContext InitDbContextWithOptions (DbContextOptions<StoretItemsCatalogueDbContext> options)
	{
		return new StoretItemsCatalogueDbContext (options);
	}
	
	protected override IItemsCompositionRepository InitRepository ()
	{
		return new ItemsCompositionsRepository (this.context);
	}
	
	[Fact]
	public async Task InsertAsyncWithExistingItemAndComponentShouldAddToDatabase ()
	{
		var userId = Guid.NewGuid();
		
		var category = new Category
		{
			Id = 1,
			Label = "Breakfast",
			UserId = userId
		};
		await context.Categories.AddAsync (category);
		
		var componentItemId = Guid.NewGuid();
		var componentItem = new Item
		{
			Id = componentItemId,
			Name = "Tea Bag",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId,
			ItemCategories = [
				new () {ItemId = componentItemId, CategoryId = 1}
			]
		};
		
		var parentItemId = Guid.NewGuid();
		var parentItem = new Item
		{
			Id = parentItemId,
			Name = "Tea Box",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId,
			ItemCategories = [
				new () {ItemId = componentItemId, CategoryId = 1, UserId = userId}
			]
		};
		await context.Items.AddRangeAsync (parentItem, componentItem);
		await context.SaveChangesAsync();
		
		var itemCompositions = new List<ItemComposition>
		{
			new () {ParentItemId = parentItemId, ComponentItemId = componentItemId, Quantity = 10, UserId = userId},
		};

		var result = await repository.BulkInsertAsync (itemCompositions);
		
		result.Should().Be (1);
		
		var saved = await context.ItemsCompositions
									.Where (i => i.ParentItemId == parentItemId && i.UserId == userId)
									.ToListAsync();
		saved.Should().NotBeNull();
		saved.Should().HaveCount (1);
		saved.Should().Contain (i => i.ComponentItemId == componentItemId && i.Quantity == 10);
	}
	
	[Fact]
	public async Task InsertAsyncWithNonExistingParentItemShouldThrowException ()
	{
		var userId = Guid.NewGuid();
		
		var component = new Item
		{
			Name = "Component",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddAsync (component);
		await context.SaveChangesAsync();
		
		var nonExistentItemId = Guid.NewGuid();
		var itemComponents = new List<ItemComposition>
		{
			new () {ParentItemId = nonExistentItemId, ComponentItemId = component.Id, Quantity = 1, UserId = userId}
		};

		Func<Task> act = async () => await repository.BulkInsertAsync (itemComponents);
		
		await act.Should().ThrowAsync<DbUpdateException>();
		
		var count = await context.ItemsCategories.CountAsync();
		count.Should().Be (0);
	}

	[Fact]
	public async Task InsertAsyncWithNonExistingComponentShouldThrowException ()
	{
		var userId = Guid.NewGuid();
		
		var parent = new Item
		{
			Name = "Parent",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddAsync (parent);
		await context.SaveChangesAsync();
		
		var nonExistentItemId = Guid.NewGuid();
		var itemComponents = new List<ItemComposition>
		{
			new () {ParentItemId = parent.Id, ComponentItemId = nonExistentItemId, Quantity = 1, UserId = userId}
		};

		Func<Task> act = async () => await repository.BulkInsertAsync (itemComponents);
		
		await act.Should().ThrowAsync<DbUpdateException>();
		
		var count = await context.ItemsCategories.CountAsync();
		count.Should().Be (0);
	}
	
	[Fact]
	public async Task InsertAsyncWithExistingRelationshipShouldThrowException ()
	{
		var userId = Guid.NewGuid();
		
		var parent = new Item
		{
			Name = "Tea Box",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var component = new Item
		{
			Name = "Tea Bag",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddRangeAsync (parent, component);
		await context.SaveChangesAsync();
		
		var composition = new ItemComposition
		{
			ParentItemId = parent.Id,
			ComponentItemId = component.Id,
			Quantity = 10,
			UserId = userId
		};
		await context.ItemsCompositions.AddAsync (composition);
		await context.SaveChangesAsync();
		
		Func<Task> act = async () => await repository.BulkInsertAsync ([composition]);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task UpdateWithExistingRelationshipShouldReturnTrueAndUpdateDatabase ()
	{
		var userId = Guid.NewGuid();
		
		var parent = new Item
		{
			Name = "Tea Box",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var component = new Item
		{
			Name = "Tea Bag",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddRangeAsync (parent, component);
		await context.SaveChangesAsync();
		
		var composition = new ItemComposition
		{
			ParentItemId = parent.Id,
			ComponentItemId = component.Id,
			Quantity = 10,
			UserId = userId
		};
		await context.ItemsCompositions.AddAsync (composition);
		await context.SaveChangesAsync();
		
		var result = await repository.UpdateAsync (parent.Id, component.Id, userId, 100);
		
		result.Should().Be (true);
		
		var updated = await context.ItemsCompositions.FirstOrDefaultAsync (c => c.ParentItemId == parent.Id && c.ComponentItemId == component.Id);
		updated.Should().NotBeNull();
		updated.Quantity.Should().Be (100);
	}
	
	[Fact]
	public async Task UpdateWithNonExistingRelationshipShouldReturnFalse ()
	{
		var result = await repository.UpdateAsync (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100);
		
		result.Should().Be (false);
	}
	
	[Fact]
	public async Task DeleteAllForItemAsyncWithExistingParentItemShouldDeleteFromDatabase ()
	{
		var userId = Guid.NewGuid();
		
		var parent = new Item
		{
			Name = "Tea Box",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var component = new Item
		{
			Name = "Tea Bag",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddRangeAsync (parent, component);
		await context.SaveChangesAsync();
		
		var composition = new ItemComposition
		{
			ParentItemId = parent.Id,
			ComponentItemId = component.Id,
			Quantity = 10,
			UserId = userId
		};
		await context.ItemsCompositions.AddAsync (composition);
		await context.SaveChangesAsync();

		var result = await repository.DeleteAllForItemAsync (parent.Id, userId);
		
		result.Should().Be (1);
		
		var deleted = await context.ItemsCompositions
									.AsNoTracking()
									.Where (i => i.ParentItemId == parent.Id && i.UserId == userId)
									.ToListAsync();
		deleted.Should().NotBeNull();
		deleted.Should().HaveCount (0);
	}
	
	[Fact]
	public async Task DeleteAllForItemAsyncWithNonExistingParentItemShouldDoNothing ()
	{
		var userId = Guid.NewGuid();
		
		var parent = new Item
		{
			Name = "Tea Box",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var component = new Item
		{
			Name = "Tea Bag",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddRangeAsync (parent, component);
		await context.SaveChangesAsync();
		
		var composition = new ItemComposition
		{
			ParentItemId = parent.Id,
			ComponentItemId = component.Id,
			Quantity = 10,
			UserId = userId
		};
		await context.ItemsCompositions.AddAsync (composition);
		await context.SaveChangesAsync();
		
		var nonExistingId = Guid.NewGuid();

		var result = await repository.DeleteAllForItemAsync (nonExistingId, userId);
		
		result.Should().Be (0);
		
		var deleted = await context.ItemsCompositions
									.AsNoTracking()
									.ToListAsync();
		deleted.Should().NotBeNull();
		deleted.Should().HaveCount (1);
	}
	
	[Fact]
	public async Task DeleteAsyncWithExistingParentAndComponentItemShouldDeleteFromDatabase ()
	{
		var userId = Guid.NewGuid();
		
		var teaBox = new Item
		{
			Name = "Tea Box",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var teaBag = new Item
		{
			Name = "Tea Bag",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var sugarBox = new Item
		{
			Name = "Sugar Box",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var sugarCube = new Item
		{
			Name = "Sugar Cube",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddRangeAsync (teaBox, teaBag, sugarBox, sugarCube);
		await context.SaveChangesAsync();
		
		var compositions = new List<ItemComposition>
		{
			new ()
			{
				ParentItemId = teaBox.Id,
				ComponentItemId = teaBag.Id,
				Quantity = 10,
				UserId = userId
			},
			new ()
			{
				ParentItemId = sugarBox.Id,
				ComponentItemId = sugarCube.Id,
				Quantity = 100,
				UserId = userId
			},
		};
		await context.ItemsCompositions.AddRangeAsync (compositions);
		await context.SaveChangesAsync();
		
		var toBeDeleted = new List<Guid>
		{
			sugarCube.Id
		};
		
		var result = await repository.BulkDeleteForItemAsync (sugarBox.Id, userId, toBeDeleted);
		
		result.Should().Be (1);
		
		var left = await context.ItemsCompositions.AsNoTracking().ToListAsync();
		left.Should().HaveCount (1);
		left.Should().Contain (i => i.ParentItemId == teaBox.Id && i.ComponentItemId == teaBag.Id && i.UserId == userId);
	}
	
	[Fact]
	public async Task DeleteAsyncWithNonExistingParentOrComponentItemShouldDoNothing ()
	{
		var userId = Guid.NewGuid();
		
		var teaBox = new Item
		{
			Name = "Tea Box",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var teaBag = new Item
		{
			Name = "Tea Bag",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var sugarBox = new Item
		{
			Name = "Sugar Box",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var sugarCube = new Item
		{
			Name = "Sugar Cube",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddRangeAsync (teaBox, teaBag, sugarBox, sugarCube);
		await context.SaveChangesAsync();
		
		var compositions = new List<ItemComposition>
		{
			new ()
			{
				ParentItemId = teaBox.Id,
				ComponentItemId = teaBag.Id,
				Quantity = 10,
				UserId = userId
			},
			new ()
			{
				ParentItemId = sugarBox.Id,
				ComponentItemId = sugarCube.Id,
				Quantity = 100,
				UserId = userId
			},
		};
		await context.ItemsCompositions.AddRangeAsync (compositions);
		await context.SaveChangesAsync();
		
		var nonExistingId = Guid.NewGuid();
		var toBeDeleted = new List<Guid>
		{
			Guid.NewGuid()
		};
		
		var result = await repository.BulkDeleteForItemAsync (nonExistingId, userId, toBeDeleted);
		
		result.Should().Be (0);
		
		var left = await context.ItemsCompositions.AsNoTracking().ToListAsync();
		left.Should().HaveCount (2);
	}
	
	[Fact]
	public async Task GetAllForItemAsyncWithExistingItemShouldReturnNonEmptyList ()
	{
		var userId = Guid.NewGuid();
		
		var teaBox = new Item
		{
			Name = "Tea Box",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var teaBag = new Item
		{
			Name = "Tea Bag",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var sugarBox = new Item
		{
			Name = "Sugar Box",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		
		var sugarCube = new Item
		{
			Name = "Sugar Cube",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		await context.Items.AddRangeAsync (teaBox, teaBag, sugarBox, sugarCube);
		await context.SaveChangesAsync();
		
		var compositions = new List<ItemComposition>
		{
			new ()
			{
				ParentItemId = teaBox.Id,
				ComponentItemId = teaBag.Id,
				Quantity = 10,
				UserId = userId
			},
			new ()
			{
				ParentItemId = sugarBox.Id,
				ComponentItemId = sugarCube.Id,
				Quantity = 100,
				UserId = userId
			},
		};
		await context.ItemsCompositions.AddRangeAsync (compositions);
		await context.SaveChangesAsync();
		
		var result = await repository.GetAllForItemAsync (teaBox.Id, userId);
		
		result.Should().NotBeNull();
		result.Should().HaveCount (1);
		result.Should().Contain (i => i.ParentItemId == teaBox.Id && i.ComponentItemId == teaBag.Id && i.UserId == userId);
		result.First().ComponentItem.Should().NotBeNull();
		result.First().Quantity.Should().Be (10);
	}
	
	[Fact]
	public async Task GetAllForItemAsyncWithNonExistingItemShouldReturnEmptyList ()
	{
		var result = await repository.GetAllForItemAsync (Guid.NewGuid(), Guid.NewGuid());
		
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}
}