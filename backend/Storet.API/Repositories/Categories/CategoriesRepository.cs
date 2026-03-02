using Microsoft.EntityFrameworkCore;
using Storet.API.Data;
using Storet.API.Models;
using Storet.API.Repositories.Base;

namespace Storet.API.Repositories.Categories;

public class CategoriesRepository : BaseRepository, ICategoriesRepository
{
	public CategoriesRepository (StoretDbContext context) : base (context) {}

	public async Task<IEnumerable<Category>> GetAllAsync ()
	{
		return await context.Categories
							.AsNoTracking()
							.ToListAsync();
	}

	public async Task<IEnumerable<CategoryHierarchy>> GetAllWithDepthAsync ()
	{
		return await context.CategoryHierarchies
							.AsNoTracking()
							.ToListAsync();
	}

	public async Task<Category?> GetOneAsync (int key)
	{
		return await context.Categories
							.AsNoTracking()
							.Include (c => c.ParentCategory)
							.FirstOrDefaultAsync (c => c.Id == key);
	}
	
	public async Task<bool> ExistsAsync (int key)
	{
		return await context.Categories
							.AsNoTracking()
							.AnyAsync (c => c.Id == key);
	}

	public async Task<Category?> InsertAsync (Category model)
	{
		await context.Categories.AddAsync (model);
		try
		{
			await context.SaveChangesAsync();
			return await context.Categories
								.AsNoTracking()
								.Include (c => c.ParentCategory)
								.FirstOrDefaultAsync (c => c.Id == model.Id);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public async Task<Category?> UpdateAsync (int key, Category model)
	{
		var old = await context.Categories.FirstOrDefaultAsync (c => c.Id == key);
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

	public async Task<bool> DeleteAsync (int key)
	{
		var old = await context.Categories
								.Include (c => c.SubCategories)
								.FirstOrDefaultAsync (c => c.Id == key);
		
		if (old == null)
			return false;

		if (old.SubCategories.Any())
			throw new InvalidOperationException ("Cannot delete category with subcategories");
		
		try
		{
			context.Categories.Remove (old);
			await context.SaveChangesAsync();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}