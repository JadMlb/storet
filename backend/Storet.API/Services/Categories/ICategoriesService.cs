using Storet.API.Contracts.Categories;
using Storet.API.Services.Base;

namespace Storet.API.Services.Categories;

public interface ICategoriesService : ICrudService<CategoryResponse, CategoryResponseWithParent, CategoryInsertRequest, CategoryUpdateRequest, int>
{}