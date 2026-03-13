using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storet.API.ItemsCatalogue.Data;
using Storet.API.ItemsCatalogue.Models;
using Storet.API.ItemsCatalogue.Repositories.ItemsCompositions;
using Storet.Tests.Common.Repository;

namespace Storet.ItemsCatalogue.Tests.Repositories;

public class ItemsCompositionsRepositoryTests : SqliteRepositoryTestsBase<StoretItemsCatalogueDbContext, IItemsCompositionRepository>
{
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
		var category = new Category
		{
			Id = 1,
			Label = "Breakfast"
		};
		await context.Categories.AddAsync (category);
		
		var componentItemId = Guid.NewGuid();
		var componentItem = new Item
		{
			Id = componentItemId,
			Name = "Tea Bag",
			ItemCategories = [
				new () {ItemId = componentItemId, CategoryId = 1}
			]
		};
		
		var parentItemId = Guid.NewGuid();
		var parentItem = new Item
		{
			Id = parentItemId,
			Name = "Tea Box",
			ItemCategories = [
				new () {ItemId = componentItemId, CategoryId = 1}
			]
		};
		await context.Items.AddRangeAsync (parentItem, componentItem);
		await context.SaveChangesAsync();
		
		var itemCompositions = new List<ItemComposition>
		{
			new () {ParentItemId = parentItemId, ComponentItemId = componentItemId, Quantity = 10, Unit = "unit"},
		};

		var result = await repository.BulkInsertAsync (itemCompositions);
		
		result.Should().Be (1);
		
