using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Storet.API.Models;

namespace Storet.API.Data;

public class StoretDbContext : DbContext
{
	public StoretDbContext (DbContextOptions<StoretDbContext> options) : base (options) {}

	public DbSet<Category> Categories;

	protected override void OnModelCreating (ModelBuilder modelBuilder)
	{
		base.OnModelCreating (modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly (Assembly.GetExecutingAssembly());
	}
}