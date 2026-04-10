using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Storet.Core.Authorization;
using Storet.Core.Exceptions;
using Storet.Core.Mappers;
using Storet.Modules.ItemsCatalogue.Contracts.Categories;
using Storet.Modules.ItemsCatalogue.Contracts.Items;
using Storet.Modules.ItemsCatalogue.Mappers;
using Storet.Modules.ItemsCatalogue.Models;
using Storet.Modules.ItemsCatalogue.Repositories.Categories;
using Storet.Modules.ItemsCatalogue.Services.Categories;

namespace Storet.ItemsCatalogue.Tests.Services;

public class CategoriesServiceTests
{
	private readonly Mock<ICategoriesRepository> mockRepository;
	private readonly Mock<ICurrentUser> mockCurrentUser;
	private readonly IMapper mapper;
	private readonly CategoriesService service;
	private readonly ILoggerFactory loggerFactory;

	public CategoriesServiceTests ()
	{
		mockRepository = new Mock<ICategoriesRepository>();
		mockCurrentUser = new Mock<ICurrentUser>();
		loggerFactory = new LoggerFactory();
		
		var categoryInsertRequestResolver = new CurrentUserResolver<CategoryInsertRequest, Category> (mockCurrentUser.Object);
		var categoryUpdateRequestResolver = new CurrentUserResolver<CategoryUpdateRequest, Category> (mockCurrentUser.Object);
		var itemInsertRequestResolver = new CurrentUserResolver<ItemInsertRequest, Item> (mockCurrentUser.Object);
		var itemUpdateRequestResolver = new CurrentUserResolver<ItemUpdateRequest, Item> (mockCurrentUser.Object);

		var config = new MapperConfiguration (
			cfg =>
			{
				cfg.ConstructServicesUsing (
					type => type == typeof (CurrentUserResolver<CategoryInsertRequest, Category>) ? categoryInsertRequestResolver :
								type == typeof (CurrentUserResolver<CategoryUpdateRequest, Category>) ? categoryUpdateRequestResolver :
								type == typeof (CurrentUserResolver<ItemInsertRequest, Item>) ? itemInsertRequestResolver :
								type == typeof (CurrentUserResolver<ItemUpdateRequest, Item>) ? itemUpdateRequestResolver :
								null
				);
				cfg.AddProfile<MappingProfile>();
			},
			loggerFactory
		);
		mapper = config.CreateMapper();
		service = new CategoriesService (mockRepository.Object, mockCurrentUser.Object, mapper);
	}

