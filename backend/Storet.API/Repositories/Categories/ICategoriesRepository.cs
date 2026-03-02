using Storet.API.Models;
using Storet.API.Repositories.Base;

namespace Storet.API.Repositories.Categories;

public interface ICategoriesRepository : ICrudRepository<Category, int>, IExistenceCheckable<int>
{
	public Task<IEnumerable<CategoryHierarchy>> GetAllWithDepthAsync ();
}