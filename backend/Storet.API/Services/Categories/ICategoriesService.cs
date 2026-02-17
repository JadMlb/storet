using Storet.API.Contracts.Categories;
using Storet.API.Services.Base;

namespace Storet.API.Services.Categories;

public interface ICategoriesService : ICrudService<CategoryRequest, CategoryInsertRequest, CategoryUpdateRequest, int>
{}