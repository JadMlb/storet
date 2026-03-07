using Storet.API.ItemsCatalogue.Models;
using Storet.API.Core.Repository;

namespace Storet.API.ItemsCatalogue.Repositories.Categories;

public interface ICategoriesRepository : ICrudRepository<Category, int>, IBulkExistenceCheckable<int>
{
	public Task<IEnumerable<CategoryHierarchy>> GetAllWithDepthAsync ();
}