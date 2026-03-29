using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storet.Modules.ItemsCatalogue.Data;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Modules.ItemsCatalogue.Repositories.Categories;
using Storet.Tests.Common.Repository;
using Xunit.Abstractions;

namespace Storet.ItemsCatalogue.Tests.Repositories;

public class CategoriesRepositoryTests : InMemoryRepositoryTestsBase<StoretItemsCatalogueDbContext, CategoriesRepository>
{
	public CategoriesRepositoryTests (ITestOutputHelper output) : base (output) {}
	
	protected override CategoriesRepository InitRepository ()
	{
		return new CategoriesRepository (context);
	}
	
	protected override StoretItemsCatalogueDbContext InitDbContextWithOptions (DbContextOptions<StoretItemsCatalogueDbContext> options)
	{
		return new StoretItemsCatalogueDbContext (options);
	}
	
	[Fact]
	public async Task InsertAsyncWithValidCategoryShouldAddToDatabase ()
	{
		var userId = Guid.NewGuid();
		var category = new Category
		{
			Label = "Electronics",
			UserId = userId
		};

		var result = await repository.InsertAsync (category);

		result.Should().NotBeNull();
		result.Id.Should().BeGreaterThan (0);
		result.Label.Should().Be ("Electronics");

		var saved = await context.Categories.FindAsync (result.Id);
		saved.Should().NotBeNull();
	}
	
	[Fact]
	public async Task InsertAsyncWithParentCategoryShouldAddToDatabase ()
	{
		var userId = Guid.NewGuid();
		var parent = new Category {Label = "Electronics", UserId = userId};
		await context.Categories.AddAsync (parent);
		await context.SaveChangesAsync();

		var child = new Category
		{
			Label = "Phones",
			ParentCategoryId = parent.Id,
			UserId = userId
		};

		var result = await repository.InsertAsync (child);

		result.Should().NotBeNull();
		result.ParentCategoryId.Should().Be (parent.Id);

		var saved = await context.Categories
									.Include (c => c.ParentCategory)
									.FirstOrDefaultAsync (c => c.Id == result.Id);
		
		saved.Should().NotBeNull();
		saved.ParentCategory.Should().NotBeNull();
		saved.ParentCategory.Label.Should().Be ("Electronics");
	}

	[Fact]
	public async Task GetOneWithExistingIdShouldReturnCategory ()
	{
		var userId = Guid.NewGuid();
		var category = new Category
		{
			Label = "Electronics",
			UserId = userId
		};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();

		var result = await repository.GetOneAsync (category.Id, userId);

		result.Should().NotBeNull();
		result.Id.Should().Be (category.Id);
		result.Label.Should().Be (category.Label);
		result.UserId.Should().Be (userId);
	}
	
	[Fact]
	public async Task GetOneWithNonExistingIdShouldReturnNull ()
	{
		var result = await repository.GetOneAsync (999, Guid.NewGuid());

		result.Should().BeNull();
	}
	
	[Fact]
	public async Task GetOneWithExistingIdButNotOwnedByUserShouldReturnNull ()
	{
		var user1Id = Guid.NewGuid();
		var user2Id = Guid.NewGuid();
		
		var category = new Category
		{
			Id = 1,
			Label = "Electronics",
			UserId = user1Id
		};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();
		
		var result = await repository.GetOneAsync (1, user2Id);

		result.Should().BeNull();
	}
	
	[Fact]
	public async Task ExistsAsyncWithExistingIdShouldReturnTrue ()
	{
		var userId = Guid.NewGuid();
		var category = new Category
		{
			Label = "Electronics",
			UserId = userId
		};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();
		
		var result = await repository.AllExistAsync (userId, [category.Id]);
		
		result.Should().BeTrue();
	}
	
	[Fact]
	public async Task ExistsAsyncWithNonExistingIdShouldReturnFalse ()
	{
		var userId = Guid.NewGuid();
		var category = new Category
		{
			Label = "Electronics",
			UserId = userId
		};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();
		
		var result = await repository.AllExistAsync (userId, [category.Id, 999]);
		
		result.Should().BeFalse();
	}
	
