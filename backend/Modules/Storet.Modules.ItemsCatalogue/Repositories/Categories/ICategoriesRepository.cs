using Storet.Core.Repository;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Repositories.Categories;

public interface ICategoriesRepository : ICrudRepository<Category, int>, IBulkExistenceCheckable<int>
{
	public Task<IEnumerable<CategoryHierarchy>> GetAllWithDepthAsync ();
}