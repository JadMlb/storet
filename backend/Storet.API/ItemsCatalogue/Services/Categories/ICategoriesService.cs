using Storet.API.Core.Service;
using Storet.API.ItemsCatalogue.Contracts.Categories;

namespace Storet.API.ItemsCatalogue.Services.Categories;

public interface ICategoriesService : ICrudService<CategoryResponseWithSubCategories, CategoryResponseWithParent, CategoryInsertRequest, CategoryUpdateRequest, int>
{}