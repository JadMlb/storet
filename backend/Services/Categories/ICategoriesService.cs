using Storet.Backend.Contracts.Categories;
using Storet.Backend.Services.Base;

namespace Storet.Backend.Services.Categories;

public interface ICategoriesService : ICrudService<CategoryRequest, CategoryInsertRequest, CategoryUpdateRequest, int>
{}