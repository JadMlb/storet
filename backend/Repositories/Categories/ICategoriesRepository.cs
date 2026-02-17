using Storet.Backend.Models;
using Storet.Backend.Repositories.Base;

namespace Storet.Backend.Repositories.Categories;

public interface ICategoriesRepository : ICrudRepository<Category, int>
{}