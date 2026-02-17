using Microsoft.EntityFrameworkCore;
using Storet.Backend.Data;
using Storet.Backend.Models;
using Storet.Backend.Repositories.Base;

namespace Storet.Backend.Repositories.Categories;

public class CategoriesRepository : BaseRepository, ICategoriesRepository
{
	public CategoriesRepository (StoretDbContext context) : base (context) {}

	public async Task<IEnumerable<Category>> GetAllAsync ()
	{
		return await context.Categories.AsNoTracking().ToListAsync();
	}

	public async Task<Category?> GetOneAsync (int key)
	{
		return await context.Categories
							.AsNoTracking()
							.FirstOrDefaultAsync (c => c.Id == key);
	}

	public async Task<Category?> InsertAsync (Category model)
	{
		await context.Categories.AddAsync (model);
		try
		{
			await context.SaveChangesAsync();
			return model;
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

	public async Task<Category?> DeleteAsync (int key)
	{
		var old = await context.Categories.FirstOrDefaultAsync (c => c.Id == key);
		if (old == null)
			return null;
		try
		{
			context.Categories.Remove (old);
			await context.SaveChangesAsync();
			return old;
		}
		catch (Exception)
		{
			return null;
		}
	}
}