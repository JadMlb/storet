using Microsoft.EntityFrameworkCore;
using Storet.Core.Exceptions;
using Storet.Core.Repository;
using Storet.Modules.ItemsCatalogue.Data;
using Storet.Modules.ItemsCatalogue.Models;

namespace Storet.Modules.ItemsCatalogue.Repositories.Categories;

public class CategoriesRepository : BaseRepository<StoretItemsCatalogueDbContext>, ICategoriesRepository
{
	public CategoriesRepository (StoretItemsCatalogueDbContext context) : base (context) {}

	public async Task<IEnumerable<Category>> GetAllAsync (Guid userId)
	{
		return await context.Categories
							.AsNoTracking()
							.Where (c => c.UserId == userId)
							.ToListAsync();
	}

	public async Task<IEnumerable<CategoryHierarchy>> GetAllWithDepthAsync (Guid userId)
	{
		return await context.CategoryHierarchies
							.AsNoTracking()
							.Where (c => c.UserId == userId)
							.ToListAsync();
	}

	public async Task<Category?> GetOneAsync (int key, Guid userId)
	{
		return await context.Categories
							.AsNoTracking()
							.Where (c => c.UserId == userId)
							.Include (c => c.ParentCategory)
							.FirstOrDefaultAsync (c => c.Id == key);
	}
	
	public async Task<bool> AllExistAsync (Guid userId, IEnumerable<int> keys)
	{
		var keysSet = keys.ToHashSet();
		var numberOfExistsingIdsInDb = await context.Categories
													.AsNoTracking()
													.Where (c => c.UserId == userId)
													.CountAsync (c => keysSet.Contains (c.Id));
		return keysSet.Count == numberOfExistsingIdsInDb;
	}

	public async Task<Category?> InsertAsync (Category model)
	{
		await context.Categories.AddAsync (model);
		try
		{
			await context.SaveChangesAsync();
			return await context.Categories
								.AsNoTracking()
								.Where (c => c.UserId == model.UserId)
								.Include (c => c.ParentCategory)
								.FirstOrDefaultAsync (c => c.Id == model.Id);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public async Task<Category?> UpdateAsync (int key, Guid userId, Category model)
	{
		var old = await context.Categories.FirstOrDefaultAsync (c => c.Id == key && c.UserId == userId);
		if (old == null)
			return null;
		
		old.Label = model.Label;
		old.ParentCategoryId = model.ParentCategoryId;
		
		try
		{
			await context.SaveChangesAsync();
			return old;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public async Task<bool> DeleteAsync (int key, Guid userId)
	{
		var old = await context.Categories
								.Where (c => c.UserId == userId)
								.Include (c => c.SubCategories)
								.FirstOrDefaultAsync (c => c.Id == key);
		
		if (old == null)
			return false;

		if (old.SubCategories.Count > 0)
			throw new EntityDependencyException (nameof (Category), key);

		context.Categories.Remove (old);
		await context.SaveChangesAsync();
		return true;
	}
}