	[Fact]
	public async Task AllExistAsyncWithExistingIdsButNotForUserShouldReturnFalse ()
	{
		var user1Id = Guid.NewGuid();
		var user2Id = Guid.NewGuid();
		
		var category1 = new Category
		{
			Label = "Electronics",
			UserId = user1Id
		};
		var category2 = new Category
		{
			Label = "Food",
			UserId = user1Id
		};
		var category3 = new Category
		{
			Label = "Food",
			UserId = user2Id
		};
		await context.Categories.AddRangeAsync (category1, category2, category3);
		await context.SaveChangesAsync();
		
		var result = await repository.AllExistAsync (user1Id, [category1.Id, category3.Id]);
		
		result.Should().BeFalse();
	}
	
	[Fact]
	public async Task GetAllShouldReturnFlatList ()
	{
		var userId = Guid.NewGuid();
		var user2Id = Guid.NewGuid();
		
		var electronics = new Category {Id = 1, Label = "Electronics", UserId = userId};
		await context.Categories.AddAsync (electronics);

		var phones = new Category {Id = 2, Label = "Phones", ParentCategoryId = electronics.Id, UserId = userId};
		var computers = new Category {Id = 3, Label = "Phones", ParentCategoryId = electronics.Id, UserId = userId};

		var iphone = new Category {Id = 4, Label = "iPhone", ParentCategoryId = phones.Id, UserId = userId};
		var android = new Category {Id = 5, Label = "Android", ParentCategoryId = phones.Id, UserId = userId};
		
		var food = new Category {Id = 7, Label = "Food", UserId = user2Id};
		
		await context.Categories.AddRangeAsync (electronics, phones, computers, iphone, android, food);
		await context.SaveChangesAsync();

		var result = await repository.GetAllAsync (userId);

		result.Should().NotBeNull();
		result.Should().HaveCount (5);
		result.First().Label.Should().Be ("Electronics");
		result.Should().AllSatisfy (c => c.UserId.Should().Be (userId));
	}

	[Fact]
	public async Task UpdateAsyncWithValidDataShouldUpdateCategory ()
	{
		var userId = Guid.NewGuid();
		var category = new Category {Label = "Electronics", UserId = userId};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();

		category.Label = "Updated Electronics";

		var result = await repository.UpdateAsync (category.Id, userId, category);

		result.Should().NotBeNull();
		result.Label.Should().Be ("Updated Electronics");

		var updated = await context.Categories.FindAsync (category.Id);
		updated.Should().NotBeNull();
		updated.Label.Should().Be ("Updated Electronics");
	}

	[Fact]
	public async Task DeletAsyncWithExistsingIdShouldRemoveCategory ()
	{
		var userId = Guid.NewGuid();
		var category = new Category {Label = "Electronics", UserId = userId};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();

		var result = await repository.DeleteAsync (category.Id, userId);
		result.Should().BeTrue();

		var deleted = await context.Categories.FindAsync (category.Id);
		deleted.Should().BeNull();
	}
	
	[Fact]
	public async Task DeleteAsyncWithNonExistsingIdShouldReturnFalse ()
	{
		var result = await repository.DeleteAsync (999, Guid.NewGuid());
		result.Should().BeFalse();
	}
	
	[Fact]
	public async Task DeleteAsyncWithParentCategoryShouldThrowWhenHasChildren ()
	{
		var userId = Guid.NewGuid();
		var parent = new Category {Id = 1, Label = "Electronics", UserId = userId};
		var child = new Category {Id = 2, Label = "Phones", ParentCategoryId = parent.Id, UserId = userId};
		await context.Categories.AddAsync (parent);
		await context.Categories.AddAsync (child);
		await context.SaveChangesAsync();

		Func<Task> act = async () => await repository.DeleteAsync (parent.Id, userId);

		await act.Should()
					.ThrowAsync<InvalidOperationException>()
					.WithMessage ("Cannot delete category with subcategories");

		var parentStillExists = await context.Categories.FindAsync (parent.Id);
		parentStillExists.Should().NotBeNull();
		
		var childStillExists = await context.Categories.FindAsync (child.Id);
		childStillExists.Should().NotBeNull();
	}
}