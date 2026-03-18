using Storet.Core.Service;
using Storet.Modules.ItemsCatalogue.Contracts.Categories;

namespace Storet.Modules.ItemsCatalogue.Services.Categories;

public interface ICategoriesService : ICrudService<CategoryResponseWithSubCategories, CategoryResponseWithParent, CategoryInsertRequest, CategoryUpdateRequest, int>
{}