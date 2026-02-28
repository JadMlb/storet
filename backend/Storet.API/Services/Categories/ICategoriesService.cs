using Storet.API.Contracts.Categories;
using Storet.API.Services.Base;

namespace Storet.API.Services.Categories;

public interface ICategoriesService : ICrudService<CategoryResponseWithSubCategories, CategoryResponseWithParent, CategoryInsertRequest, CategoryUpdateRequest, int>
{}