	[Fact]
	public async Task CreateCategoryAsyncWithValidDataShouldCreateCategory ()
	{
		var createDto = new CategoryInsertRequest
		{
			Label = "Electronics"
		};

		var userId = Guid.NewGuid();
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockRepository.Setup (r => r.InsertAsync (It.IsAny<Category>()))
						.ReturnsAsync (
							(Category c) =>
							{
								c.Id = 1;
								return c;
							}
						);

		var result = await service.InsertAsync (createDto);

		result.Should().NotBeNull();
		result.Id.Should().Be (1);
		result.Label.Should().Be ("Electronics");

		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockRepository.Verify (
			r => r.InsertAsync (It.Is<Category> (c => c.Label == "Electronics")),
			Times.Once()
		);
		mockCurrentUser.VerifyNoOtherCalls();
		mockRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateCategoryAsyncWithParentIdShouldVerifyParentExists ()
	{
		var createDto = new CategoryInsertRequest
		{
			Label = "Phones",
			ParentCategoryId = 1
		};

		var userId = Guid.NewGuid();
		var parentCategory = new Category {Id = 1, Label = "Electronics", UserId = userId};

		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockRepository.Setup (r => r.GetOneAsync (1, userId))
						.ReturnsAsync (parentCategory);
		mockRepository.Setup (r => r.InsertAsync (It.IsAny<Category>()))
						.ReturnsAsync (
							(Category c) =>
							{
								c.Id = 2;
								c.ParentCategory = parentCategory;
								return c;
							}
						);

		var result = await service.InsertAsync (createDto);

		result.Should().NotBeNull();
		result.Id.Should().Be (2);
		result.Label.Should().Be ("Phones");
		result.ParentCategory.Should().NotBeNull();
		result.ParentCategory.Id.Should().Be (1);
		result.ParentCategory.Label.Should().Be ("Electronics");
		
		mockCurrentUser.Verify (u => u.Id, Times.Exactly (2));
		mockRepository.Verify (r => r.GetOneAsync (1, userId), Times.Once());
		mockRepository.Verify (
			r => r.InsertAsync (
				It.Is<Category> (
					c => c.Label == "Phones" && c.ParentCategoryId == 1
				)
			),
			Times.Once()
		);
		mockRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateCategoryAsyncWithNonExistingParentShouldThrowExceptionAndNotCreate ()
	{
		var createDto = new CategoryInsertRequest
		{
			Label = "Phones",
			ParentCategoryId = 999
		};

		var userId = Guid.NewGuid();
		
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockRepository.Setup (r => r.GetOneAsync (999, userId))
						.ReturnsAsync ((Category?) null);

		Func<Task> act = async () => await service.InsertAsync (createDto);

		await act.Should().ThrowAsync<InvalidOperationException>()
							.WithMessage ("Parent category is not found");
		
		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockRepository.Verify (r => r.GetOneAsync (999, userId), Times.Once());
		mockRepository.Verify (r => r.InsertAsync (It.IsAny<Category>()), Times.Never());
		mockRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task CreateCategoryAsyncWithoutParentShouldCreateRootCategory ()
	{
		var createDto = new CategoryInsertRequest
		{
			Label = "Electronics"
		};

		var userId = Guid.NewGuid();
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockRepository.Setup (r => r.InsertAsync (It.IsAny<Category>()))
						.ReturnsAsync (
							(Category c) =>
							{
								c.Id = 1;
								return c;
							}
						);
		var result = await service.InsertAsync (createDto);

		result.Should().NotBeNull();
		result.Id.Should().Be (1);
		result.Label.Should().Be ("Electronics");
		result.ParentCategory.Should().BeNull();

		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockRepository.Verify (r => r.GetOneAsync (It.IsAny<int>(), userId), Times.Never());
		mockRepository.Verify (
			r => r.InsertAsync (
				It.Is<Category> (c => c.Label == "Electronics" && c.ParentCategoryId == null)
			),
			Times.Once()
		);
		mockRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetAllShouldReturnValidHierarchy ()
	{
		var userId = Guid.NewGuid();
		var categories = new List<CategoryHierarchy> 
		{
			new () {Id = 4, Label = "iPhone", ParentCategoryId = 2, Level = 2, UserId = userId},
			new () {Id = 5, Label = "Android", ParentCategoryId = 2, Level = 2, UserId = userId},
			new () {Id = 2, Label = "Phones", ParentCategoryId = 1, Level = 1, UserId = userId},
			new () {Id = 3, Label = "Computers", ParentCategoryId = 1, Level = 1, UserId = userId},
			new () {Id = 6, Label = "Food", Level = 0, UserId = userId},
			new () {Id = 1, Label = "Electronics", Level = 0, UserId = userId}
		};

		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockRepository.Setup (r => r.GetAllWithDepthAsync (userId))
						.ReturnsAsync (categories);
		
		var result = await service.GetAllAsync();

		result.Should().NotBeNull();
		result.Should().HaveCount (2);
		result.First().Label.Should().Be ("Food");
		result.ElementAt(1).Label.Should().Be ("Electronics");
		result.ElementAt(1).SubCategories.Should().HaveCount (2);
		result.ElementAt(1).SubCategories.Should().Contain (c => c.Label == "Phones");
		result.ElementAt(1).SubCategories.Should().Contain (c => c.Label == "Computers");

		var phonesNode = result.ElementAt(1).SubCategories.First (c => c.Label == "Phones");
		phonesNode.SubCategories.Should().HaveCount (2);
		phonesNode.SubCategories.Should().Contain (c => c.Label == "iPhone");
		phonesNode.SubCategories.Should().Contain (c => c.Label == "Android");

		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockRepository.Verify (r => r.GetAllWithDepthAsync (userId), Times.Once());
		mockRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetCategoryByIdAsyncWithExistingIdShouldReturnCategory ()
	{
		var userId = Guid.NewGuid();
		var category = new Category
		{
			Id = 1,
			Label = "Phones",
			ParentCategoryId = 1,
			ParentCategory = new Category {Id = 2, Label = "Electronics"},
			UserId = userId
		};

		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockRepository.Setup (r => r.GetOneAsync (1, userId))
						.ReturnsAsync (category);

		var result = await service.GetOneAsync (1);

		result.Should().NotBeNull();
		result.Id.Should().Be (1);
		result.Label.Should().Be ("Phones");
		result.ParentCategory.Should().NotBeNull();

		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockRepository.Verify (r => r.GetOneAsync (1, userId), Times.Once());
		mockRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetCategoryByIdAsyncWithNonExistingIdShouldReturnNull ()
	{
		var userId = Guid.NewGuid();
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockRepository.Setup (r => r.GetOneAsync (999, userId))
						.ReturnsAsync ((Category?) null);

		var result = await service.GetOneAsync (999);

		result.Should().BeNull();

		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockRepository.Verify (r => r.GetOneAsync (999, userId), Times.Once());
		mockRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task UpdateCategoryAsyncWithValidDataShouldUpdateCategory ()
	{
		var userId = Guid.NewGuid();
		var category = new Category
		{
			Id = 1,
			Label = "Phones",
			UserId = userId
		};

		var updateDto = new CategoryUpdateRequest
		{
			Label = "Updated Phones",
			ParentCategoryId = 2
		};

		var parentCategory = new Category
		{
			Id = 2,
			Label = "Electronics",
			UserId = userId
		};

		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockRepository.Setup (r => r.GetOneAsync (1, userId))
						.ReturnsAsync (category);
		mockRepository.Setup (r => r.GetOneAsync (2, userId))
						.ReturnsAsync (parentCategory);
		mockRepository.Setup (r => r.UpdateAsync (1, userId, It.IsAny<Category>()))
						.ReturnsAsync (
							(int id, Guid userId, Category c) =>
							{
								c.Id = id;
								c.ParentCategory = parentCategory;
								return c;
							}
						);
		
		var result = await service.UpdateAsync (1, updateDto);

		result.Should().NotBeNull();
		result.Id.Should().Be (1);
		result.Label.Should().Be ("Updated Phones");
		result.ParentCategory.Should().NotBeNull();
		result.ParentCategory.Id.Should().Be (2);

		mockCurrentUser.Verify (u => u.Id, Times.Exactly (3));
		mockRepository.Verify (r => r.GetOneAsync (1, userId), Times.Once());
		mockRepository.Verify (r => r.GetOneAsync (2, userId), Times.Once());
		mockRepository.Verify (
			r => r.UpdateAsync (
				1,
				userId,
				It.Is<Category> (
					c => c.Label == "Updated Phones"
						&& c.ParentCategoryId == 2
				)
			),
			Times.Once()
		);
		mockRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateCategoryAsyncWithNonExistentParentShouldThrow ()
	{
		var userId = Guid.NewGuid();
		var category = new Category
		{
			Id = 1,
			Label = "Phones",
			UserId = userId
		};

		var updateDto = new CategoryUpdateRequest
		{
			Label = "Updated Phones",
			ParentCategoryId = 999
		};

		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockRepository.Setup (r => r.GetOneAsync (1, userId))
						.ReturnsAsync (category);
		mockRepository.Setup (r => r.GetOneAsync (999, userId))
						.ReturnsAsync ((Category?) null);
		
		Func<Task> act = async () => await service.UpdateAsync (1, updateDto);
		
		await act.Should()
					.ThrowAsync<InvalidOperationException>()
					.WithMessage ("Parent category is not found");

		mockCurrentUser.Verify (u => u.Id, Times.Exactly (2));
		mockRepository.Verify (r => r.GetOneAsync (1, userId), Times.Once());
		mockRepository.Verify (r => r.GetOneAsync (999, userId), Times.Once());
		mockRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task DeleteCategoryAsyncWhenDeleteSucceedsShouldReturnTrue ()
	{
		var userId = Guid.NewGuid();
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockRepository.Setup (r => r.DeleteAsync (1, userId))
						.ReturnsAsync (true);
		var result = await service.DeleteAsync (1);

		result.Should().BeTrue();

		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockRepository.Verify (r => r.DeleteAsync (1, userId), Times.Once());
		mockRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteCategoryAsyncWhenDeleteFailsShouldReturnFalse ()
	{
		var userId = Guid.NewGuid();
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		mockRepository.Setup (r => r.DeleteAsync (1, userId))
						.ReturnsAsync (false);

		var result = await service.DeleteAsync (1);

		result.Should().BeFalse();

		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockRepository.Verify (r => r.DeleteAsync (1, userId), Times.Once());
		mockRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteCategoryAsyncWithCategoryThatHasChildrenShouldRethrowException ()
	{
		var userId = Guid.NewGuid();
		mockCurrentUser.Setup (u => u.Id)
						.Returns (userId);
		var expectedException = new EntityDependencyException (nameof (Category), 1);
		mockRepository.Setup (r => r.DeleteAsync (1, userId))
						.ThrowsAsync (expectedException);

		Func<Task> act = async () => await service.DeleteAsync (1);

		await act.Should()
					.ThrowAsync<EntityDependencyException>()
					.WithMessage ("Cannot delete Category with ID 1 because others depend on it");

		mockCurrentUser.Verify (u => u.Id, Times.Once());
		mockRepository.Verify (r => r.DeleteAsync (1, userId), Times.Once());
		mockRepository.VerifyNoOtherCalls();
		mockCurrentUser.VerifyNoOtherCalls();
	}
}