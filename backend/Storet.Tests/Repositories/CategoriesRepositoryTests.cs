using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storet.API.Data;
using Storet.API.Models;
using Storet.API.Repositories.Categories;

namespace Storet.Tests.Repositories;

public class CategoriesRepositoryTests : IDisposable
{
	private readonly StoretDbContext context;
	private readonly CategoriesRepository repository;

	public CategoriesRepositoryTests ()
	{
		var options = new DbContextOptionsBuilder<StoretDbContext>()
							.UseInMemoryDatabase (databaseName: Guid.NewGuid().ToString())
							.Options;
		
		context = new StoretDbContext (options);
		repository = new CategoriesRepository (context);
	}

	[Fact]
	public async Task InsertAsyncWithValidCategoryShouldAddToDatabase ()
	{
		var category = new Category
		{
			Label = "Electronics"
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
		var parent = new Category {Label = "Electronics"};
		await context.Categories.AddAsync (parent);
		await context.SaveChangesAsync();

		var child = new Category
		{
			Label = "Phones",
			ParentCategoryId = parent.Id
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
		var category = new Category
		{
			Label = "Electronics"
		};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();

		var result = await repository.GetOneAsync (category.Id);

		result.Should().NotBeNull();
		result.Id.Should().Be (category.Id);
		result.Label.Should().Be (category.Label);
	}
	
	[Fact]
	public async Task GetOneWithNonExistingIdShouldReturnNull ()
	{
		var result = await repository.GetOneAsync (999);

		result.Should().BeNull();
	}
	
	[Fact]
	public async Task ExistsAsyncWithExistingIdShouldReturnTrue ()
	{
		var category = new Category
		{
			Label = "Electronics"
		};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();
		
		var result = await repository.ExistsAsync (category.Id);
		
		result.Should().BeTrue();
	}
	
	[Fact]
	public async Task ExistsAsyncWithNonExistingIdShouldReturnFalse ()
	{
		var result = await repository.ExistsAsync (999);
		
		result.Should().BeFalse();
	}
	
	[Fact]
	public async Task GetAllShouldReturnFlatList ()
	{
		var electronics = new Category {Id = 1, Label = "Electronics"};
		await context.Categories.AddAsync (electronics);

		var phones = new Category {Id = 2, Label = "Phones", ParentCategoryId = electronics.Id};
		var computers = new Category {Id = 3, Label = "Phones", ParentCategoryId = electronics.Id};

		var iphone = new Category {Id = 4, Label = "iPhone", ParentCategoryId = phones.Id};
		var android = new Category {Id = 5, Label = "Android", ParentCategoryId = phones.Id};
		await context.Categories.AddRangeAsync (electronics, phones, computers, iphone, android);
		await context.SaveChangesAsync();

		var result = await repository.GetAllAsync();

		result.Should().NotBeNull();
		result.Should().HaveCount (5);
		result.First().Label.Should().Be ("Electronics");
	}

	[Fact]
	public async Task UpdateAsyncWithValidDataShouldUpdateCategory ()
	{
		var category = new Category {Label = "Electronics"};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();

		category.Label = "Updated Electronics";

		var result = await repository.UpdateAsync (category.Id, category);

		result.Should().NotBeNull();
		result.Label.Should().Be ("Updated Electronics");

		var updated = await context.Categories.FindAsync (category.Id);
		updated.Should().NotBeNull();
		updated.Label.Should().Be ("Updated Electronics");
	}

	[Fact]
	public async Task DeletAsyncWithExistsingIdShouldRemoveCategory ()
	{
		var category = new Category {Label = "Electronics"};
		await context.Categories.AddAsync (category);
		await context.SaveChangesAsync();

		var result = await repository.DeleteAsync (category.Id);
		result.Should().BeTrue();

		var deleted = await context.Categories.FindAsync (category.Id);
		deleted.Should().BeNull();
	}
	
	[Fact]
	public async Task DeleteAsyncWithNonExistsingIdShouldReturnFalse ()
	{
		var result = await repository.DeleteAsync (999);
		result.Should().BeFalse();
	}
	
	[Fact]
	public async Task DeleteAsyncWithParentCategoryShouldThrowWhenHasChildren ()
	{
		var parent = new Category {Id = 1, Label = "Electronics"};
		var child = new Category {Id = 2, Label = "Phones", ParentCategoryId = parent.Id};
		await context.Categories.AddAsync (parent);
		await context.Categories.AddAsync (child);
		await context.SaveChangesAsync();

		Func<Task> act = async () => await repository.DeleteAsync (parent.Id);

		await act.Should()
					.ThrowAsync<InvalidOperationException>()
					.WithMessage ("Cannot delete category with subcategories");

		var parentStillExists = await context.Categories.FindAsync (parent.Id);
		parentStillExists.Should().NotBeNull();
		
		var childStillExists = await context.Categories.FindAsync (child.Id);
		childStillExists.Should().NotBeNull();
	}

	public void Dispose ()
	{
		context.Database.EnsureDeleted();
		context.Dispose();
	}
}