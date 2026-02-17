using Storet.API.Contracts.Categories;
using Storet.API.Models;

namespace Storet.API.Mappers;

public static class CategoryMappers
{
	public static CategoryRequest ToRequestDTO (this Category category)
	{
		return new CategoryRequest
		{
			Id = category.Id,
			Label = category.Label,
			ParentCategory = category.ParentCategory?.ToRequestDTO(),
			SubCategories = category.SubCategories?.Select (sc => sc.ToRequestDTO()) ?? []
		};
	}

	public static Category ToModel (this CategoryInsertRequest request)
	{
		return new Category
		{
			Label = request.Label,
			ParentCategoryId = request.ParentCategoryId
		};
	}
}