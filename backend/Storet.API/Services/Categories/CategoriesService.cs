using Storet.API.Contracts.Categories;
using Storet.API.Mappers;
using Storet.API.Repositories.Categories;

namespace Storet.API.Services.Categories;

public class CategoriesService : ICategoriesService
{
	private readonly ICategoriesRepository repository;

	public CategoriesService (ICategoriesRepository repository)
	{
		this.repository = repository;
	}

	public async Task<IEnumerable<CategoryRequest>> GetAllAsync ()
	{
		var categories = await repository.GetAllAsync();
		return categories?.Select (c => c.ToRequestDTO()) ?? [];
	}

	public async Task<CategoryRequest?> GetOneAsync (int key)
	{
		var category = await repository.GetOneAsync (key);
		return category?.ToRequestDTO();
	}

	public async Task<CategoryRequest?> InsertAsync (CategoryInsertRequest model)
	{
		var category = await repository.InsertAsync (model.ToModel());
		return category?.ToRequestDTO();
	}

	public async Task<CategoryRequest?> UpdateAsync (int key, CategoryUpdateRequest model)
	{
		var existingCategory = await repository.GetOneAsync (key);
		if (existingCategory == null)
			return null;
		
		existingCategory.Label = model.Label ?? existingCategory.Label;
		existingCategory.ParentCategoryId = model.ParentCategoryId ?? existingCategory.ParentCategoryId;

		var updatedCategory = await repository.UpdateAsync (key, existingCategory);
		return updatedCategory?.ToRequestDTO();
	}

	public async Task<CategoryRequest?> DeleteAsync (int key)
	{
		var category = await repository.DeleteAsync (key);
		return category?.ToRequestDTO();
	}
}