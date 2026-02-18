using AutoMapper;
using Storet.API.Contracts.Categories;
using Storet.API.Models;
using Storet.API.Repositories.Categories;

namespace Storet.API.Services.Categories;

public class CategoriesService : ICategoriesService
{
	private readonly ICategoriesRepository repository;
	private readonly IMapper mapper;

	public CategoriesService (ICategoriesRepository repository, IMapper mapper)
	{
		this.repository = repository;
		this.mapper = mapper;
	}

	public async Task<IEnumerable<CategoryResponse>> GetAllAsync ()
	{
		var categoriesWithLevels = await repository.GetAllWithDepthAsync();

		if (categoriesWithLevels == null)
			return [];

		var roots = new List<Category>();

		// dict that maps the ids of parent categories to the list of children
		var childrenDict = new Dictionary<int, List<Category>>();
		foreach (var categoryHierarchy in categoriesWithLevels)
		{
			var category = mapper.Map<Category> (categoryHierarchy);

			// check if current node has children registered in childern dict
			if (childrenDict.TryGetValue (categoryHierarchy.Id, out var childrenForCategory))
			{
				category.SubCategories = childrenForCategory;

				// clear entry
				childrenDict.Remove (category.Id);
			}

			if (categoryHierarchy.Level == 0)
			{
				roots.Add (category);
			}
			else if (childrenDict.TryGetValue (categoryHierarchy.ParentCategoryId, out var childrenForParentCategory))
			{
				childrenForParentCategory.Add (category);
			}
			else
				childrenDict[categoryHierarchy.ParentCategoryId] = [category];
		}
		
		return roots.Select (mapper.Map<CategoryResponse>);
	}

	public async Task<CategoryResponse?> GetOneAsync (int key)
	{
		var category = await repository.GetOneAsync (key);
		return mapper.Map<CategoryResponse> (category);
	}

	public async Task<CategoryResponse?> InsertAsync (CategoryInsertRequest model)
	{
		if (model.ParentCategoryId.HasValue)
		{
			var _ = await repository.GetOneAsync (model.ParentCategoryId.Value) ??
						throw new InvalidOperationException ("Parent category is not found");
		}

		var category = await repository.InsertAsync (mapper.Map<Category> (model));
		return mapper.Map<CategoryResponse> (category);
	}

	public async Task<CategoryResponse?> UpdateAsync (int key, CategoryUpdateRequest model)
	{
		var existingCategory = await repository.GetOneAsync (key);
		if (existingCategory == null)
			return null;

		if (model.ParentCategoryId.HasValue)
		{
			var _ = await repository.GetOneAsync (model.ParentCategoryId.Value) ??
						throw new InvalidOperationException ("Parent category is not found");
		}
		
		existingCategory.Label = model.Label ?? existingCategory.Label;
		existingCategory.ParentCategoryId = model.ParentCategoryId ?? existingCategory.ParentCategoryId;

		var updatedCategory = await repository.UpdateAsync (key, existingCategory);
		return mapper.Map<CategoryResponse> (updatedCategory);
	}

	public async Task<bool> DeleteAsync (int key)
	{
		try
		{
			return await repository.DeleteAsync (key);
		}
		catch (InvalidOperationException)
		{
			throw;
		}
		catch (Exception)
		{
			return false;
		}
	}
}