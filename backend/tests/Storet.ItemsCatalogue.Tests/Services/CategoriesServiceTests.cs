using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Storet.API.ItemsCatalogue.Contracts.Categories;
using Storet.API.ItemsCatalogue.Mappers;
using Storet.API.ItemsCatalogue.Models;
using Storet.API.ItemsCatalogue.Repositories.Categories;
using Storet.API.ItemsCatalogue.Services.Categories;

namespace Storet.ItemsCatalogue.Tests.Services;

public class CategoriesServiceTests
{
	private readonly Mock<ICategoriesRepository> mockRepository;
	private readonly IMapper mapper;
	private readonly CategoriesService service;
	private readonly ILoggerFactory loggerFactory;

	public CategoriesServiceTests ()
	{
		mockRepository = new Mock<ICategoriesRepository>();
		loggerFactory = new LoggerFactory();
		
		var config = new MapperConfiguration (cfg => cfg.AddProfile<MappingProfile>(), loggerFactory);
		mapper = config.CreateMapper();
		service = new CategoriesService (mockRepository.Object, mapper);
	}

	[Fact]
	public async Task CreateCategoryAsyncWithValidDataShouldCreateCategory ()
	{
		var createDto = new CategoryInsertRequest
		{
			Label = "Electronics"
		};

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

		mockRepository.Verify (
			r => r.InsertAsync (It.Is<Category> (c => c.Label == "Electronics")),
			Times.Once()
		);
	}
	