		var saved = await context.ItemsCompositions
									.Where (i => i.ParentItemId == parentItemId)
									.ToListAsync();
		saved.Should().NotBeNull();
		saved.Should().HaveCount (1);
		saved.Should().Contain (i => i.ComponentItemId == componentItemId && i.Quantity == 10 && i.Unit == "unit");
	}
	
	[Fact]
	public async Task InsertAsyncWithNonExistingParentItemShouldThrowException ()
	{
		var component = new Item
		{
			Name = "Component"
		};
		await context.Items.AddAsync (component);
		await context.SaveChangesAsync();
		
		var nonExistentItemId = Guid.NewGuid();
		var itemComponents = new List<ItemComposition>
		{
			new () {ParentItemId = nonExistentItemId, ComponentItemId = component.Id, Quantity = 1, Unit = "unit"}
		};

		Func<Task> act = async () => await repository.BulkInsertAsync (itemComponents);
		
		await act.Should().ThrowAsync<DbUpdateException>();
		
		var count = await context.ItemsCategories.CountAsync();
		count.Should().Be (0);
	}

	[Fact]
	public async Task InsertAsyncWithNonExistingComponentShouldThrowException ()
	{
		var parent = new Item
		{
			Name = "Parent"
		};
		await context.Items.AddAsync (parent);
		await context.SaveChangesAsync();
		
		var nonExistentItemId = Guid.NewGuid();
		var itemComponents = new List<ItemComposition>
		{
			new () {ParentItemId = parent.Id, ComponentItemId = nonExistentItemId, Quantity = 1, Unit = "unit"}
		};

		Func<Task> act = async () => await repository.BulkInsertAsync (itemComponents);
		
		await act.Should().ThrowAsync<DbUpdateException>();
		
		var count = await context.ItemsCategories.CountAsync();
		count.Should().Be (0);
	}
	
	[Fact]
	public async Task InsertAsyncWithExistingRelationshipShouldThrowException ()
	{
		var parent = new Item
		{
			Name = "Tea Box"
		};
		
		var component = new Item
		{
			Name = "Tea Bag"
		};
		await context.Items.AddRangeAsync (parent, component);
		await context.SaveChangesAsync();
		
		var composition = new ItemComposition
		{
			ParentItemId = parent.Id,
			ComponentItemId = component.Id,
			Quantity = 10,
			Unit = "unit"
		};
		await context.ItemsCompositions.AddAsync (composition);
		await context.SaveChangesAsync();
		
		Func<Task> act = async () => await repository.BulkInsertAsync ([composition]);
		
		await act.Should().ThrowAsync<DbUpdateException>();
	}
	
	[Fact]
	public async Task UpdateWithExistingRelationshipShouldReturnTrueAndUpdateDatabase ()
	{
		var parent = new Item
		{
			Name = "Tea Box"
		};
		
		var component = new Item
		{
			Name = "Tea Bag"
		};
		await context.Items.AddRangeAsync (parent, component);
		await context.SaveChangesAsync();
		
		var composition = new ItemComposition
		{
			ParentItemId = parent.Id,
			ComponentItemId = component.Id,
			Quantity = 10,
			Unit = "unit"
		};
		await context.ItemsCompositions.AddAsync (composition);
		await context.SaveChangesAsync();
		
		var result = await repository.UpdateAsync (parent.Id, component.Id, 100);
		
		result.Should().Be (true);
		
		var updated = await context.ItemsCompositions.FirstOrDefaultAsync (c => c.ParentItemId == parent.Id && c.ComponentItemId == component.Id);
		updated.Should().NotBeNull();
		updated.Quantity.Should().Be (100);
	}
	
	[Fact]
	public async Task UpdateWithNonExistingRelationshipShouldReturnFalse ()
	{
		var result = await repository.UpdateAsync (Guid.NewGuid(), Guid.NewGuid(), 100);
		
		result.Should().Be (false);
	}
	
	[Fact]
	public async Task DeleteAllForItemAsyncWithExistingParentItemShouldDeleteFromDatabase ()
	{
		var parent = new Item
		{
			Name = "Tea Box"
		};
		
		var component = new Item
		{
			Name = "Tea Bag"
		};
		await context.Items.AddRangeAsync (parent, component);
		await context.SaveChangesAsync();
		
		var composition = new ItemComposition
		{
			ParentItemId = parent.Id,
			ComponentItemId = component.Id,
			Quantity = 10,
			Unit = "unit"
		};
		await context.ItemsCompositions.AddAsync (composition);
		await context.SaveChangesAsync();

		var result = await repository.DeleteAllForItemAsync (parent.Id);
		
		result.Should().Be (1);
		
		var deleted = await context.ItemsCompositions
									.AsNoTracking()
									.Where (i => i.ParentItemId == parent.Id)
									.ToListAsync();
		deleted.Should().NotBeNull();
		deleted.Should().HaveCount (0);
	}
	
	[Fact]
	public async Task DeleteAllForItemAsyncWithNonExistingParentItemShouldDoNothing ()
	{
		var parent = new Item
		{
			Name = "Tea Box"
		};
		
		var component = new Item
		{
			Name = "Tea Bag"
		};
		await context.Items.AddRangeAsync (parent, component);
		await context.SaveChangesAsync();
		
		var composition = new ItemComposition
		{
			ParentItemId = parent.Id,
			ComponentItemId = component.Id,
			Quantity = 10,
			Unit = "unit"
		};
		await context.ItemsCompositions.AddAsync (composition);
		await context.SaveChangesAsync();
		
		var nonExistingId = Guid.NewGuid();

		var result = await repository.DeleteAllForItemAsync (nonExistingId);
		
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
		var teaBox = new Item
		{
			Name = "Tea Box"
		};
		
		var teaBag = new Item
		{
			Name = "Tea Bag"
		};
		
		var sugarBox = new Item
		{
			Name = "Sugar Box"
		};
		
		var sugarCube = new Item
		{
			Name = "Sugar Cube"
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
				Unit = "unit"
			},
			new ()
			{
				ParentItemId = sugarBox.Id,
				ComponentItemId = sugarCube.Id,
				Quantity = 100,
				Unit = "unit"
			},
		};
		await context.ItemsCompositions.AddRangeAsync (compositions);
		await context.SaveChangesAsync();
		
		var toBeDeleted = new List<Guid>
		{
			sugarCube.Id
		};
		
		var result = await repository.BulkDeleteForItemAsync (sugarBox.Id, toBeDeleted);
		
		result.Should().Be (1);
		
		var left = await context.ItemsCompositions.AsNoTracking().ToListAsync();
		left.Should().HaveCount (1);
		left.Should().Contain (i => i.ParentItemId == teaBox.Id && i.ComponentItemId == teaBag.Id);
	}
	
	[Fact]
	public async Task DeleteAsyncWithNonExistingParentOrComponentItemShouldDoNothing ()
	{
		var teaBox = new Item
		{
			Name = "Tea Box"
		};
		
		var teaBag = new Item
		{
			Name = "Tea Bag"
		};
		
		var sugarBox = new Item
		{
			Name = "Sugar Box"
		};
		
		var sugarCube = new Item
		{
			Name = "Sugar Cube"
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
				Unit = "unit"
			},
			new ()
			{
				ParentItemId = sugarBox.Id,
				ComponentItemId = sugarCube.Id,
				Quantity = 100,
				Unit = "unit"
			},
		};
		await context.ItemsCompositions.AddRangeAsync (compositions);
		await context.SaveChangesAsync();
		
		var nonExistingId = Guid.NewGuid();
		var toBeDeleted = new List<Guid>
		{
			Guid.NewGuid()
		};
		
		var result = await repository.BulkDeleteForItemAsync (nonExistingId, toBeDeleted);
		
		result.Should().Be (0);
		
		var left = await context.ItemsCompositions.AsNoTracking().ToListAsync();
		left.Should().HaveCount (2);
	}
	
	[Fact]
	public async Task GetAllForItemAsyncWithExistingItemShouldReturnNonEmptyList ()
	{
		var teaBox = new Item
		{
			Name = "Tea Box"
		};
		
		var teaBag = new Item
		{
			Name = "Tea Bag"
		};
		
		var sugarBox = new Item
		{
			Name = "Sugar Box"
		};
		
		var sugarCube = new Item
		{
			Name = "Sugar Cube"
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
				Unit = "unit"
			},
			new ()
			{
				ParentItemId = sugarBox.Id,
				ComponentItemId = sugarCube.Id,
				Quantity = 100,
				Unit = "unit"
			},
		};
		await context.ItemsCompositions.AddRangeAsync (compositions);
		await context.SaveChangesAsync();
		
		var result = await repository.GetAllForItemAsync (teaBox.Id);
		
		result.Should().NotBeNull();
		result.Should().HaveCount (1);
		result.Should().Contain (i => i.ParentItemId == teaBox.Id && i.ComponentItemId == teaBag.Id);
		result.First().ComponentItem.Should().NotBeNull();
		result.First().Quantity.Should().Be (10);
		result.First().Unit.Should().Be ("unit");
	}
	
	[Fact]
	public async Task GetAllForItemAsyncWithNonExistingItemShouldReturnEmptyList ()
	{
		var result = await repository.GetAllForItemAsync (Guid.NewGuid());
		
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}
}