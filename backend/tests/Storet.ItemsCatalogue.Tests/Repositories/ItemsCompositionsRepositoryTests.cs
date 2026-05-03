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
		
		result.Should().Contain (c => c == component.Id);
		
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
		
		result.Should().BeEmpty();
		
		var deleted = await context.ItemsCompositions
									.AsNoTracking()
									.ToListAsync();
		deleted.Should().NotBeNull();
		deleted.Should().HaveCount (1);
	}

	[Fact]
	public async Task GetComponentIdsWithNonExistingItemOrNoComponentsShouldReturnEntryWithNull ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		List<Guid> itemIdAsList = [itemId];
		
		var result = await repository.GetComponentIdsForItemsAsync (itemIdAsList, userId);

		result.Should().NotBeEmpty();

		var componentsForItem = result.GetValueOrDefault (itemId);
		componentsForItem.Should().NotBeNull();
		componentsForItem.Should().BeEmpty();
	}

	[Fact]
	public async Task GetComponentIdsWithExistingItemAndComponentsShouldReturnEntry ()
	{
		var userId = Guid.NewGuid();
		var itemId = Guid.NewGuid();
		List<Guid> itemIdAsList = [itemId];

		var category = new Category
		{
			Id = 1,
			Label = "Hygiene",
			UserId = userId
		};
		await context.Categories.AddAsync (category);
		
		var parentItem = new Item
		{
			Id = itemId,
			Name = "Special pack",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};
		var itemCategory = new ItemCategory
		{
			ItemId = itemId,
			CategoryId = 1,
			UserId = userId
		};
		await context.ItemsCategories.AddAsync (itemCategory);

		Guid component1Id = Guid.NewGuid(), component2Id = Guid.NewGuid();
		var component1 = new Item
		{
			Id = component1Id,
			Name = "Shampoo",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};

		var component2 = new Item
		{
			Id = component2Id,
			Name = "Shower Gel",
			Quantity = 1,
			Unit = Unit.Unit,
			UserId = userId
		};

		await context.Items.AddRangeAsync (parentItem, component1, component2);

		var relationships = new List<ItemComposition>
		{
			new ()
			{
				ParentItemId = itemId,
				ComponentItemId = component1Id,
				Quantity = 1,
				UserId = userId
			},
			new ()
			{
				ParentItemId = itemId,
				ComponentItemId = component2Id,
				Quantity = 1,
				UserId = userId
			}
		};
		await context.ItemsCompositions.AddRangeAsync (relationships);

		await context.SaveChangesAsync();

		context.Entry(parentItem).State = EntityState.Detached;
		context.Entry(component1).State = EntityState.Detached;
		context.Entry(component2).State = EntityState.Detached;
		
		var result = await repository.GetComponentIdsForItemsAsync (itemIdAsList, userId);

		result.Should().NotBeEmpty();

		var componentsForItem = result.GetValueOrDefault (itemId);
		componentsForItem.Should().NotBeNull();
		componentsForItem.Should().HaveCount (2);
		componentsForItem.Should().Contain (cId => cId == component1Id);
		componentsForItem.Should().Contain (cId => cId == component2Id);
	}
}