	[Fact]
	public async Task CreateCategoryAsyncWithParentIdShouldVerifyParentExists ()
	{
		var createDto = new CategoryInsertRequest
		{
			Label = "Phones",
			ParentCategoryId = 1
		};

		var parentCategory = new Category {Id = 1, Label = "Electronics"};

		mockRepository.Setup (r => r.GetOneAsync (1))
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
		
		mockRepository.Verify (r => r.GetOneAsync (1), Times.Once());
		mockRepository.Verify (
			r => r.InsertAsync (
				It.Is<Category> (
					c => c.Label == "Phones" && c.ParentCategoryId == 1
				)
			),
			Times.Once()
		);
		mockRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task CreateCategoryAsyncWithNonExistingParentShouldThrowExceptionAndNotCreate ()
	{
		var createDto = new CategoryInsertRequest
		{
			Label = "Phones",
			ParentCategoryId = 999
		};

		mockRepository.Setup (r => r.GetOneAsync (999))
						.ReturnsAsync ((Category?) null);

		Func<Task> act = async () => await service.InsertAsync (createDto);

		await act.Should().ThrowAsync<InvalidOperationException>()
							.WithMessage ("Parent category is not found");
		
		mockRepository.Verify (r => r.GetOneAsync (999), Times.Once());
		mockRepository.Verify (r => r.InsertAsync (It.IsAny<Category>()), Times.Never());
		mockRepository.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task CreateCategoryAsyncWithoutParentShouldCreateRootCategory ()
	{
		var createDto = new CategoryInsertRequest
		{
			Label = "Electronics"
		};

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

		mockRepository.Verify (r => r.GetOneAsync (It.IsAny<int>()), Times.Never());
		mockRepository.Verify (
			r => r.InsertAsync (
				It.Is<Category> (c => c.Label == "Electronics" && c.ParentCategoryId == null)
			),
			Times.Once()
		);
		mockRepository.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetAllShouldReturnValidHierarchy ()
	{
		var categories = new List<CategoryHierarchy> 
		{
			new () {Id = 4, Label = "iPhone", ParentCategoryId = 2, Level = 2},
			new () {Id = 5, Label = "Android", ParentCategoryId = 2, Level = 2},
			new () {Id = 2, Label = "Phones", ParentCategoryId = 1, Level = 1},
			new () {Id = 3, Label = "Computers", ParentCategoryId = 1, Level = 1},
			new () {Id = 6, Label = "Food", Level = 0},
			new () {Id = 1, Label = "Electronics", Level = 0}
		};

		mockRepository.Setup (r => r.GetAllWithDepthAsync())
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

		mockRepository.Verify (r => r.GetAllWithDepthAsync(), Times.Once());
		mockRepository.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetCategoryByIdAsyncWithExistingIdShouldReturnCategory ()
	{
		var category = new Category
		{
			Id = 1,
			Label = "Phones",
			ParentCategoryId = 1,
			ParentCategory = new Category {Id = 2, Label = "Electronics"}
		};

		mockRepository.Setup (r => r.GetOneAsync (1))
						.ReturnsAsync (category);

		var result = await service.GetOneAsync (1);

		result.Should().NotBeNull();
		result.Id.Should().Be (1);
		result.Label.Should().Be ("Phones");
		result.ParentCategory.Should().NotBeNull();

		mockRepository.Verify (r => r.GetOneAsync (1), Times.Once());
		mockRepository.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task GetCategoryByIdAsyncWithNonExistingIdShouldReturnNull ()
	{
		mockRepository.Setup (r => r.GetOneAsync (999))
						.ReturnsAsync ((Category?) null);

		var result = await service.GetOneAsync (999);

		result.Should().BeNull();

		mockRepository.Verify (r => r.GetOneAsync (999), Times.Once());
		mockRepository.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task UpdateCategoryAsyncWithValidDataShouldUpdateCategory ()
	{
		var category = new Category
		{
			Id = 1,
			Label = "Phones"
		};

		var updateDto = new CategoryUpdateRequest
		{
			Label = "Updated Phones",
			ParentCategoryId = 2
		};

		var parentCategory = new Category
		{
			Id = 2,
			Label = "Electronics"
		};

		mockRepository.Setup (r => r.GetOneAsync (1))
						.ReturnsAsync (category);
		mockRepository.Setup (r => r.GetOneAsync (2))
						.ReturnsAsync (parentCategory);
		mockRepository.Setup (r => r.UpdateAsync (1, It.IsAny<Category>()))
						.ReturnsAsync (
							(int id, Category c) =>
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

		mockRepository.Verify (r => r.GetOneAsync (1), Times.Once());
		mockRepository.Verify (r => r.GetOneAsync (2), Times.Once());
		mockRepository.Verify (
			r => r.UpdateAsync (
				1,
				It.Is<Category> (
					c => c.Label == "Updated Phones"
						&& c.ParentCategoryId == 2
				)
			),
			Times.Once()
		);
		mockRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task UpdateCategoryAsyncWithNonExistentParentShouldThrow ()
	{
		var category = new Category
		{
			Id = 1,
			Label = "Phones"
		};

		var updateDto = new CategoryUpdateRequest
		{
			Label = "Updated Phones",
			ParentCategoryId = 999
		};

		mockRepository.Setup (r => r.GetOneAsync (1))
						.ReturnsAsync (category);
		mockRepository.Setup (r => r.GetOneAsync (999))
						.ReturnsAsync ((Category?) null);
		
		Func<Task> act = async () => await service.UpdateAsync (1, updateDto);
		
		await act.Should()
					.ThrowAsync<InvalidOperationException>()
					.WithMessage ("Parent category is not found");

		mockRepository.Verify (r => r.GetOneAsync (1), Times.Once());
		mockRepository.Verify (r => r.GetOneAsync (999), Times.Once());
		mockRepository.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task DeleteCategoryAsyncWhenDeleteSucceedsShouldReturnTrue ()
	{
		mockRepository.Setup (r => r.DeleteAsync (1))
						.ReturnsAsync (true);
		var result = await service.DeleteAsync (1);

		result.Should().BeTrue();

		mockRepository.Verify (r => r.DeleteAsync (1), Times.Once());
		mockRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteCategoryAsyncWhenDeleteFailsShouldReturnFalse ()
	{
		mockRepository.Setup (r => r.DeleteAsync (1))
						.ReturnsAsync (false);

		var result = await service.DeleteAsync (1);

		result.Should().BeFalse();

		mockRepository.Verify (r => r.DeleteAsync (1), Times.Once());
		mockRepository.VerifyNoOtherCalls();
	}
	
	[Fact]
	public async Task DeleteCategoryAsyncWithCategoryThatHasChildrenShouldRethrowException ()
	{
		var expectedException = new InvalidOperationException ("Cannot delete category with subcategories");
		mockRepository.Setup (r => r.DeleteAsync (1))
						.ThrowsAsync (expectedException);

		Func<Task> act = async () => await service.DeleteAsync (1);

		await act.Should()
					.ThrowAsync<InvalidOperationException>()
					.WithMessage ("Cannot delete category with subcategories");

		mockRepository.Verify (r => r.DeleteAsync (1), Times.Once());
		mockRepository.VerifyNoOtherCalls();
	}
}